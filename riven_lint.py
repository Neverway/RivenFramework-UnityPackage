#!/usr/bin/env python3
from __future__ import annotations

import argparse
import bisect
import csv
import fnmatch
import io
import json
import os
import re
import shlex
import subprocess
import sys
import textwrap
import threading
import queue
from dataclasses import dataclass, field

VERSION = "0.1.0"
CONFIG_NAME = ".rivenlint.json"
BANNER_WIDTH = 120
PLACEHOLDER_DESC = "what this is for, in a line or two."

RULES = {
    "H001": ("No standard header", "error",
             "This script has no Neverway header."),
    "H002": ("Old-style header", "warning",
             "This script still has the old 'Written by' header."),
    "H003": ("Header description empty", "error",
             "The header has no description yet."),
    "H004": ("Header author empty", "warning",
             "The header has no author."),
    "R001": ("Missing standard region", "warning",
             "A class is missing one of the standard regions (Variables: Inspector, Reference, External, Internal. "
             "Functions: Inherited, External, Internal). Empty regions stay in on purpose."),
    "R002": ("Member in the wrong region", "info",
             "This variable or function is sitting in the wrong region."),
    "D001": ("External method has no summary", "error",
             "Public methods need a /// <summary> so other developers know how to use them."),
    "D002": ("Summary missing <param>/<returns>", "info",
             "This method has a summary but is missing <param> or <returns> tags."),
    "T001": ("No tooltip or comment", "info",
             "Inspector, Reference and External variables should have a [Tooltip] or a comment."),
    "C001": ("FindObjectOfType-style call", "warning",
             "Avoid Find...ObjectOfType in framework code. Cache a reference or use a game instance module instead."),
    "C002": ("Legacy namespace import", "warning",
             "'using Neverway.Framework...' is the legacy framework. Use RivenFramework instead."),
    "N001": ("No namespace", "off",
             "This type is not inside a namespace (off until we decide on a namespace policy)."),
}
SEVERITIES = ["error", "warning", "info", "off"]
SEV_ORDER = {"error": 0, "warning": 1, "info": 2}

DEFAULT_CONFIG = {
    "author": "",
    "year": "2027",
    "editor": "",
    "ignore": [
        "Library", "Temp", "obj", "Logs", ".git", "node_modules", "Plugins", "_PoiyomiShaders",
        "TextMesh Pro", "Fantasy Skybox FREE", "_REMOVE_", "RivenFramework", "*.Designer.cs",
    ],
    "severity": {},
}

REQUIRED_REGIONS = {
    "Variables": ["Inspector", "Reference", "External", "Internal"],
    "Functions": ["Inherited", "External", "Internal"],
}
PARENTS = {"variables", "functions"}
SUBS = {"inspector", "reference", "external", "internal", "inherited"}

UNITY_MESSAGES = set("""
Awake Start Update FixedUpdate LateUpdate OnEnable OnDisable OnDestroy Reset OnValidate OnGUI
OnDrawGizmos OnDrawGizmosSelected OnApplicationQuit OnApplicationFocus OnApplicationPause
OnTriggerEnter OnTriggerExit OnTriggerStay OnTriggerEnter2D OnTriggerExit2D OnTriggerStay2D
OnCollisionEnter OnCollisionExit OnCollisionStay OnCollisionEnter2D OnCollisionExit2D OnCollisionStay2D
OnMouseDown OnMouseUp OnMouseEnter OnMouseExit OnMouseOver OnMouseDrag OnMouseUpAsButton
OnBecameVisible OnBecameInvisible OnAnimatorMove OnAnimatorIK OnRenderObject OnWillRenderObject
OnPreCull OnPreRender OnPostRender OnRenderImage OnJointBreak OnJointBreak2D OnParticleCollision
OnParticleTrigger OnParticleSystemStopped OnTransformChildrenChanged OnTransformParentChanged
OnBeforeTransformParentChanged OnRectTransformDimensionsChange OnAudioFilterRead
OnControllerColliderHit OnSceneGUI OnInspectorGUI OnBeforeSerialize OnAfterDeserialize
""".split())
REFERENCE_TYPES = {
    "Transform", "GameObject", "Rigidbody", "Rigidbody2D", "Collider", "Collider2D", "BoxCollider",
    "SphereCollider", "CapsuleCollider", "MeshCollider", "Camera", "Animator", "AudioSource", "AudioClip",
    "Renderer", "MeshRenderer", "SkinnedMeshRenderer", "SpriteRenderer", "Light", "Canvas", "RectTransform",
    "Image", "Text", "TMP_Text", "TextMeshProUGUI", "Button", "Slider", "Toggle", "Component",
    "MonoBehaviour", "ScriptableObject", "UnityEvent", "Material", "Mesh", "Sprite", "Texture",
    "Texture2D", "ParticleSystem", "NavMeshAgent", "CharacterController", "Joint", "LogicInput",
    "LogicOutput", "SceneReference", "VisualElement", "UIDocument",
}
SKIP_DOC_ATTRS = {"ContextMenu", "MenuItem", "RuntimeInitializeOnLoadMethod", "InitializeOnLoadMethod"}
MODIFIERS = {
    "public", "private", "protected", "internal", "static", "readonly", "const", "volatile", "new",
    "override", "virtual", "abstract", "sealed", "async", "extern", "unsafe", "partial", "event",
    "implicit", "explicit", "required",
}


def load_config(root):
    cfg = json.loads(json.dumps(DEFAULT_CONFIG))
    path = os.path.join(root, CONFIG_NAME) if root else ""
    if path and os.path.isfile(path):
        try:
            with open(path, "r", encoding="utf-8") as f:
                data = json.load(f)
            for k, v in data.items():
                if k in cfg:
                    cfg[k] = v
        except (OSError, ValueError):
            pass
    return cfg


def save_config(root, cfg):
    with open(os.path.join(root, CONFIG_NAME), "w", encoding="utf-8") as f:
        json.dump(cfg, f, indent=4)
        f.write("\n")


def severity_of(rule, cfg):
    sev = cfg.get("severity", {}).get(rule)
    if sev in SEVERITIES:
        return sev
    return RULES[rule][1]


@dataclass
class Issue:
    rule: str
    severity: str
    path: str
    rel: str
    line: int
    message: str
    symbol: str = ""
    fix: str = ""
    data: dict = field(default_factory=dict)

    @property
    def key(self):
        return (self.rel, self.rule, self.symbol)


def read_source(path):
    with open(path, "rb") as f:
        raw = f.read()
    bom = raw.startswith(b"\xef\xbb\xbf")
    if bom:
        raw = raw[3:]
    text = raw.decode("utf-8", errors="replace")
    newline = "\r\n" if "\r\n" in text else "\n"
    return text.replace("\r\n", "\n").replace("\r", "\n"), newline, bom


def encode_source(text, newline, bom):
    data = text.replace("\n", newline).encode("utf-8")
    return (b"\xef\xbb\xbf" + data) if bom else data


def write_bytes_atomic(path, data):
    tmp = path + ".rivenlint.tmp"
    with open(tmp, "wb") as f:
        f.write(data)
    os.replace(tmp, path)


_CHAR_RE = re.compile(r"'(?:\\.|[^\\'\n])'")


def _blank(s):
    return re.sub(r"[^\n]", " ", s)


def mask_source(text):
    out = []
    n = len(text)
    i = 0
    line = 0
    has_code = False
    trailing = set()
    while i < n:
        c = text[i]
        if c == "\n":
            out.append("\n")
            line += 1
            has_code = False
            i += 1
            continue
        if c == "#" and not has_code:
            j = text.find("\n", i)
            j = n if j == -1 else j
            out.append(" " * (j - i))
            i = j
            continue
        if c == "/" and text.startswith("//", i):
            j = text.find("\n", i)
            j = n if j == -1 else j
            if has_code:
                trailing.add(line)
            out.append(" " * (j - i))
            i = j
            continue
        if c == "/" and text.startswith("/*", i):
            j = text.find("*/", i + 2)
            j = n if j == -1 else j + 2
            seg = text[i:j]
            if has_code:
                trailing.add(line)
            out.append(_blank(seg))
            line += seg.count("\n")
            if "\n" in seg:
                has_code = False
            i = j
            continue
        if c == '"':
            verbatim = (i > 0 and text[i - 1] == "@") or (i > 1 and text[i - 2:i] in ("$@", "@$"))
            j = i + 1
            if verbatim:
                while j < n:
                    if text[j] == '"':
                        if text.startswith('""', j):
                            j += 2
                            continue
                        break
                    j += 1
            else:
                while j < n and text[j] != '"' and text[j] != "\n":
                    if text[j] == "\\":
                        j += 1
                    j += 1
            j = min(j, n - 1)
            seg = text[i:j + 1]
            out.append('"' + _blank(seg[1:-1]) + '"' if len(seg) >= 2 else seg)
            line += seg.count("\n")
            has_code = True
            i = j + 1
            continue
        if c == "'":
            m = _CHAR_RE.match(text, i)
            if m:
                out.append("'" + " " * (m.end() - i - 2) + "'")
                has_code = True
                i = m.end()
                continue
        out.append(c)
        if not c.isspace():
            has_code = True
        i += 1
    return "".join(out), trailing


def match_braces(masked):
    stack = []
    match = {}
    for i, ch in enumerate(masked):
        if ch == "{":
            stack.append(i)
        elif ch == "}" and stack:
            match[stack.pop()] = i
    return match


def split_top(s, sep=","):
    parts, depth, cur = [], 0, []
    for k, ch in enumerate(s):
        if ch in "([{<":
            depth += 1
        elif ch in ")]}>":
            if ch == ">" and k > 0 and s[k - 1] == "=":
                pass
            else:
                depth = max(0, depth - 1)
        if ch == sep and depth == 0:
            parts.append("".join(cur))
            cur = []
        else:
            cur.append(ch)
    parts.append("".join(cur))
    return parts


def strip_attrs(head):
    i = 0
    names = []
    while True:
        m = re.match(r"\s*\[", head[i:])
        if not m:
            break
        j = i + m.end() - 1
        depth = 0
        k = j
        while k < len(head):
            if head[k] == "[":
                depth += 1
            elif head[k] == "]":
                depth -= 1
                if depth == 0:
                    break
            k += 1
        inner = head[j + 1:k]
        for part in split_top(inner, ","):
            nm = re.match(r"\s*(?:\w+\s*:\s*)?([A-Za-z_][\w.]*)", part)
            if nm:
                name = nm.group(1).split(".")[-1]
                if name.endswith("Attribute") and len(name) > 9:
                    name = name[:-9]
                names.append(name)
        i = k + 1
    return names, head[i:]


def top_level_index(s, ch):
    depth = 0
    for k, c in enumerate(s):
        if c == ch and ch == "(" and depth == 0:
            return k
        if c in "([<":
            depth += 1
        elif c in ")]>":
            if c == ">" and k > 0 and s[k - 1] == "=":
                continue
            depth = max(0, depth - 1)
        elif c == ch and depth == 0:
            if ch == "=":
                prev = s[k - 1] if k > 0 else ""
                nxt = s[k + 1] if k + 1 < len(s) else ""
                if nxt in "=>" or prev in "=!<>":
                    continue
            return k
    return -1


_NEW_FIRST = re.compile(r"^//=+\(\s*Neverway\s+(\d{4})\s*\)=+//\s*$")
_OLD_FIRST = re.compile(r"^//=+\s*\(\s*Neverway\s+(\d{4})\s*\)\s*(?:Written by\s+(.*?))?\s*=+\s*$")
_CLOSE_NEW = re.compile(r"^//=+//\s*$")
_CLOSE_OLD = re.compile(r"^//=+\s*$")
_LABEL = re.compile(r"^//\s*(Author|Contributors|Description)\s*:?\s*$", re.I)


def _line_text(l):
    return re.sub(r"^//\s?", "", l).strip()


def find_header(lines):
    s = 0
    while s < len(lines) and not lines[s].strip():
        s += 1
    if s >= len(lines):
        return None
    first = lines[s].strip()
    m = _NEW_FIRST.match(first)
    if m:
        e = None
        for k in range(s + 1, min(len(lines), s + 80)):
            if _CLOSE_NEW.match(lines[k].strip()):
                e = k
                break
            if not lines[k].strip().startswith("//"):
                break
        if e is None:
            return None
        sections = {"author": [], "contributors": [], "description": []}
        has_desc_section = False
        desc_line = s
        cur = None
        for k in range(s + 1, e):
            lm = _LABEL.match(lines[k].strip())
            if lm:
                cur = lm.group(1).lower()
                if cur == "description":
                    has_desc_section = True
                    desc_line = k
                continue
            txt = _line_text(lines[k].strip())
            if cur and txt:
                sections[cur].append(txt)
        desc = " ".join(sections["description"]).strip()
        if desc.lower().strip(".") == PLACEHOLDER_DESC.strip("."):
            desc = ""
        return {"style": "new", "start": s, "end": e, "year": m.group(1),
                "author": " ".join(sections["author"]).strip(),
                "contributors": sections["contributors"], "description": desc,
                "has_desc_section": has_desc_section, "desc_line": desc_line}
    m = _OLD_FIRST.match(first)
    if m:
        e = None
        for k in range(s + 1, min(len(lines), s + 40)):
            if _CLOSE_OLD.match(lines[k].strip()):
                e = k
                break
            if not lines[k].strip().startswith("//"):
                break
        if e is None:
            return None
        purpose, notes, cur = [], [], None
        for k in range(s + 1, e):
            t = _line_text(lines[k].strip())
            pm = re.match(r"(?i)^purpose\s*:\s*(.*)$", t)
            nm = re.match(r"(?i)^notes?\s*:\s*(.*)$", t)
            if pm:
                cur = purpose
                if pm.group(1):
                    purpose.append(pm.group(1))
            elif nm:
                cur = notes
                if nm.group(1):
                    notes.append(nm.group(1))
            elif cur is not None and t:
                cur.append(t)
        desc = " ".join(purpose + notes).strip()
        return {"style": "old", "start": s, "end": e, "year": m.group(1),
                "author": (m.group(2) or "").strip(), "contributors": [], "description": desc,
                "has_desc_section": True, "desc_line": s}
    return None


def build_header(year, author, contributors, description, width=BANNER_WIDTH):
    first = "//" + "=" * 42 + "( Neverway %s )" % year
    first += "=" * max(3, width - len(first) - 2) + "//"
    out = [first, "// Author"]
    out += ["//  " + a for a in author.splitlines() if a.strip()] or ["//"]
    out += ["//", "// Contributors"]
    out += ["//  " + c for c in contributors if c.strip()] or ["//"]
    out += ["//", "// Description"]
    desc_lines = []
    for para in description.splitlines():
        if para.strip():
            desc_lines += textwrap.wrap(para.strip(), width=width - 6) or [""]
    out += ["//  " + d for d in desc_lines] or ["//"]
    out.append("//" + "=" * (width - 4) + "//")
    return out


def apply_header(text, year, author, contributors, description):
    lines = text.split("\n")
    h = find_header(lines)
    new = build_header(h["year"] if h and h["style"] == "new" else year, author, contributors, description)
    if h:
        lines[h["start"]:h["end"] + 1] = new
        after = h["start"] + len(new)
    else:
        lines[0:0] = new
        after = len(new)
    if after >= len(lines) or lines[after].strip():
        lines.insert(after, "")
    return "\n".join(lines)


_TYPE_RE = re.compile(r"\b(class|struct|interface|enum|record)\s+([A-Za-z_]\w*)")


def find_top_types(masked, match):
    types = []

    def scan(a, b):
        s = a
        i = a
        while i < b:
            c = masked[i]
            if c == "{":
                head = masked[s:i]
                close = match.get(i, b)
                if re.search(r"\bnamespace\b", head):
                    scan(i + 1, close)
                else:
                    m = _TYPE_RE.search(head)
                    if m:
                        types.append({"kind": m.group(1), "name": m.group(2), "open": i, "close": close,
                                      "head": head, "head_start": s})
                i = close + 1
                s = i
                continue
            if c == ";":
                s = i + 1
            i += 1

    scan(0, len(masked))
    return types


def _bases_of(t):
    m = re.search(r"\b(?:class|struct|interface|record)\s+\w+(?:\s*<[^>{]*>)?\s*:\s*([^{]+)", t["head"])
    if not m:
        return []
    txt = re.split(r"\bwhere\b", m.group(1))[0]
    return [b.strip() for b in split_top(txt, ",") if b.strip()]


def parse_members(masked, a, b, match):
    members = []
    i = a
    s = a
    paren = 0
    bracket = 0
    n = b
    while i < n:
        c = masked[i]
        if c == "(":
            paren += 1
        elif c == ")":
            paren = max(0, paren - 1)
        elif c == "[":
            bracket += 1
        elif c == "]":
            bracket = max(0, bracket - 1)
        elif paren == 0 and bracket == 0:
            if c == "{":
                head = masked[s:i]
                close = match.get(i, n)
                _attrs, rest = strip_attrs(head)
                if top_level_index(rest, "=") != -1 and "=>" not in rest.split("=")[0] + "=" and "(" not in rest.split("=")[0]:
                    i = close + 1
                    continue
                if top_level_index(rest, "=") != -1 and "(" in rest and rest.index("(") > top_level_index(rest, "="):
                    i = close + 1
                    continue
                members.append({"head": head, "start": s, "end": close, "block": (i, close), "arrow": False})
                i = close + 1
                nxt = re.match(r"\s*=(?!>)", masked[i:i + 8])
                if nxt:
                    j = masked.find(";", i)
                    i = (j if j != -1 else n - 1) + 1
                s = i
                continue
            if c == ";":
                members.append({"head": masked[s:i], "start": s, "end": i, "block": None, "arrow": False})
                s = i + 1
            elif c == "=" and masked[i + 1:i + 2] == ">":
                head = masked[s:i]
                j = i + 2
                depth = 0
                while j < n:
                    ch = masked[j]
                    if ch in "({[":
                        depth += 1
                    elif ch in ")}]":
                        depth -= 1
                    elif ch == ";" and depth <= 0:
                        break
                    j += 1
                members.append({"head": head, "start": s, "end": j, "block": None, "arrow": True})
                i = j + 1
                s = i
                continue
        i += 1
    return members


def classify_member(m, class_name):
    attrs, rest = strip_attrs(m["head"])
    rest_s = rest.strip()
    if not rest_s:
        return None
    if re.search(r"\b(class|struct|interface|enum|record|delegate|namespace|using)\b", rest_s.split("(")[0]):
        return None
    eq = top_level_index(rest_s, "=")
    par = top_level_index(rest_s, "(")
    info = {"attrs": attrs, "raw": rest_s}
    if par != -1 and (eq == -1 or par < eq):
        pre = rest_s[:par].strip()
        pre_nogen = re.sub(r"<[^<>]*(?:<[^<>]*>[^<>]*)*>\s*$", "", pre).strip()
        toks = [t for t in split_top(pre_nogen.replace("\n", " "), " ") if t.strip()]
        toks = [t.strip() for t in toks if t.strip()]
        mods = {t for t in toks if t in MODIFIERS}
        nonmods = [t for t in toks if t not in MODIFIERS]
        if not nonmods:
            return None
        name = nonmods[-1]
        explicit = "." in name
        name = name.split(".")[-1]
        if name == "operator" or "operator" in nonmods:
            return None
        ret = " ".join(nonmods[:-1])
        close = _matching_paren(rest_s, par)
        params = rest_s[par + 1:close] if close != -1 else ""
        info.update(kind="ctor" if not ret and name == class_name else "method", name=name, mods=mods,
                    ret=ret, params=params, explicit=explicit)
        if not ret and name != class_name:
            return None
        return info
    decl = rest_s[:eq] if eq != -1 else rest_s
    if m.get("arrow"):
        decl = rest_s
    decl = decl.strip()
    first = split_top(decl, ",")[0].strip()
    first_nogen = first
    toks = [t.strip() for t in split_top(first_nogen.replace("\n", " "), " ") if t.strip()]
    mods = {t for t in toks if t in MODIFIERS}
    nonmods = [t for t in toks if t not in MODIFIERS]
    if len(nonmods) < 2 and "event" not in mods:
        return None
    name = nonmods[-1] if nonmods else ""
    if not re.match(r"^[A-Za-z_]\w*$", name):
        return None
    typ = " ".join(nonmods[:-1])
    kind = "field"
    if "event" in mods:
        kind = "event"
    elif m.get("block") or m.get("arrow"):
        kind = "property"
    if name == "this":
        return None
    info.update(kind=kind, name=name, mods=mods, type=typ)
    return info


def _matching_paren(s, start):
    depth = 0
    for k in range(start, len(s)):
        if s[k] == "(":
            depth += 1
        elif s[k] == ")":
            depth -= 1
            if depth == 0:
                return k
    return -1


def parse_params(params):
    result = []
    for p in split_top(params, ","):
        p = p.strip()
        if not p:
            continue
        _a, p = strip_attrs(p)
        eq = top_level_index(p, "=")
        if eq != -1:
            p = p[:eq]
        toks = [t for t in p.split() if t not in ("ref", "out", "in", "params", "this", "scoped")]
        if toks and re.match(r"^[A-Za-z_]\w*$", toks[-1]):
            result.append(toks[-1])
    return result


def region_stacks(lines):
    stacks = []
    stack = []
    for l in lines:
        st = l.strip()
        if st.startswith("#region"):
            name = st[7:].strip()
            m = re.search(r"\(\s*([^)]+?)\s*\)", name)
            nm = m.group(1) if m else name.strip("=-/ \t")
            stack.append(nm.strip())
        elif st.startswith("#endregion"):
            if stack:
                stack.pop()
        stacks.append(tuple(stack))
    return stacks


def section_of(stack):
    sub = parent = None
    for nm in reversed(stack):
        low = nm.lower()
        if sub is None and low in SUBS:
            sub = nm.capitalize()
            continue
        if sub is not None and low in PARENTS:
            parent = nm.capitalize()
            break
        if sub is None and low in PARENTS:
            parent = nm.capitalize()
            break
    return parent, sub


_IGNORE_FILE = re.compile(r"//\s*riven-lint:\s*ignore-file", re.I)
_IGNORE_RULES = re.compile(r"//\s*riven-lint:\s*ignore\s+([A-Z0-9, ]+)", re.I)
_FIND_RE = re.compile(r"\bFind(?:First)?Object(?:s)?(?:Of|By)Type\w*")
_BANNER_ONLY = re.compile(r"^//[=\-\s*/_.#~]*$")


def analyze_text(text, rel, path, cfg):
    issues = []
    if _IGNORE_FILE.search(text):
        return issues
    ignored = set()
    for m in _IGNORE_RULES.finditer(text):
        ignored |= {r.strip().upper() for r in m.group(1).split(",") if r.strip()}
    lines = text.split("\n")
    if any("<auto-generated" in l for l in lines[:6]):
        return issues

    def add(rule, line, message, symbol="", fix="", data=None):
        if rule in ignored:
            return
        sev = severity_of(rule, cfg)
        if sev == "off":
            return
        issues.append(Issue(rule, sev, path, rel, line, message, symbol, fix, data or {}))

    masked, trailing = mask_source(text)
    match = match_braces(masked)
    line_starts = [0]
    for m in re.finditer("\n", text):
        line_starts.append(m.end())

    def line_of(idx):
        return bisect.bisect_right(line_starts, idx) - 1

    stacks = region_stacks(lines)
    types = [t for t in find_top_types(masked, match)]

    h = find_header(lines)
    author_default = cfg.get("author", "")
    if h is None:
        add("H001", 1, "No standard header", "header", "header",
            {"author": author_default, "contributors": [], "description": "", "year": cfg.get("year", "2027")})
    elif h["style"] == "old":
        add("H002", h["start"] + 1, "Old 'Written by' header, needs the new format",
            "header", "header",
            {"author": h["author"] or author_default, "contributors": [], "description": h["description"],
             "year": cfg.get("year", "2027")})
    else:
        data = {"author": h["author"] or author_default, "contributors": h["contributors"],
                "description": h["description"], "year": h["year"]}
        if not h["description"]:
            msg = "The header description is empty" if h["has_desc_section"] else "The header has no Description section"
            add("H003", h["desc_line"] + 1, msg, "header-desc", "header", data)
        if not h["author"]:
            add("H004", h["start"] + 2, "The header author is empty", "header-author", "header", data)

    for k, l in enumerate(lines):
        if re.match(r"\s*using\s+Neverway\.Framework\b", l):
            add("C002", k + 1, "Legacy import: " + l.strip(), "using:" + l.strip())
    for m in _FIND_RE.finditer(masked):
        ln = line_of(m.start())
        add("C001", ln + 1, "Avoid " + m.group(0) + "(), cache a reference or use a game instance module",
            "find:%d:%s" % (ln, m.group(0)))

    for t in types:
        if t["kind"] in ("interface", "enum"):
            continue
        cname = t["name"]
        open_line, close_line = line_of(t["open"]), line_of(t["close"])
        bases = _bases_of(t)
        has_iface = any(re.match(r"^I[A-Z]", b.split("<")[0].strip()) for b in bases)
        if not re.search(r"\bnamespace\b", masked[:t["head_start"]]):
            add("N001", open_line + 1, "Type '%s' is not inside a namespace" % cname, "ns:" + cname)

        raw_members = parse_members(masked, t["open"] + 1, t["close"], match)
        members = []
        for rm in raw_members:
            info = classify_member(rm, cname)
            if info is None:
                continue
            first_nonspace = re.search(r"\S", masked[rm["start"]:rm["end"] + 1])
            sidx = rm["start"] + (first_nonspace.start() if first_nonspace else 0)
            info["start_line"] = line_of(sidx)
            info["end_line"] = line_of(rm["end"])
            head_rel = rm["head"]
            par = head_rel.find("(")
            name_pos = rm["start"] + (par if info["kind"] == "method" and par != -1 else max(len(head_rel) - 1, 0))
            info["name_line"] = line_of(name_pos) if info["kind"] == "method" else info["start_line"]
            members.append(info)

        present = set()
        for ln in range(open_line, close_line + 1):
            st = lines[ln].strip()
            if st.startswith("#region"):
                stk = stacks[ln]
                nm = stk[-1].lower() if stk else ""
                par_nm = stk[-2].lower() if len(stk) > 1 else ""
                if nm in PARENTS:
                    present.add((nm, None))
                elif nm in SUBS:
                    present.add((par_nm, nm))
        if members:
            missing = []
            for par_nm, subs in REQUIRED_REGIONS.items():
                if (par_nm.lower(), None) not in present:
                    missing.append(par_nm)
                for sub in subs:
                    if (par_nm.lower(), sub.lower()) not in present:
                        missing.append("%s/%s" % (par_nm, sub))
            if missing:
                add("R001", open_line + 1, "Class %s is missing region(s): %s" % (cname, ", ".join(missing)),
                    "regions:" + cname)
        class_has_regions = any(s for s in present)

        for mem in members:
            kind, name = mem["kind"], mem["name"]
            mods, attrs = mem["mods"], mem["attrs"]
            stk = stacks[mem["start_line"]]
            parent, sub = section_of(stk)
            where = "%s/%s" % (parent or "?", sub) if sub else "no region"
            sym = "%s.%s" % (cname, name)

            def comment_above(sl):
                k = sl - 1
                if k < 0:
                    return False
                s2 = lines[k].strip()
                return s2.startswith("//") and not _BANNER_ONLY.match(s2)

            def docs_above(sl):
                k = sl - 1
                block = []
                while k >= 0 and lines[k].strip().startswith("///"):
                    block.insert(0, lines[k].strip()[3:].strip())
                    k -= 1
                return block, k + 1

            if kind in ("field", "property", "event"):
                if "const" in mods or ("static" in mods and "readonly" in mods):
                    continue
                public = "public" in mods
                serialized = "SerializeField" in attrs or "SerializeReference" in attrs
                hidden = "HideInInspector" in attrs or "NonSerialized" in attrs
                typ = mem.get("type", "")
                base_typ = re.split(r"[<\[?]", typ)[0].split(".")[-1].strip()
                if kind == "event":
                    continue
                if kind == "property":
                    expected = {"External"} if public else {"Internal"}
                elif public and "static" not in mods:
                    if hidden:
                        expected = {"External"}
                    elif base_typ in REFERENCE_TYPES:
                        expected = {"Reference"}
                    else:
                        expected = {"Inspector", "Reference"}
                elif serialized:
                    expected = {"Reference"} if base_typ in REFERENCE_TYPES else {"Inspector", "Reference"}
                elif public:
                    expected = {"External"}
                elif "protected" in mods:
                    expected = {"Internal", "External"}
                else:
                    expected = {"Internal"}
                if parent and parent != "Variables" or (sub in ("Inherited",) and parent != "Variables"):
                    add("R002", mem["name_line"] + 1,
                        "Variable '%s' is in %s (expected Variables/%s)" % (name, where, "/".join(sorted(expected))),
                        "place:" + sym)
                elif sub and sub not in expected and parent == "Variables":
                    add("R002", mem["name_line"] + 1,
                        "Variable '%s' is in %s (expected Variables/%s)" % (name, where, "/".join(sorted(expected))),
                        "place:" + sym)
                elif not sub and class_has_regions:
                    add("R002", mem["name_line"] + 1,
                        "Variable '%s' is not inside a region (expected Variables/%s)" % (name, "/".join(sorted(expected))),
                        "place:" + sym)
                if kind == "field" and "static" not in mods and (public or serialized):
                    has_tip = ("Tooltip" in attrs or comment_above(mem["start_line"])
                               or bool(docs_above(mem["start_line"])[0])
                               or mem["end_line"] in trailing or mem["start_line"] in trailing)
                    if not has_tip:
                        section = "External" if hidden else ("Reference" if base_typ in REFERENCE_TYPES else "Inspector")
                        add("T001", mem["name_line"] + 1, "'%s' has no tooltip or comment" % name, "tip:" + sym,
                            "tooltip", {"name": name, "line": mem["start_line"] + 1,
                                        "as_comment": section == "External"})
                continue

            if kind == "method":
                is_unity = name in UNITY_MESSAGES
                explicit = mem.get("explicit", False)
                override = "override" in mods
                public = "public" in mods
                inherited = is_unity or override or explicit
                if inherited:
                    expected = {"Inherited"}
                elif public:
                    expected = {"External", "Inherited"} if has_iface else {"External"}
                elif "protected" in mods:
                    expected = {"Internal", "External"}
                else:
                    expected = {"Internal"}
                params = parse_params(mem["params"])
                msym = "%s.%s(%s)" % (cname, name, ",".join(params))
                if parent != "Functions" and (sub or class_has_regions):
                    add("R002", mem["name_line"] + 1,
                        "Method '%s' is in %s (expected Functions/%s)" % (name, where, "/".join(sorted(expected))),
                        "place:" + msym)
                elif sub and sub not in expected and parent == "Functions":
                    add("R002", mem["name_line"] + 1,
                        "Method '%s' is in %s (expected Functions/%s)" % (name, where, "/".join(sorted(expected))),
                        "place:" + msym)
                elif not sub and class_has_regions:
                    add("R002", mem["name_line"] + 1,
                        "Method '%s' is not inside a region (expected Functions/%s)" % (name, "/".join(sorted(expected))),
                        "place:" + msym)

                needs_docs = (public and not inherited and not (set(attrs) & SKIP_DOC_ATTRS)
                              and not (has_iface and sub == "Inherited"))
                if needs_docs:
                    block, doc_start = docs_above(mem["start_line"])
                    doc_text = " ".join(block)
                    has_summary = bool(re.search(r"<summary>\s*\S", doc_text)) or "<inheritdoc" in doc_text
                    returns = mem["ret"].strip() not in ("void", "")
                    data = {"class": cname, "name": name, "params": params, "returns": returns,
                            "line": mem["start_line"] + 1, "doc_start": doc_start + 1 if block else None,
                            "doc_end": mem["start_line"] if block else None}
                    if not has_summary:
                        add("D001", mem["name_line"] + 1, "Public method '%s' has no summary" % name,
                            "doc:" + msym, "summary", dict(data, mode="new"))
                    elif "<inheritdoc" not in doc_text:
                        miss = [p for p in params if not re.search(r'<param\s+name="%s"' % re.escape(p), doc_text)]
                        need_ret = returns and "<returns>" not in doc_text
                        if miss or need_ret:
                            parts = ["<param> for " + ", ".join(miss)] if miss else []
                            if need_ret:
                                parts.append("<returns>")
                            add("D002", mem["name_line"] + 1,
                                "Method '%s' is missing " % name + " and ".join(parts), "docp:" + msym,
                                "summary", dict(data, mode="extend", missing_params=miss, need_returns=need_ret))
    return issues


def _xml(s):
    return s.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def _indent_of(line):
    return line[:len(line) - len(line.lstrip())]


def apply_summary(text, issue, summary, param_docs, returns_doc):
    lines = text.split("\n")
    d = issue.data
    at = d["line"] - 1
    ind = _indent_of(lines[at])
    if d.get("mode") == "extend" and d.get("doc_end") is not None:
        end = d["doc_end"]
        add = []
        for p in d.get("missing_params", []):
            add.append('%s/// <param name="%s">%s</param>' % (ind, p, _xml(param_docs.get(p, "").strip())))
        if d.get("need_returns"):
            add.append("%s/// <returns>%s</returns>" % (ind, _xml(returns_doc.strip())))
        lines[end:end] = add
        return "\n".join(lines)
    block = [ind + "/// <summary>"]
    for ln in (textwrap.wrap(_xml(summary.strip()), width=100) or [""]):
        block.append(ind + "/// " + ln)
    block.append(ind + "/// </summary>")
    for p in d.get("params", []):
        block.append('%s/// <param name="%s">%s</param>' % (ind, p, _xml(param_docs.get(p, "").strip())))
    if d.get("returns"):
        block.append("%s/// <returns>%s</returns>" % (ind, _xml(returns_doc.strip())))
    lines[at:at] = block
    return "\n".join(lines)


def apply_tooltip(text, issue, tip, as_comment):
    lines = text.split("\n")
    at = issue.data["line"] - 1
    ind = _indent_of(lines[at])
    tip = tip.strip()
    if as_comment:
        new = ind + "// " + tip
    else:
        new = ind + '[Tooltip("%s")]' % tip.replace("\\", "\\\\").replace('"', '\\"')
    lines.insert(at, new)
    return "\n".join(lines)


def apply_fix(issue, values):
    text, newline, bom = read_source(issue.path)
    old = encode_source(text, newline, bom)
    if issue.fix == "header":
        contributors = [c.strip() for c in re.split(r"[,\n]", values.get("contributors", "")) if c.strip()]
        new = apply_header(text, issue.data.get("year", "2027"), values.get("author", "").strip(),
                           contributors, values.get("description", "").strip())
    elif issue.fix == "summary":
        new = apply_summary(text, issue, values.get("summary", ""), values.get("params", {}),
                            values.get("returns", ""))
    elif issue.fix == "tooltip":
        new = apply_tooltip(text, issue, values.get("tip", ""), values.get("as_comment", False))
    else:
        raise ValueError("This issue can't be fixed automatically")
    data = encode_source(new, newline, bom)
    write_bytes_atomic(issue.path, data)
    return old, data


def is_ignored(rel, patterns):
    segs = rel.split("/")
    for pat in patterns:
        if any(ch in pat for ch in "*?[/"):
            if fnmatch.fnmatch(rel, pat) or fnmatch.fnmatch(rel, "*/" + pat) or fnmatch.fnmatch(segs[-1], pat):
                return True
        elif pat in segs:
            return True
    return False


def analyze_file(path, root, cfg):
    rel = os.path.relpath(path, root).replace(os.sep, "/")
    try:
        text, _nl, _bom = read_source(path)
    except OSError as e:
        return [Issue("H001", "error", path, rel, 1, "Could not read file: %s" % e, "read-error")]
    try:
        return analyze_text(text, rel, path, cfg)
    except Exception as e:
        return [Issue("H001", "warning", path, rel, 1, "Checker could not parse this file (%s)" % e, "parse-error")]


def list_scripts(root, cfg):
    out = []
    root = os.path.abspath(root)
    if os.path.isfile(root):
        return [root]
    for dp, dn, fn in os.walk(root):
        rel_dir = os.path.relpath(dp, root).replace(os.sep, "/")
        dn[:] = [d for d in dn if not is_ignored(("" if rel_dir == "." else rel_dir + "/") + d, cfg["ignore"])]
        for f in fn:
            if f.endswith(".cs"):
                rel = ("" if rel_dir == "." else rel_dir + "/") + f
                if not is_ignored(rel, cfg["ignore"]):
                    out.append(os.path.join(dp, f))
    return sorted(out)


def scan_folder(root, cfg, progress=None):
    base = os.path.abspath(root)
    if os.path.isfile(base):
        base = os.path.dirname(base)
    files = list_scripts(root, cfg)
    issues = []
    for k, p in enumerate(files):
        issues += analyze_file(p, base, cfg)
        if progress and (k % 25 == 0 or k == len(files) - 1):
            progress(k + 1, len(files))
    issues.sort(key=lambda i: (SEV_ORDER[i.severity], i.rel, i.line, i.rule))
    return issues, len(files)


def open_in_editor(cfg, path, line):
    cmd = (cfg.get("editor") or "").strip()
    if cmd:
        quoted = '"%s"' % path if os.name == "nt" else shlex.quote(path)
        subprocess.Popen(cmd.replace("{file}", quoted).replace("{line}", str(line)), shell=True)
        return
    if sys.platform.startswith("win"):
        os.startfile(path)
    elif sys.platform == "darwin":
        subprocess.Popen(["open", path])
    else:
        subprocess.Popen(["xdg-open", path])


def format_report(issues, fmt="text"):
    if fmt == "json":
        return json.dumps([{"severity": i.severity, "rule": i.rule, "file": i.rel, "line": i.line,
                            "message": i.message} for i in issues], indent=2)
    if fmt == "csv":
        buf = io.StringIO()
        w = csv.writer(buf, lineterminator="\n")
        w.writerow(["severity", "rule", "file", "line", "message"])
        for i in issues:
            w.writerow([i.severity, i.rule, i.rel, i.line, i.message])
        return buf.getvalue()
    return "\n".join("%s:%d: %s %s %s" % (i.rel, i.line, i.severity, i.rule, i.message) for i in issues)


def cli_check(args):
    cfg = load_config(args.check if os.path.isdir(args.check) else os.path.dirname(os.path.abspath(args.check)))
    issues, nfiles = scan_folder(args.check, cfg)
    cutoff = SEV_ORDER[args.min_severity]
    shown = [i for i in issues if SEV_ORDER[i.severity] <= cutoff]
    out = format_report(shown, args.format)
    if out:
        print(out)
    counts = {s: sum(1 for i in issues if i.severity == s) for s in ("error", "warning", "info")}
    print("\n%d files checked: %d errors, %d warnings, %d info" % (nfiles, counts["error"], counts["warning"],
                                                                 counts["info"]), file=sys.stderr)
    if counts["error"] or (args.strict and counts["warning"]):
        return 1
    return 0


def run_gui(start_path=None):
    try:
        import tkinter as tk
        from tkinter import ttk, filedialog, messagebox
    except ImportError:
        print("This Python doesn't have tkinter, so the window can't open.\n"
              "  Windows: rerun the Python installer and make sure tcl/tk is ticked.\n"
              "  Debian/Ubuntu: sudo apt install python3-tk\n"
              "You can still run it without a window:  python3 riven_lint.py --check <folder>", file=sys.stderr)
        return 1
    App(tk, ttk, filedialog, messagebox, start_path).run()
    return 0


class App:
    SEV_COLORS = {"error": "#c0392b", "warning": "#b9770e", "info": "#2471a3"}

    def __init__(self, tk, ttk, filedialog, messagebox, start_path=None):
        self.tk, self.ttk, self.filedialog, self.messagebox = tk, ttk, filedialog, messagebox
        self.root_dir = None
        self.cfg = load_config(None)
        self.issues = []
        self.files_scanned = 0
        self.undo_stack = []
        self.sort_col, self.sort_rev = "sev", False
        self.scan_queue = queue.Queue()
        self.shown = {}

        self.win = tk.Tk()
        self.win.title("Riven Lint %s" % VERSION)
        self.win.geometry("1180x720")
        try:
            if sys.platform.startswith("linux"):
                ttk.Style().theme_use("clam")
        except tk.TclError:
            pass

        self.var_err = tk.BooleanVar(value=True)
        self.var_warn = tk.BooleanVar(value=True)
        self.var_info = tk.BooleanVar(value=True)
        self.var_fixable = tk.BooleanVar(value=False)
        self.var_rule = tk.StringVar(value="All rules")
        self.var_search = tk.StringVar()
        self.var_status = tk.StringVar(value="Pick a folder to check (File > Open Folder).")
        self.var_folder = tk.StringVar(value="(no folder)")

        self._build_menu()
        self._build_ui()
        self.win.bind("<Control-o>", lambda e: self.open_folder())
        self.win.bind("<F5>", lambda e: self.rescan())
        self.win.bind("<Control-z>", lambda e: self.undo())
        self.var_search.trace_add("write", lambda *a: self.refresh_list())
        self.win.after(150, self._poll_scan)
        if start_path:
            self.win.after(200, lambda: self.load_folder(start_path))
        elif os.path.isfile(self._last_folder_file()):
            try:
                last = open(self._last_folder_file(), encoding="utf-8").read().strip()
                if last and os.path.isdir(last):
                    self.var_folder.set(last + "   (press F5 to scan)")
                    self.root_dir = last
                    self.cfg = load_config(last)
            except OSError:
                pass

    @staticmethod
    def _last_folder_file():
        base = os.environ.get("APPDATA") if os.name == "nt" else os.path.join(os.path.expanduser("~"), ".config")
        os.makedirs(base or ".", exist_ok=True)
        return os.path.join(base or ".", "riven_lint_last_folder.txt")

    def run(self):
        self.win.mainloop()

    def _build_menu(self):
        tk = self.tk
        mb = tk.Menu(self.win)
        f = tk.Menu(mb, tearoff=0)
        f.add_command(label="Open Folder...", accelerator="Ctrl+O", command=self.open_folder)
        f.add_command(label="Rescan", accelerator="F5", command=self.rescan)
        f.add_separator()
        f.add_command(label="Export Report...", command=self.export_report)
        f.add_separator()
        f.add_command(label="Quit", command=self.win.destroy)
        mb.add_cascade(label="File", menu=f)
        e = tk.Menu(mb, tearoff=0)
        e.add_command(label="Undo Last Fix", accelerator="Ctrl+Z", command=self.undo)
        mb.add_cascade(label="Edit", menu=e)
        v = tk.Menu(mb, tearoff=0)
        v.add_checkbutton(label="Show errors", variable=self.var_err, command=self.refresh_list)
        v.add_checkbutton(label="Show warnings", variable=self.var_warn, command=self.refresh_list)
        v.add_checkbutton(label="Show info", variable=self.var_info, command=self.refresh_list)
        v.add_separator()
        v.add_checkbutton(label="Only issues I can fill in here", variable=self.var_fixable, command=self.refresh_list)
        mb.add_cascade(label="View", menu=v)
        t = tk.Menu(mb, tearoff=0)
        t.add_command(label="Fix Selected...", accelerator="Enter", command=self.fix_selected)
        t.add_command(label="Fix Queue...", command=self.fix_queue)
        t.add_command(label="Open in Editor", command=self.open_selected)
        t.add_separator()
        t.add_command(label="Settings...", command=self.settings_dialog)
        mb.add_cascade(label="Tools", menu=t)
        h = tk.Menu(mb, tearoff=0)
        h.add_command(label="Rules...", command=self.rules_dialog)
        h.add_command(label="About", command=lambda: self.messagebox.showinfo(
            "Riven Lint", "Riven Lint %s\nChecks scripts against the Riven Framework conventions.\n\n"
                          "No window needed: riven_lint.py --check <folder>" % VERSION))
        mb.add_cascade(label="Help", menu=h)
        self.win.config(menu=mb)

    def _build_ui(self):
        tk, ttk = self.tk, self.ttk
        top = ttk.Frame(self.win, padding=(8, 6))
        top.pack(fill="x")
        ttk.Button(top, text="Open Folder...", command=self.open_folder).pack(side="left")
        ttk.Label(top, textvariable=self.var_folder).pack(side="left", padx=8)
        ttk.Button(top, text="Fix Queue...", command=self.fix_queue).pack(side="right")

        flt = ttk.Frame(self.win, padding=(8, 0, 8, 6))
        flt.pack(fill="x")
        self.chk_err = ttk.Checkbutton(flt, text="Errors", variable=self.var_err, command=self.refresh_list)
        self.chk_warn = ttk.Checkbutton(flt, text="Warnings", variable=self.var_warn, command=self.refresh_list)
        self.chk_info = ttk.Checkbutton(flt, text="Info", variable=self.var_info, command=self.refresh_list)
        for w in (self.chk_err, self.chk_warn, self.chk_info):
            w.pack(side="left", padx=(0, 8))
        ttk.Checkbutton(flt, text="Only fixable", variable=self.var_fixable, command=self.refresh_list).pack(
            side="left", padx=(0, 12))
        self.rule_box = ttk.Combobox(flt, textvariable=self.var_rule, state="readonly", width=34,
                                     values=["All rules"] + ["%s  %s" % (k, v[0]) for k, v in RULES.items()])
        self.rule_box.pack(side="left")
        self.rule_box.bind("<<ComboboxSelected>>", lambda e: self.refresh_list())
        ttk.Label(flt, text="  Search:").pack(side="left")
        ttk.Entry(flt, textvariable=self.var_search, width=28).pack(side="left")

        pan = ttk.PanedWindow(self.win, orient="vertical")
        pan.pack(fill="both", expand=True, padx=8)
        box = ttk.Frame(pan)
        cols = ("sev", "file", "line", "rule", "msg")
        self.tree = ttk.Treeview(box, columns=cols, show="headings", selectmode="browse")
        for c, label, w in (("sev", "Severity", 80), ("file", "File", 380), ("line", "Line", 55),
                            ("rule", "Rule", 55), ("msg", "Message", 560)):
            self.tree.heading(c, text=label, command=lambda c=c: self.sort_by(c))
            self.tree.column(c, width=w, anchor="w", stretch=(c == "msg"))
        sb = ttk.Scrollbar(box, orient="vertical", command=self.tree.yview)
        self.tree.configure(yscrollcommand=sb.set)
        self.tree.pack(side="left", fill="both", expand=True)
        sb.pack(side="right", fill="y")
        for sev, col in self.SEV_COLORS.items():
            self.tree.tag_configure(sev, foreground=col)
        self.tree.bind("<<TreeviewSelect>>", lambda e: self.show_preview())
        self.tree.bind("<Double-1>", lambda e: self.fix_selected())
        self.tree.bind("<Return>", lambda e: self.fix_selected())
        self.tree.bind("<Button-3>", self._context_menu)
        pan.add(box, weight=3)

        prev = ttk.Frame(pan)
        self.preview = tk.Text(prev, height=9, wrap="none", font=("Courier", 10), state="disabled", bg="#fafafa",
                               fg="#222222")
        self.preview.tag_configure("hit", background="#ffe9a8")
        self.preview.pack(fill="both", expand=True)
        pan.add(prev, weight=1)

        ttk.Label(self.win, textvariable=self.var_status, anchor="w", padding=(8, 4)).pack(fill="x")

    def open_folder(self):
        p = self.filedialog.askdirectory(title="Pick the folder to check (your Assets folder or the package folder)")
        if p:
            self.load_folder(p)

    def load_folder(self, p):
        self.root_dir = p
        self.cfg = load_config(p)
        self.var_folder.set(p)
        try:
            with open(self._last_folder_file(), "w", encoding="utf-8") as f:
                f.write(p)
        except OSError:
            pass
        self.rescan()

    def rescan(self):
        if not self.root_dir:
            self.open_folder()
            return
        self.var_status.set("Scanning...")
        cfg = self.cfg
        root = self.root_dir

        def work():
            try:
                issues, n = scan_folder(root, cfg, lambda a, b: self.scan_queue.put(("progress", a, b)))
                self.scan_queue.put(("done", issues, n))
            except Exception as e:
                self.scan_queue.put(("error", str(e), 0))

        threading.Thread(target=work, daemon=True).start()

    def _poll_scan(self):
        try:
            while True:
                kind, a, b = self.scan_queue.get_nowait()
                if kind == "progress":
                    self.var_status.set("Scanning, %d of %d files done" % (a, b))
                elif kind == "done":
                    self.issues, self.files_scanned = a, b
                    self.refresh_list()
                else:
                    self.var_status.set("Scan failed: %s" % a)
        except queue.Empty:
            pass
        self.win.after(120, self._poll_scan)

    def rescan_file(self, path):
        base = self.root_dir if os.path.isdir(self.root_dir or "") else os.path.dirname(path)
        self.issues = [i for i in self.issues if i.path != path] + analyze_file(path, base, self.cfg)
        self.issues.sort(key=lambda i: (SEV_ORDER[i.severity], i.rel, i.line, i.rule))
        self.refresh_list()

    def _visible(self):
        sevs = {s for s, v in (("error", self.var_err), ("warning", self.var_warn), ("info", self.var_info)) if v.get()}
        rule = self.var_rule.get().split()[0] if self.var_rule.get() != "All rules" else None
        q = self.var_search.get().strip().lower()
        out = []
        for i in self.issues:
            if i.severity not in sevs:
                continue
            if rule and i.rule != rule:
                continue
            if self.var_fixable.get() and not i.fix:
                continue
            if q and q not in (i.rel + " " + i.message + " " + i.rule).lower():
                continue
            out.append(i)
        keyf = {"sev": lambda i: (SEV_ORDER[i.severity], i.rel, i.line), "file": lambda i: (i.rel, i.line),
                "line": lambda i: (i.line,), "rule": lambda i: (i.rule, i.rel, i.line),
                "msg": lambda i: (i.message.lower(),)}[self.sort_col]
        out.sort(key=keyf, reverse=self.sort_rev)
        return out

    def sort_by(self, col):
        self.sort_rev = (not self.sort_rev) if self.sort_col == col else False
        self.sort_col = col
        self.refresh_list()

    def refresh_list(self):
        sel = self.selected_issue()
        sel_key = sel.key if sel else None
        self.tree.delete(*self.tree.get_children())
        self.shown = {}
        pick = None
        vis = self._visible()
        for i in vis[:5000]:
            iid = self.tree.insert("", "end", values=(i.severity, i.rel, i.line, i.rule + (" *" if i.fix else ""),
                                                      i.message), tags=(i.severity,))
            self.shown[iid] = i
            if sel_key and i.key == sel_key:
                pick = iid
        if pick:
            self.tree.selection_set(pick)
            self.tree.see(pick)
        counts = {s: sum(1 for i in self.issues if i.severity == s) for s in ("error", "warning", "info")}
        self.chk_err.config(text="Errors (%d)" % counts["error"])
        self.chk_warn.config(text="Warnings (%d)" % counts["warning"])
        self.chk_info.config(text="Info (%d)" % counts["info"])
        extra = "  (only the first 5000 are listed)" if len(vis) > 5000 else ""
        self.var_status.set("%d files checked: %d errors, %d warnings, %d info. Showing %d. "
                            "A * next to a rule means you can fix it from here.%s" %
                            (self.files_scanned, counts["error"], counts["warning"], counts["info"], len(vis), extra))
        self.show_preview()

    def selected_issue(self):
        sel = self.tree.selection()
        return self.shown.get(sel[0]) if sel else None

    def show_preview(self):
        i = self.selected_issue()
        t = self.preview
        t.config(state="normal")
        t.delete("1.0", "end")
        if i:
            try:
                text, _n, _b = read_source(i.path)
                ls = text.split("\n")
                a, b = max(0, i.line - 5), min(len(ls), i.line + 4)
                for k in range(a, b):
                    t.insert("end", "%5d  %s\n" % (k + 1, ls[k]), ("hit",) if k + 1 == i.line else ())
            except OSError:
                t.insert("end", "(file not found)")
        t.config(state="disabled")

    def _context_menu(self, ev):
        iid = self.tree.identify_row(ev.y)
        if not iid:
            return
        self.tree.selection_set(iid)
        i = self.shown[iid]
        m = self.tk.Menu(self.win, tearoff=0)
        m.add_command(label="Fix...", command=self.fix_selected, state="normal" if i.fix else "disabled")
        m.add_command(label="Open in editor", command=self.open_selected)
        m.add_command(label="Copy path", command=lambda: (self.win.clipboard_clear(), self.win.clipboard_append(i.path)))
        m.add_separator()
        m.add_command(label="Ignore this file", command=lambda: self.ignore_file(i))
        m.add_command(label="Turn off rule %s" % i.rule, command=lambda: self.turn_off_rule(i.rule))
        m.tk_popup(ev.x_root, ev.y_root)

    def ignore_file(self, i):
        try:
            text, nl, bom = read_source(i.path)
            old = encode_source(text, nl, bom)
            new = encode_source("// riven-lint: ignore-file\n" + text, nl, bom)
            write_bytes_atomic(i.path, new)
            self.undo_stack.append((i.path, old))
            self.rescan_file(i.path)
        except OSError as e:
            self.messagebox.showerror("Riven Lint", str(e))

    def turn_off_rule(self, rule):
        if self.messagebox.askyesno("Riven Lint", "Turn off rule %s for this project?\nThis gets saved in %s." % (rule, CONFIG_NAME)):
            self.cfg.setdefault("severity", {})[rule] = "off"
            if self.root_dir:
                save_config(self.root_dir, self.cfg)
            self.rescan()

    def open_selected(self):
        i = self.selected_issue()
        if i:
            try:
                open_in_editor(self.cfg, i.path, i.line)
            except Exception as e:
                self.messagebox.showerror("Riven Lint", "Couldn't open the editor:\n%s\n\nYou can set an editor command under "
                                                        "Tools > Settings." % e)

    def fix_selected(self):
        i = self.selected_issue()
        if not i:
            return
        if not i.fix:
            self.open_selected()
            return
        self.fix_dialog(i, queue_mode=False)

    def fix_queue(self):
        todo = [i for i in self._visible() if i.fix]
        if not todo:
            self.messagebox.showinfo("Riven Lint", "There's nothing to fill in with the current filters.\n"
                                                    "Clear the filters, or look for issues marked with a *.")
            return
        keys = []
        seen = set()
        for i in todo:
            if i.key not in seen:
                seen.add(i.key)
                keys.append(i.key)
        n = len(keys)
        done = 0
        for pos, key in enumerate(keys):
            cur = next((x for x in self.issues if x.key == key), None)
            if cur is None:
                continue
            res = self.fix_dialog(cur, queue_mode=True, pos=(pos + 1, n))
            if res == "stop":
                break
            done += 1
        self.var_status.set("Fix queue done.")

    def fix_dialog(self, issue, queue_mode, pos=None):
        tk, ttk = self.tk, self.ttk
        dlg = tk.Toplevel(self.win)
        dlg.transient(self.win)
        title = "Fix %s" % issue.rule if not pos else "Fix %s  (%d of %d)" % (issue.rule, pos[0], pos[1])
        dlg.title(title)
        dlg.geometry("760x560")
        result = {"v": "cancel"}
        frm = ttk.Frame(dlg, padding=10)
        frm.pack(fill="both", expand=True)
        ttk.Label(frm, text="%s  line %d" % (issue.rel, issue.line), font=("TkDefaultFont", 10, "bold")).pack(anchor="w")
        ttk.Label(frm, text=issue.message, wraplength=720).pack(anchor="w", pady=(0, 6))
        ctx = tk.Text(frm, height=9, wrap="none", font=("Courier", 10), bg="#fafafa", fg="#222222")
        try:
            text, _n, _b = read_source(issue.path)
            ls = text.split("\n")
            a, b = max(0, issue.line - 5), min(len(ls), issue.line + 5)
            for k in range(a, b):
                ctx.insert("end", "%5d  %s\n" % (k + 1, ls[k]), ("hit",) if k + 1 == issue.line else ())
        except OSError:
            pass
        ctx.tag_configure("hit", background="#ffe9a8")
        ctx.config(state="disabled")
        ctx.pack(fill="x", pady=(0, 8))
        form = ttk.Frame(frm)
        form.pack(fill="both", expand=True)
        ents = {}
        d = issue.data

        def text_area(label, init="", height=3):
            ttk.Label(form, text=label).pack(anchor="w", pady=(4, 0))
            w = tk.Text(form, height=height, wrap="word")
            w.insert("1.0", init)
            w.pack(fill="x")
            return w

        def entry(label, init=""):
            ttk.Label(form, text=label).pack(anchor="w", pady=(4, 0))
            v = tk.StringVar(value=init)
            ttk.Entry(form, textvariable=v).pack(fill="x")
            return v

        first_widget = None
        if issue.fix == "header":
            ents["author"] = entry("Author", d.get("author", ""))
            ents["contributors"] = entry("Contributors (comma separated, optional)", ", ".join(d.get("contributors", [])))
            ents["description"] = text_area("Description: what is this script for? A line or two is fine.", d.get("description", ""))
            first_widget = ents["description"]
        elif issue.fix == "summary":
            if d.get("mode") == "new":
                ents["summary"] = text_area("Summary: what does '%s' do, and how do you use it?" % d["name"], "")
                first_widget = ents["summary"]
            ents["params"] = {}
            pnames = d.get("params", []) if d.get("mode") == "new" else d.get("missing_params", [])
            for p in pnames:
                ents["params"][p] = entry("Parameter '%s'" % p)
                if first_widget is None:
                    first_widget = True
            if (d.get("returns") and d.get("mode") == "new") or d.get("need_returns"):
                ents["returns"] = entry("Returns")
        elif issue.fix == "tooltip":
            ents["tip"] = text_area("Tooltip for '%s'" % d.get("name", ""), "", 2)
            first_widget = ents["tip"]
            ents["as_comment"] = tk.BooleanVar(value=d.get("as_comment", False))
            ttk.Checkbutton(form, text="Use a // comment instead of a [Tooltip] "
                                       "(for variables that are hidden in the inspector)",
                            variable=ents["as_comment"]).pack(anchor="w", pady=4)

        def collect():
            def val(x):
                if isinstance(x, tk.Text):
                    return x.get("1.0", "end").strip()
                return x.get().strip()
            v = {}
            for k, x in ents.items():
                if k == "params":
                    v["params"] = {p: val(w) for p, w in x.items()}
                elif k == "as_comment":
                    v["as_comment"] = x.get()
                elif k == "tip":
                    v["tip"] = val(x)
                else:
                    v[k] = val(x)
            return v

        def apply(go_next):
            v = collect()
            req = {"header": "description", "summary": "summary", "tooltip": "tip"}[issue.fix]
            if issue.fix == "header" and not v.get("description"):
                if not self.messagebox.askyesno("Riven Lint", "The description is empty. Apply anyway?", parent=dlg):
                    return
            elif issue.fix in ("summary", "tooltip") and req in v and not v[req]:
                self.messagebox.showinfo("Riven Lint", "Write something first, or hit Skip.", parent=dlg)
                return
            try:
                old, _new = apply_fix(issue, v)
            except Exception as e:
                self.messagebox.showerror("Riven Lint", "Couldn't apply the fix:\n%s" % e, parent=dlg)
                return
            self.undo_stack.append((issue.path, old))
            if issue.fix == "header" and v.get("author"):
                self.cfg["author"] = v["author"]
            self.rescan_file(issue.path)
            result["v"] = "next" if go_next else "applied"
            dlg.destroy()

        bar = ttk.Frame(frm)
        bar.pack(fill="x", pady=(10, 0))
        if queue_mode:
            ttk.Button(bar, text="Apply & Next  (Ctrl+Enter)", command=lambda: apply(True)).pack(side="left")
            ttk.Button(bar, text="Skip", command=lambda: (result.update(v="skip"), dlg.destroy())).pack(side="left", padx=6)
            ttk.Button(bar, text="Open in editor", command=lambda: open_in_editor(self.cfg, issue.path, issue.line)).pack(
                side="left")
            ttk.Button(bar, text="Stop", command=lambda: (result.update(v="stop"), dlg.destroy())).pack(side="right")
        else:
            ttk.Button(bar, text="Apply  (Ctrl+Enter)", command=lambda: apply(False)).pack(side="left")
            ttk.Button(bar, text="Open in editor", command=lambda: open_in_editor(self.cfg, issue.path, issue.line)).pack(
                side="left", padx=6)
            ttk.Button(bar, text="Cancel", command=dlg.destroy).pack(side="right")
        dlg.bind("<Control-Return>", lambda e: apply(queue_mode))
        dlg.bind("<Escape>", lambda e: dlg.destroy())
        if isinstance(first_widget, tk.Text):
            first_widget.focus_set()
        dlg.grab_set()
        self.win.wait_window(dlg)
        return result["v"]

    def undo(self):
        if not self.undo_stack:
            self.var_status.set("Nothing to undo.")
            return
        path, old = self.undo_stack.pop()
        try:
            write_bytes_atomic(path, old)
        except OSError as e:
            self.messagebox.showerror("Riven Lint", str(e))
            return
        self.rescan_file(path)
        self.var_status.set("Undid the last change to %s" % os.path.basename(path))

    def settings_dialog(self):
        tk, ttk = self.tk, self.ttk
        dlg = tk.Toplevel(self.win)
        dlg.title("Settings")
        dlg.geometry("640x640")
        dlg.transient(self.win)
        frm = ttk.Frame(dlg, padding=10)
        frm.pack(fill="both", expand=True)
        ttk.Label(frm, text="These settings are saved in %s in the folder you scan. Commit it and your team gets the same settings."
                  % CONFIG_NAME, wraplength=600).pack(anchor="w", pady=(0, 8))
        v_author = tk.StringVar(value=self.cfg.get("author", ""))
        v_year = tk.StringVar(value=self.cfg.get("year", "2027"))
        v_editor = tk.StringVar(value=self.cfg.get("editor", ""))
        for label, var in (("Your name (used for new headers)", v_author), ("Year on new headers", v_year),
                           ("Editor command. Use {file} and {line}, for example: rider --line {line} {file}", v_editor)):
            ttk.Label(frm, text=label).pack(anchor="w", pady=(4, 0))
            ttk.Entry(frm, textvariable=var).pack(fill="x")
        ttk.Label(frm, text="Skip these (one per line, folder names or wildcards)").pack(anchor="w", pady=(8, 0))
        ign = tk.Text(frm, height=7)
        ign.insert("1.0", "\n".join(self.cfg.get("ignore", [])))
        ign.pack(fill="x")
        ttk.Label(frm, text="How serious is each rule?").pack(anchor="w", pady=(8, 2))
        grid = ttk.Frame(frm)
        grid.pack(fill="x")
        sev_vars = {}
        for r, (rid, (name, default, _d)) in enumerate(RULES.items()):
            ttk.Label(grid, text="%s  %s" % (rid, name)).grid(row=r, column=0, sticky="w", pady=1)
            v = tk.StringVar(value=self.cfg.get("severity", {}).get(rid, default))
            ttk.Combobox(grid, textvariable=v, values=SEVERITIES, state="readonly", width=9).grid(row=r, column=1, padx=8)
            sev_vars[rid] = v

        def save():
            self.cfg["author"] = v_author.get().strip()
            self.cfg["year"] = v_year.get().strip() or "2027"
            self.cfg["editor"] = v_editor.get().strip()
            self.cfg["ignore"] = [l.strip() for l in ign.get("1.0", "end").splitlines() if l.strip()]
            self.cfg["severity"] = {rid: v.get() for rid, v in sev_vars.items() if v.get() != RULES[rid][1]}
            if self.root_dir:
                save_config(self.root_dir, self.cfg)
            dlg.destroy()
            self.rescan()

        bar = ttk.Frame(frm)
        bar.pack(fill="x", pady=10)
        ttk.Button(bar, text="Save & Rescan", command=save).pack(side="left")
        ttk.Button(bar, text="Cancel", command=dlg.destroy).pack(side="right")
        dlg.grab_set()

    def rules_dialog(self):
        tk, ttk = self.tk, self.ttk
        dlg = tk.Toplevel(self.win)
        dlg.title("Rules")
        dlg.geometry("700x520")
        t = tk.Text(dlg, wrap="word", padx=10, pady=10)
        t.pack(fill="both", expand=True)
        for rid, (name, sev, desc) in RULES.items():
            t.insert("end", "%s  %s   [default: %s]\n    %s\n\n" % (rid, name, sev, desc))
        t.insert("end", "To skip a single script, add one of these as a comment in it:\n    // riven-lint: ignore-file\n"
                        "    // riven-lint: ignore D001,T001\n")
        t.config(state="disabled")

    def export_report(self):
        p = self.filedialog.asksaveasfilename(defaultextension=".txt", filetypes=[
            ("Text", "*.txt"), ("CSV", "*.csv"), ("JSON", "*.json")])
        if not p:
            return
        fmt = "csv" if p.lower().endswith(".csv") else "json" if p.lower().endswith(".json") else "text"
        with open(p, "w", encoding="utf-8") as f:
            f.write(format_report(self._visible(), fmt) + "\n")
        self.var_status.set("Report saved to %s" % p)


def main(argv=None):
    ap = argparse.ArgumentParser(description="Checks Riven Framework scripts")
    ap.add_argument("path", nargs="?", help="folder to open in the window")
    ap.add_argument("--check", metavar="FOLDER", help="skip the window, print the issues, exit with 1 if there are errors")
    ap.add_argument("--min-severity", choices=["error", "warning", "info"], default="info")
    ap.add_argument("--format", choices=["text", "csv", "json"], default="text")
    ap.add_argument("--strict", action="store_true", help="exit with 1 on warnings too")
    args = ap.parse_args(argv)
    if args.check:
        return cli_check(args)
    return run_gui(args.path)


if __name__ == "__main__":
    sys.exit(main())
