#!/usr/bin/env python3
# Riven Format
# Rebuilds a C# script so it follows a template of your own.
#
#   python riven_format.py                       opens the window
#   python riven_format.py File.cs --auto        preview with default choices, nothing is written
#   python riven_format.py File.cs --auto --apply
#   python riven_format.py Folder --auto --apply convert every .cs file in a folder
#   python riven_format.py --new MyScript        print a new script made from the template
#
# The template is a plain text file (riven_format_template.txt next to this script).
# Tokens that mean something to the tool:
#   #USINGS#                   where the using statements go
#   #ROOTNAMESPACEBEGIN#       start of the namespace (the lines in between are indented for it)
#   #ROOTNAMESPACEEND#         end of the namespace
#   #SCRIPTNAME#               the name of the main class
#   #ADDITIONAL CLASSES#       where the other classes of the file go
# Every #region ... #endregion pair with nothing inside it is a section that code can be sorted into.
# The line with "class #SCRIPTNAME#" is replaced with the real class declaration.

import argparse
import bisect
import json
import os
import re
import sys

APP_DIR = os.path.dirname(os.path.abspath(__file__))
CONFIG_PATH = os.path.join(APP_DIR, "riven_format.json")
TEMPLATE_PATH = os.path.join(APP_DIR, "riven_format_template.txt")

DEFAULT_TEMPLATE = """//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//
//====================================================================================================================//

#USINGS#
using UnityEngine;

#ROOTNAMESPACEBEGIN#
    public class #SCRIPTNAME# : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }

    #ADDITIONAL CLASSES#
#ROOTNAMESPACEEND#
"""

UNITY_MESSAGES = (
    "Awake Start Update FixedUpdate LateUpdate OnEnable OnDisable OnDestroy OnValidate Reset OnGUI "
    "OnDrawGizmos OnDrawGizmosSelected OnTriggerEnter OnTriggerExit OnTriggerStay OnTriggerEnter2D "
    "OnTriggerExit2D OnTriggerStay2D OnCollisionEnter OnCollisionExit OnCollisionStay OnCollisionEnter2D "
    "OnCollisionExit2D OnCollisionStay2D OnMouseDown OnMouseUp OnMouseEnter OnMouseExit OnMouseOver "
    "OnMouseDrag OnMouseUpAsButton OnApplicationQuit OnApplicationPause OnApplicationFocus OnBecameVisible "
    "OnBecameInvisible OnAnimatorMove OnAnimatorIK OnParticleCollision OnParticleTrigger "
    "OnTransformChildrenChanged OnTransformParentChanged OnBeforeTransformParentChanged "
    "OnRectTransformDimensionsChange OnPreCull OnPreRender OnPostRender OnRenderImage OnRenderObject "
    "OnWillRenderObject OnJointBreak OnControllerColliderHit OnAudioFilterRead OnCanvasGroupChanged "
    "OnDidApplyAnimationProperties OnBeforeSerialize OnAfterDeserialize OnPointerClick OnPointerDown "
    "OnPointerUp OnPointerEnter OnPointerExit OnDrag OnBeginDrag OnEndDrag OnDrop OnScroll OnSelect "
    "OnDeselect OnMove OnSubmit OnCancel OnUpdateSelected OnInitializePotentialDrag"
).split()

REFERENCE_TYPES = [
    r"GameObject", r"Transform", r"RectTransform", r"Component", r"Rigidbody\w*", r"\w*Collider\w*",
    r"Camera", r"Animator", r"AudioSource", r"AudioListener", r"\w*Renderer", r"Canvas\w*", r"Image",
    r"Text", r"TMP_\w+", r"TextMeshPro\w*", r"Button", r"Slider", r"Toggle", r"Light\w*", r"NavMesh\w*",
    r"CharacterController", r"GI_\w+", r"WB_\w+", r"LB_\w+", r"Pawn\w*", r"Actor\w*",
]

def _rule(label, section, category, *conds):
    return {"label": label, "section": section, "category": category,
            "when": [{"field": f, "pattern": p, "not": n} for f, p, n in conds]}


DEFAULT_RULES = [
    _rule("Unity messages", "function", "Inherited", ("name", "@unity_messages", False)),
    _rule("Overrides", "function", "Inherited", ("mods", r"\boverride\b", False)),
    _rule("Constructors, nested types, delegates", "function", "Internal", ("kind", r"^(nested|delegate|ctor|dtor)$", False)),
    _rule("Public functions", "function", "External", ("access", r"^public$", False)),
    _rule("Events", "variable", "Reference", ("kind", r"^event$", False)),
    _rule("Event-like types", "variable", "Reference", ("text", r"\b(UnityEvent\w*|Action|Func|EventHandler|LogicOutput|LogicInput)\b", False)),
    _rule("Constants", "variable", "Internal", ("mods", r"\bconst\b", False)),
    _rule("Private static readonly", "variable", "Internal", ("mods", r"\bstatic\b", False), ("mods", r"\breadonly\b", False), ("access", r"^public$", True)),
    _rule("Public properties", "variable", "External", ("kind", r"^property$", False), ("access", r"^public$", False)),
    _rule("Other properties", "variable", "Internal", ("kind", r"^property$", False)),
    _rule("Public static", "variable", "External", ("access", r"^public$", False), ("mods", r"\bstatic\b", False)),
    _rule("Public but hidden", "variable", "External", ("access", r"^public$", False), ("attrs", r"HideInInspector|NonSerialized", False)),
    _rule("Public fields", "variable", "Inspector", ("access", r"^public$", False)),
    _rule("Serialized fields", "variable", "Inspector", ("attrs", r"SerializeField|SerializeReference", False)),
    _rule("Known reference types", "variable", "Reference", ("type", "@reference_types", False)),
]


DEFAULT_SETTINGS = {
    "template_path": "",
    "namespace_mode": "use",
    "namespace": "RivenFramework",
    "remove_banners": True,
    "carry_description": True,
    "sort_usings": True,
    "member_indent": 4,
    "blank_between_vars": 0,
    "blank_between_funcs": 1,
    "blank_between_types": 1,
    "backup": False,
    "doc_formats": {
        "tooltip": '[Tooltip("{text}")]',
        "comment": "// {text}",
        "summary": "/// <summary>\n/// {text}\n/// </summary>",
    },
    "doc_style": {"Internal": "comment"},
    "category_kinds": {},
    "unity_messages": UNITY_MESSAGES,
    "reference_types": REFERENCE_TYPES,
    # Old-layout cleanup. Each entry is a regex matched against one whole line.
    "banner_lines": [r"^\s*/\*[-=*\s]*\[.*\][-=*\s]*\*/\s*$"],
    "banner_framed": [r"^\s*//\s*=[-=]{3,}=?\s*$"],
    # Header comment. Each heading is a regex matched against the text after the //.
    "header": {
        "detect": r"====|Author|Description|Written by",
        "author": r"Authors?",
        "contributors": r"Contributors?",
        "description": r"Description",
        "description_line": "//  {text}",
        "name_line": "//  {text}",
    },
    "header_author_mode": "keep",
    "header_description_from_summary": False,
    # Where members go when no existing region says otherwise. First matching rule wins.
    "placement_rules": DEFAULT_RULES,
    "placement_default": "Internal",
    # Which sections are expected to carry a description (used for the yellow/green check)
    "describe_variables_except": ["Internal"],
    "describe_functions_in": ["External"],
}


class Unsupported(Exception):
    pass


# ---------------------------------------------------------------------------------------------------------------------
# Settings
# ---------------------------------------------------------------------------------------------------------------------

def load_settings(path=None):
    s = json.loads(json.dumps(DEFAULT_SETTINGS))
    path = path or CONFIG_PATH
    if os.path.isfile(path):
        try:
            with open(path, "r", encoding="utf-8") as f:
                data = json.load(f)
            for k, v in data.items():
                if isinstance(v, dict) and isinstance(s.get(k), dict):
                    s[k].update(v)
                else:
                    s[k] = v
        except (OSError, ValueError):
            pass
    return s


def save_settings(s, path=None):
    with open(path or CONFIG_PATH, "w", encoding="utf-8") as f:
        json.dump(s, f, indent=2)


def load_template_text(settings):
    path = settings.get("template_path") or TEMPLATE_PATH
    if os.path.isfile(path):
        with open(path, "r", encoding="utf-8") as f:
            return f.read().replace("\r\n", "\n")
    return DEFAULT_TEMPLATE


# ---------------------------------------------------------------------------------------------------------------------
# Source masking (comments and strings blanked, same length)
# ---------------------------------------------------------------------------------------------------------------------

def mask_source(text):
    n = len(text)
    out = list(text)
    verbatim_lines = set()
    raw_found = False
    line = 0
    stack = [["code", 0]]
    i = 0

    def blank(a, b):
        for k in range(a, b):
            if out[k] != "\n":
                out[k] = " "

    while i < n:
        c = text[i]
        mode = stack[-1][0]
        if c == "\n":
            line += 1
            if mode == "str" and stack[-1][1] in ("verb", "verbi"):
                verbatim_lines.add(line)
            i += 1
            continue
        if mode == "code":
            nxt = text[i + 1] if i + 1 < n else ""
            if c == "/" and nxt == "/":
                j = text.find("\n", i)
                j = n if j == -1 else j
                blank(i, j)
                i = j
                continue
            if c == "/" and nxt == "*":
                j = text.find("*/", i + 2)
                j = n if j == -1 else j + 2
                line += text.count("\n", i, j)
                blank(i, j)
                i = j
                continue
            if c == "'":
                j = i + 1
                if j < n and text[j] == "\\":
                    j += 2
                    while j < n and text[j] != "'" and text[j] != "\n":
                        j += 1
                else:
                    j += 1
                if j < n and text[j] == "'":
                    blank(i + 1, j)
                    i = j + 1
                else:
                    i += 1
                continue
            if c == '"':
                if text.startswith('"""', i):
                    raw_found = True
                prefix = ""
                k = i - 1
                while k >= 0 and text[k] in "@$":
                    prefix += text[k]
                    k -= 1
                verb = "@" in prefix
                interp = "$" in prefix
                kind = ("verbi" if interp else "verb") if verb else ("regi" if interp else "reg")
                stack.append(["str", kind])
                i += 1
                continue
            if stack[-1][1] > 0 or len(stack) > 1:
                if c == "{":
                    stack[-1][1] += 1
                elif c == "}":
                    stack[-1][1] -= 1
                    if stack[-1][1] < 0 and len(stack) > 1:
                        stack.pop()
                        i += 1
                        continue
            i += 1
            continue
        kind = stack[-1][1]
        interp = kind in ("regi", "verbi")
        verb = kind in ("verb", "verbi")
        if c == '"':
            if verb and i + 1 < n and text[i + 1] == '"':
                out[i] = " "
                out[i + 1] = " "
                i += 2
                continue
            stack.pop()
            i += 1
            continue
        if c == "\\" and not verb:
            out[i] = " "
            if i + 1 < n and text[i + 1] != "\n":
                out[i + 1] = " "
            i += 2
            continue
        if interp and c == "{":
            if i + 1 < n and text[i + 1] == "{":
                out[i] = " "
                out[i + 1] = " "
                i += 2
                continue
            stack.append(["code", 0])
            i += 1
            continue
        if interp and c == "}" and i + 1 < n and text[i + 1] == "}":
            out[i] = " "
            out[i + 1] = " "
            i += 2
            continue
        out[i] = " "
        i += 1
    return "".join(out), verbatim_lines, raw_found


class Text:
    # A source text with its masked copy and line tables
    def __init__(self, text):
        self.text = text
        self.masked, self.verb, self.raw_found = mask_source(text)
        self.lines = text.split("\n")
        self.mlines = self.masked.split("\n")
        self.starts = []
        pos = 0
        for ln in self.lines:
            self.starts.append(pos)
            pos += len(ln) + 1

    def line_of(self, pos):
        return max(0, bisect.bisect_right(self.starts, pos) - 1)

    def line_end(self, line):
        return self.starts[line] + len(self.lines[line])


def match_brace(m, i):
    # m[i] is '{'. Returns the index of the matching '}'.
    depth = 0
    n = len(m)
    while i < n:
        c = m[i]
        if c == "{":
            depth += 1
        elif c == "}":
            depth -= 1
            if depth == 0:
                return i
        i += 1
    raise Unsupported("unbalanced braces")


def scan_to_semicolon(m, i, end):
    depth = 0
    while i < end:
        c = m[i]
        if c in "([{":
            depth += 1
        elif c in ")]}":
            depth -= 1
        elif c == ";" and depth <= 0:
            return i + 1
        i += 1
    raise Unsupported("statement without a semicolon")


TYPE_KW = re.compile(r"\b(class|struct|interface|enum|record)\b")


def scan_item(T, p, end):
    # Finds the extent of the member or type that starts at p.
    m = T.masked
    i = p
    depth = 0
    while i < end:
        c = m[i]
        if c in "([":
            depth += 1
        elif c in ")]":
            depth -= 1
        elif depth == 0:
            if c == ";":
                return {"hdr_end": i, "boundary": ";", "end": i + 1}
            if c == "{":
                header = m[p:i]
                close = match_brace(m, i)
                item_end = close + 1
                is_type = bool(TYPE_KW.search(header))
                if not is_type and not header.rstrip().endswith(")"):
                    k = item_end
                    while k < end and m[k].isspace():
                        k += 1
                    if k < end and m[k] == "=" and not m.startswith("==", k):
                        item_end = scan_to_semicolon(m, k + 1, end)
                return {"hdr_end": i, "boundary": "{", "end": item_end, "open": i, "close": close}
            if c == "=":
                nxt = m[i + 1] if i + 1 < len(m) else ""
                if nxt == ">":
                    return {"hdr_end": i, "boundary": "=>", "end": scan_to_semicolon(m, i + 2, end)}
                return {"hdr_end": i, "boundary": "=", "end": scan_to_semicolon(m, i + 1, end)}
        i += 1
    raise Unsupported("could not find the end of a declaration")


def next_code(T, pos, end):
    # Position of the next code character, skipping blanks, comments and directive lines
    while pos < end:
        line = T.line_of(pos)
        ls = T.starts[line]
        le = T.line_end(line)
        whole = T.masked[ls:le].strip()
        seg = T.masked[pos:min(le, end)]
        if whole.startswith("#") or seg.strip() == "":
            pos = le + 1
            continue
        return pos + (len(seg) - len(seg.lstrip()))
    return None


def extend_to_line_end(T, end_pos):
    # Includes a comment that follows the declaration on the same line
    line = T.line_of(max(0, end_pos - 1))
    le = T.line_end(line)
    if end_pos < le and T.masked[end_pos:le].strip() == "":
        return le
    return end_pos


# ---------------------------------------------------------------------------------------------------------------------
# Preprocessor directives
# ---------------------------------------------------------------------------------------------------------------------

def region_title(arg):
    m = re.search(r"\(\s*(.+?)\s*\)", arg)
    if m:
        return m.group(1)
    return arg.strip("-= /\t").strip()


def scan_directives(T):
    n = len(T.lines)
    cond = [None] * n
    regions = [()] * n
    kinds = [None] * n
    stack = []
    rstack = []

    def effective():
        parts = []
        for g in stack:
            gp = ["!(%s)" % x for x in g["neg"]]
            if g["cur"] is not None:
                gp.append(g["cur"] if not g["neg"] else "(%s)" % g["cur"])
            parts.append(" && ".join(gp))
        parts = [p for p in parts if p]
        if not parts:
            return None
        if len(parts) == 1:
            return parts[0]
        return " && ".join("(%s)" % p if re.search(r"[\s|!]", p) else p for p in parts)

    for i, ml in enumerate(T.mlines):
        s = ml.strip()
        if s.startswith("#"):
            mm = re.match(r"\s*#\s*(\w+)\s*(.*?)\s*(?://.*)?$", T.lines[i])
            d = mm.group(1) if mm else ""
            arg = mm.group(2) if mm else ""
            kinds[i] = d
            if d == "if":
                stack.append({"neg": [], "cur": arg})
            elif d == "elif" and stack:
                g = stack[-1]
                if g["cur"] is not None:
                    g["neg"].append(g["cur"])
                g["cur"] = arg
            elif d == "else" and stack:
                g = stack[-1]
                if g["cur"] is not None:
                    g["neg"].append(g["cur"])
                g["cur"] = None
            elif d == "endif" and stack:
                stack.pop()
            elif d == "region":
                rstack.append(region_title(arg))
            elif d == "endregion" and rstack:
                rstack.pop()
        cond[i] = effective()
        regions[i] = tuple(rstack)
    return cond, regions, kinds


# ---------------------------------------------------------------------------------------------------------------------
# Parsing a C# file
# ---------------------------------------------------------------------------------------------------------------------

TOOLTIP_RE = re.compile(r'\[(?:field:\s*)?Tooltip\(\s*"((?:[^"\\]|\\.)*)"\s*\)\]')
MODS = {"public", "private", "protected", "internal", "static", "readonly", "const", "abstract", "virtual",
        "override", "sealed", "new", "extern", "unsafe", "async", "partial", "volatile", "event", "implicit",
        "explicit", "operator"}


def expand_indent(line):
    # Number of leading columns (tab = 4)
    n = 0
    for ch in line:
        if ch == " ":
            n += 1
        elif ch == "\t":
            n += 4
        else:
            break
    return n


def shift_line(line, delta):
    if line.strip() == "":
        return ""
    n = expand_indent(line)
    return " " * max(0, n + delta) + line.lstrip(" \t")


def collapse(s):
    return re.sub(r"\s+", " ", s).strip()


def _compile_list(items):
    out = []
    for p in items or []:
        try:
            out.append(re.compile(p))
        except re.error:
            pass
    return out


def clean_lead(lines, settings):
    # lines: list of raw comment/blank lines. Drops banners, collapses blanks, trims the ends
    out = list(lines)
    if settings.get("remove_banners", True):
        framed = _compile_list(settings.get("banner_framed"))
        single = _compile_list(settings.get("banner_lines"))
        res = []
        i = 0
        while i < len(out):
            ln = out[i]
            if any(r.match(ln) for r in framed):
                if i + 2 < len(out) and any(r.match(out[i + 2]) for r in framed) and out[i + 1].strip().startswith("//"):
                    i += 3
                    continue
                i += 1
                continue
            if any(r.match(ln) for r in single):
                i += 1
                continue
            res.append(ln)
            i += 1
        out = res
    res = []
    for ln in out:
        if ln.strip() == "" and (not res or res[-1].strip() == ""):
            continue
        res.append(ln)
    while res and res[-1].strip() == "":
        res.pop()
    return res


class Member:
    def __init__(self):
        self.kind = "field"        # field property event method ctor dtor nested type delegate comment
        self.section = "variable"  # variable or function
        self.name = ""
        self.sig = ""
        self.access = "private"
        self.mods = set()
        self.attrs = ""
        self.attr_list = []
        self.rest = ""
        self.lead = []
        self.body = ""
        self.trail = []
        self.indent0 = 0
        self.verb_rel = set()
        self.cond = None
        self.region_path = ()
        self.existing_text = ""
        self.existing_style = ""
        self.category = ""
        self.doc = ""
        self.placement = ""
        self.order = 0
        self.src_start = 0
        self.src_end = 0

    def label(self):
        return "%s: %s" % (self.kind, self.sig)


def classify(m, header_masked, header_raw, boundary, class_name):
    # Attributes
    pos = 0
    hm = header_masked
    attrs = []
    while True:
        while pos < len(hm) and hm[pos].isspace():
            pos += 1
        if pos < len(hm) and hm[pos] == "[":
            depth = 0
            k = pos
            while k < len(hm):
                if hm[k] == "[":
                    depth += 1
                elif hm[k] == "]":
                    depth -= 1
                    if depth == 0:
                        break
                k += 1
            attrs.append(header_raw[pos:k + 1])
            pos = k + 1
        else:
            break
    rest = collapse(hm[pos:])
    m.attrs = " ".join(attrs)
    m.attr_list = attrs
    m.rest = rest
    words = re.findall(r"[A-Za-z_]\w*", re.sub(r"<[^<>()]*>", " ", rest))
    m.mods = {w for w in words if w in MODS}
    acc = "private"
    if "public" in m.mods:
        acc = "public"
    elif "protected" in m.mods:
        acc = "protected"
    elif "internal" in m.mods:
        acc = "internal"
    m.access = acc
    tm = re.search(r"\b(class|struct|interface|enum|record)\b\s+(\w+)", rest)
    stripped = re.sub(r"\bwhere\s+\w+\s*:.*$", "", rest).rstrip()
    if tm:
        m.kind = "nested"
        m.name = tm.group(2)
        m.section = "function"
    elif re.search(r"\bdelegate\b", rest):
        m.kind = "delegate"
        m.section = "function"
        mm = re.search(r"(\w+)\s*(?:<[^()]*>)?\s*\(", rest)
        m.name = mm.group(1) if mm else "delegate"
    elif stripped.endswith(")"):
        mm = re.search(r"([~\w]+)\s*(?:<[^()]*>)?\s*\(", stripped)
        name = mm.group(1) if mm else "?"
        if "operator" in m.mods:
            name = "operator"
        m.name = name
        m.section = "function"
        if name.startswith("~"):
            m.kind = "dtor"
        elif name == class_name:
            m.kind = "ctor"
        else:
            m.kind = "method"
    elif re.search(r"\bevent\b", rest):
        m.kind = "event"
        m.section = "variable"
        m.name = (re.findall(r"\w+", rest) or ["?"])[-1]
    elif boundary in ("{", "=>"):
        m.kind = "property"
        m.section = "variable"
        names = re.findall(r"[A-Za-z_]\w*", rest)
        m.name = names[-1] if names else "?"
    else:
        m.kind = "field"
        m.section = "variable"
        names = re.findall(r"[A-Za-z_]\w*", rest)
        m.name = names[-1] if names else "?"
    sig = rest
    if len(sig) > 90:
        sig = sig[:87] + "..."
    m.sig = sig


def extract_existing_doc(m):
    m.existing_text = ""
    m.existing_style = ""
    tm = None if m.kind in ("nested", "type", "delegate") else TOOLTIP_RE.search(m.attrs or "")
    if tm:
        m.existing_text = tm.group(1).replace('\\"', '"').replace("\\\\", "\\")
        m.existing_style = "tooltip"
        return
    tail = []
    for ln in reversed(m.lead):
        s = ln.strip()
        if s.startswith("///"):
            tail.append(s)
        else:
            break
    if tail:
        tail.reverse()
        joined = " ".join(re.sub(r"^///\s?", "", t) for t in tail)
        sm = re.search(r"<summary>(.*?)</summary>", joined)
        if sm:
            m.existing_text = collapse(sm.group(1))
            m.existing_style = "summary"
            return
    tail = []
    for ln in reversed(m.lead):
        s = ln.strip()
        if s.startswith("//") and not s.startswith("///"):
            tail.append(s)
        else:
            break
    if tail:
        tail.reverse()
        m.existing_text = collapse(" ".join(re.sub(r"^//+\s?", "", t) for t in tail))
        m.existing_style = "comment"


def build_item(T, p, item, class_name, prev_end_line, dirs, settings, order):
    cond, regions, _kinds = dirs
    m = Member()
    end = extend_to_line_end(T, item["end"])
    start_line = T.line_of(p)
    end_line = T.line_of(max(p, end - 1))
    if cond[start_line] != cond[end_line]:
        raise Unsupported("a #if block starts or ends in the middle of a declaration near line %d" % (start_line + 1))
    m.cond = cond[start_line]
    m.region_path = regions[start_line]
    m.src_start = p
    m.src_end = end
    hdr_raw = T.text[p:item["hdr_end"]]
    hdr_mask = T.masked[p:item["hdr_end"]]
    classify(m, hdr_mask, hdr_raw, item["boundary"], class_name)
    # Leading comments
    lead = []
    if prev_end_line < start_line:
        for ln in range(prev_end_line + 1, start_line):
            if T.mlines[ln].strip().startswith("#"):
                continue
            raw = T.lines[ln]
            lead.append(raw if raw.strip() else "")
    m.lead = clean_lead(lead, settings)
    m.indent0 = expand_indent(T.lines[start_line])
    m.body = T.text[p:end]
    first_abs = start_line
    m.verb_rel = set()
    for k in range(1, m.body.count("\n") + 1):
        if first_abs + k in T.verb:
            m.verb_rel.add(k)
    m.order = order
    extract_existing_doc(m)
    return m, end_line


class TypeInfo:
    def __init__(self):
        self.member = None
        self.name = ""
        self.kind = ""
        self.mods = set()
        self.attr_lines = []
        self.decl = ""
        self.open = 0
        self.close = 0
        self.members = []
        self.cond = None


class ParsedFile:
    def __init__(self):
        self.T = None
        self.usings = []
        self.namespace = None
        self.types = []
        self.description = []
        self.authors = []
        self.contributors = []
        self.filename = ""
        self.cond = None


def hdr(settings, key):
    return (settings or DEFAULT_SETTINGS).get("header", {}).get(key) or DEFAULT_SETTINGS["header"][key]


def heading_re(settings, key):
    return re.compile(r"^\s*//\s*(?:%s)\s*$" % hdr(settings, key), re.I)


def unique_names(names):
    seen = set()
    out = []
    for nm in names:
        nm = nm.strip()
        if nm and nm.lower() not in seen:
            seen.add(nm.lower())
            out.append(nm)
    return out


def initial_people(parsed, tmpl, settings):
    authors = list(tmpl.authors)
    contributors = list(tmpl.contributors)
    if settings.get("carry_description", True):
        if settings.get("header_author_mode", "keep") == "keep" and parsed.authors:
            authors = list(parsed.authors)
        contributors = contributors + parsed.contributors + parsed.authors
    have = {x.lower() for x in authors}
    contributors = [x for x in unique_names(contributors) if x.lower() not in have]
    return unique_names(authors), contributors


def replace_names(L, i, out, names, settings):
    # Called just after a heading line was written. Drops the template's own names under it and writes ours.
    n = len(L)
    heads = [re.compile(hdr(settings, k), re.I) for k in ("author", "contributors", "description")]
    j = i
    while j < n:
        st = L[j].strip()
        if not st.startswith("//"):
            break
        body = st[2:].strip()
        if body == "" or st[2:3] == "=" or any(h.fullmatch(body) for h in heads):
            break
        j += 1
    if not names:
        return j
    fmt = hdr(settings, "name_line")
    out.extend(fmt.replace("{text}", nm).rstrip() for nm in names)
    while j < n and L[j].strip() in ("//", ""):
        j += 1
    out.append("//")
    return j


def header_desc_lines(plan):
    settings = plan.settings
    if plan.parsed is None:
        return []
    if plan.header_from_summary:
        text = (plan.class_doc or "").strip()
    else:
        text = (plan.header_desc or "").strip()
    if not text:
        return []
    same_as_old = "\n".join(re.sub(r"^\s*//\s?", "", x).strip() for x in plan.header_desc_raw) == text
    if same_as_old and not plan.header_from_summary:
        return list(plan.header_desc_raw)
    fmt = hdr(settings, "description_line")
    return [fmt.replace("{text}", ln).rstrip() for ln in text.split("\n")]


def header_end_line(T, settings=None):
    # The last line of a leading header comment block, or -1
    n = len(T.lines)
    i = 0
    while i < n and T.lines[i].strip() == "":
        i += 1
    start = i
    while i < n and T.mlines[i].strip() == "" and T.lines[i].strip() != "":
        i += 1
    if i == start:
        return -1
    block = "\n".join(T.lines[start:i])
    if re.search(hdr(settings, "detect"), block):
        return i - 1
    return -1


def header_names(lines, key, settings=None):
    heading = re.compile(hdr(settings, key), re.I)
    others = [re.compile(hdr(settings, k), re.I) for k in ("author", "contributors", "description")]
    names = []
    on = False
    for ln in lines:
        st = ln.strip()
        if not st.startswith("//"):
            if st:
                break
            if on:
                break
            continue
        body = st[2:].strip()
        if heading.fullmatch(body):
            on = True
            continue
        if on:
            if body == "" or st[2:3] == "=" or any(o.fullmatch(body) for o in others):
                break
            names.append(body)
    return names


def extract_description(T, settings=None):
    lines = T.lines
    out = []
    head = heading_re(settings, "description")
    for idx, ln in enumerate(lines):
        if head.match(ln):
            for ln2 in lines[idx + 1:]:
                if re.match(r"^\s*//=", ln2) or not ln2.strip().startswith("//"):
                    break
                out.append(ln2.rstrip())
            break
        if ln.strip() and not ln.strip().startswith("//"):
            break
    out = [x for x in out if x.strip() not in ("//", "")]
    return out


def parse_class_members(T, info, dirs, settings):
    cond, regions, kinds = dirs
    members = []
    pos = info.open + 1
    prev_end_line = T.line_of(info.open)
    order = 0
    end = info.close
    while True:
        p = next_code(T, pos, end)
        if p is None:
            break
        item = scan_item(T, p, end)
        start_line = T.line_of(p)
        eff_prev = prev_end_line
        last_dir = None
        for ln in range(prev_end_line + 1, start_line):
            if kinds[ln] in ("region", "endregion"):
                last_dir = ln
        if last_dir is not None:
            if members:
                before = []
                for ln in range(prev_end_line + 1, last_dir):
                    if T.mlines[ln].strip().startswith("#"):
                        continue
                    before.append(T.lines[ln] if T.lines[ln].strip() else "")
                before = clean_lead(before, settings)
                if before:
                    members[-1].trail = members[-1].trail + before
            eff_prev = last_dir
        m, end_line = build_item(T, p, item, info.name, eff_prev, dirs, settings, order)
        members.append(m)
        order += 1
        pos = m.src_end
        prev_end_line = end_line
    # Comments after the last member
    close_line = T.line_of(info.close)
    trail = []
    for ln in range(prev_end_line + 1, close_line):
        if T.mlines[ln].strip().startswith("#"):
            continue
        raw = T.lines[ln]
        trail.append(raw if raw.strip() else "")
    trail = clean_lead(trail, settings)
    if trail:
        if members:
            members[-1].trail = trail
        else:
            c = Member()
            c.kind = "comment"
            c.section = "function"
            c.lead = []
            c.body = "\n".join(t.strip() if i == 0 else t for i, t in enumerate(trail))
            c.indent0 = expand_indent(trail[0])
            c.sig = "(comments)"
            c.order = 0
            members.append(c)
    return members


def scan_top(T, start, end, parsed, dirs, settings, state):
    cond, regions, kinds = dirs
    pos = start
    while True:
        p = next_code(T, pos, end)
        if p is None:
            break
        m = T.masked
        line = T.line_of(p)
        head = m[p:p + 200]
        if re.match(r"using\b", head):
            mu = re.match(r"using\s+(?:static\s+)?[^;{}()]*;", m[p:])
            if not mu:
                raise Unsupported("unrecognized using statement near line %d" % (line + 1))
            text = collapse(T.text[p:p + mu.end()])
            parsed.usings.append((text, cond[line]))
            pos = p + mu.end()
            state["prev"] = T.line_of(pos - 1)
            continue
        if re.match(r"namespace\b", head):
            mn = re.match(r"namespace\s+([\w.]+)\s*([{;])", m[p:])
            if not mn:
                raise Unsupported("unrecognized namespace near line %d" % (line + 1))
            if parsed.namespace is not None and parsed.namespace != mn.group(1):
                raise Unsupported("the file has more than one namespace")
            parsed.namespace = mn.group(1)
            if mn.group(2) == "{":
                open_i = p + mn.end() - 1
                close_i = match_brace(m, open_i)
                state["prev"] = T.line_of(open_i)
                scan_top(T, open_i + 1, close_i, parsed, dirs, settings, state)
                pos = close_i + 1
                state["prev"] = T.line_of(close_i)
            else:
                pos = p + mn.end()
                state["prev"] = T.line_of(pos - 1)
            continue
        if re.match(r"\[\s*assembly\b", head):
            raise Unsupported("the file has an assembly attribute")
        item = scan_item(T, p, end)
        mem, end_line = build_item(T, p, item, "", state["prev"], dirs, settings, len(parsed.types))
        if mem.kind not in ("nested", "delegate"):
            raise Unsupported("unrecognized code outside a class near line %d" % (line + 1))
        info = TypeInfo()
        info.member = mem
        info.name = mem.name
        info.cond = mem.cond
        mm = re.search(r"\b(class|struct|interface|enum|record)\b", mem.rest)
        info.kind = mm.group(1) if mm else mem.kind
        info.mods = set(mem.mods)
        info.decl = mem.rest
        info.attr_lines = list(mem.attr_list)
        if item["boundary"] == "{":
            info.open = item["open"]
            info.close = item["close"]
        parsed.types.append(info)
        pos = mem.src_end
        state["prev"] = end_line


def parse_source(text, filename, settings=None):
    if "\r" in text.replace("\r\n", ""):
        raise Unsupported("the file has stray carriage returns")
    T = Text(text)
    if T.raw_found:
        raise Unsupported("the file uses raw string literals (\"\"\")")
    dirs = scan_directives(T)
    parsed = ParsedFile()
    parsed.T = T
    parsed.filename = filename
    parsed.description = extract_description(T, settings)
    parsed.authors = header_names(T.lines, "author", settings)
    parsed.contributors = header_names(T.lines, "contributors", settings)
    state = {"prev": header_end_line(T, settings)}
    scan_top(T, 0, len(T.text), parsed, dirs, settings or DEFAULT_SETTINGS, state)
    parsed.dirs = dirs
    return parsed


def parse_members_for(parsed, info, settings):
    info.members = parse_class_members(parsed.T, info, parsed.dirs, settings)
    return info.members


# ---------------------------------------------------------------------------------------------------------------------
# Template
# ---------------------------------------------------------------------------------------------------------------------

class Category:
    def __init__(self):
        self.id = ""
        self.title = ""
        self.section = "other"
        self.indent = 0
        self.start = 0
        self.end = 0
        self.top = ""


class Template:
    def __init__(self):
        self.lines = []
        self.cats = []
        self.cat_by_start = {}
        self.usings_idx = None
        self.ns_begin = None
        self.ns_end = None
        self.addl_idx = None
        self.class_line = None
        self.class_open = None
        self.class_close = None
        self.template_usings = []
        self.template_using_lines = set()
        self.base_indent = 0
        self.authors = []
        self.contributors = []


def norm_token(s):
    return re.sub(r"[\s_]", "", s).upper()


def parse_template(text, settings):
    t = Template()
    t.lines = text.split("\n")
    L = t.lines
    for i, ln in enumerate(L):
        tok = norm_token(ln.strip())
        if tok == "#USINGS#":
            t.usings_idx = i
        elif tok == "#ROOTNAMESPACEBEGIN#":
            t.ns_begin = i
        elif tok == "#ROOTNAMESPACEEND#":
            t.ns_end = i
        elif tok == "#ADDITIONALCLASSES#":
            t.addl_idx = i
    for i, ln in enumerate(L):
        if re.search(r"\b(class|struct)\s+#SCRIPTNAME#", ln):
            t.class_line = i
            break
    if t.class_line is None:
        raise Unsupported("the template has no line with 'class #SCRIPTNAME#'")
    T = Text("\n".join(L))
    open_i = None
    for i in range(t.class_line, len(L)):
        if T.mlines[i].rstrip().endswith("{"):
            open_i = T.starts[i] + len(T.mlines[i].rstrip()) - 1
            t.class_open = i
            break
    if open_i is None:
        raise Unsupported("could not find the opening brace of the template class")
    close_i = match_brace(T.masked, open_i)
    t.class_close = T.line_of(close_i)
    t.base_indent = expand_indent(L[t.class_line])
    t.authors = header_names(L, "author", settings)
    t.contributors = header_names(L, "contributors", settings)
    first = t.usings_idx if t.usings_idx is not None else 0
    for i, ln in enumerate(L):
        if t.ns_begin is not None and i >= t.ns_begin:
            break
        if re.match(r"\s*using\s+[^;()]*;\s*$", ln):
            t.template_usings.append(collapse(ln))
            t.template_using_lines.add(i)
    # regions inside the class body
    stack = []
    kinds_override = settings.get("category_kinds", {})
    for i in range(t.class_open + 1, t.class_close):
        s = L[i].strip()
        mm = re.match(r"#\s*region\b(.*)$", s)
        if mm:
            node = {"title": region_title(mm.group(1)), "start": i, "children": 0, "indent": expand_indent(L[i])}
            if stack:
                stack[-1]["children"] += 1
            stack.append(node)
            continue
        if re.match(r"#\s*endregion\b", s) and stack:
            node = stack.pop()
            node["end"] = i
            if node["children"] == 0:
                titles = [x["title"] for x in stack] + [node["title"]]
                cat = Category()
                cat.id = "/".join(titles)
                cat.title = node["title"]
                cat.top = titles[0]
                cat.indent = node["indent"]
                cat.start = node["start"]
                cat.end = i
                top_l = cat.top.lower()
                if cat.id in kinds_override:
                    cat.section = kinds_override[cat.id]
                elif re.search(r"variable|field|propert", top_l):
                    cat.section = "variable"
                elif re.search(r"function|method", top_l):
                    cat.section = "function"
                else:
                    cat.section = "other"
                t.cats.append(cat)
                t.cat_by_start[cat.start] = cat
    if not t.cats:
        raise Unsupported("the template has no empty #region sections to sort code into")
    return t


def find_category(t, section, role):
    # Finds the category that suits a role such as inspector, reference, external, internal, inherited
    cats = [c for c in t.cats if c.section == section] or list(t.cats)
    for c in cats:
        if c.title.lower() == role:
            return c
    if role in ("inspector", "inherited"):
        return cats[0]
    if role == "reference":
        return cats[1] if len(cats) > 1 else cats[0]
    if role == "external":
        return cats[-2] if len(cats) > 1 else cats[0]
    return cats[-1]


# ---------------------------------------------------------------------------------------------------------------------
# Choosing default categories
# ---------------------------------------------------------------------------------------------------------------------

RULE_FIELDS = ("name", "kind", "access", "mods", "attrs", "type", "text")


def member_facts(m):
    type_text = collapse(re.sub(r"\b(public|private|protected|internal|static|readonly|const|volatile|new|event)\b", "", m.rest))
    first = re.match(r"([A-Za-z_][\w.]*)", type_text)
    tname = first.group(1).split(".")[-1] if first else ""
    return {"name": m.name, "kind": m.kind, "access": m.access, "mods": " ".join(sorted(m.mods)),
            "attrs": m.attrs, "type": tname, "text": type_text}


def condition_holds(cond, facts, settings):
    value = facts.get(cond.get("field"), "")
    pattern = cond.get("pattern", "")
    try:
        if pattern.startswith("@"):
            names = settings.get(pattern[1:]) or []
            hit = any(re.fullmatch(p, value) for p in names)
        else:
            hit = re.search(pattern, value) is not None
    except re.error:
        hit = False
    return (not hit) if cond.get("not") else hit


def pick_category_name(m, settings):
    # Returns the category name the placement rules choose, or the configured default
    facts = member_facts(m)
    for rule in settings.get("placement_rules", []):
        sec = rule.get("section", "any")
        if sec not in ("any", m.section):
            continue
        if all(condition_holds(c, facts, settings) for c in rule.get("when", [])):
            return rule.get("category", "")
    return settings.get("placement_default", "Internal")


def find_category_named(t, section, name):
    # Matches a category by its title or its full id, case-insensitively. None when the template has no such section.
    want = (name or "").strip().lower()
    cats = [c for c in t.cats if c.section == section] or list(t.cats)
    for c in cats:
        if c.id.lower() == want or c.title.lower() == want:
            return c
    return None


def default_role(m, settings, tmpl):
    return pick_category_name(m, settings)


def region_hint(m, tmpl):
    # If the code already sits in a region that matches a template section, use that one
    path = [p.lower() for p in m.region_path]
    if not path:
        return None
    ids = {c.id.lower(): c for c in tmpl.cats}
    if len(path) >= 2:
        key = "/".join(path[-2:])
        if key in ids:
            return ids[key]
    leaf = path[-1]
    matches = [c for c in tmpl.cats if c.title.lower() == leaf and c.section == m.section]
    if len(matches) == 1:
        return matches[0]
    return None


# ---------------------------------------------------------------------------------------------------------------------
# Plan
# ---------------------------------------------------------------------------------------------------------------------

class Plan:
    def __init__(self):
        self.parsed = None
        self.main = None
        self.members = []
        self.extras = []
        self.class_doc = ""
        self.authors = []
        self.contributors = []
        self.header_desc = ""
        self.header_desc_raw = []
        self.header_from_summary = False
        self.class_lead = []
        self.tmpl = None
        self.settings = None
        self.usings = []
        self.namespace = None


def build_plan(parsed, template, settings, main_name=None):
    plan = Plan()
    plan.parsed = parsed
    plan.tmpl = template
    plan.settings = settings
    stem = os.path.splitext(os.path.basename(parsed.filename))[0]
    classes = [t for t in parsed.types if t.kind in ("class", "struct")]
    main = None
    if main_name:
        for t in parsed.types:
            if t.name == main_name:
                main = t
    if main is None:
        for t in classes:
            if t.name == stem:
                main = t
                break
    if main is None and len(classes) == 1:
        main = classes[0]
    if main is None:
        for t in classes:
            if re.search(r":\s*[^{]*\b(MonoBehaviour|ScriptableObject)\b", t.decl):
                main = t
                break
    if main is None and classes:
        main = classes[0]
    if main is None:
        raise Unsupported("no class found to use as the main class")
    if main.kind not in ("class", "struct"):
        raise Unsupported("the main type is a %s, which the template can't hold" % main.kind)
    if main.cond:
        raise Unsupported("the main class is inside a #if block")
    plan.main = main
    members = parse_members_for(parsed, main, settings)
    plan.members = members
    for m in members:
        hint = region_hint(m, template)
        if hint is not None:
            m.category = hint.id
        else:
            role = default_role(m, settings, template)
            cat = find_category_named(template, m.section, role) or find_category(template, m.section, role.lower())
            m.category = cat.id
        m.doc = m.existing_text
    for t in parsed.types:
        if t is main:
            continue
        mem = t.member
        mem.order = 100000 + mem.order
        mem.placement = "below"
        mem.doc = mem.existing_text
        plan.extras.append(mem)
    plan.class_doc = main.member.existing_text
    if settings.get("carry_description", True):
        plan.header_desc_raw = list(parsed.description)
        plan.header_desc = "\n".join(re.sub(r"^\s*//\s?", "", x).strip() for x in parsed.description)
    plan.authors, plan.contributors = initial_people(parsed, template, settings)
    plan.header_from_summary = (not plan.header_desc.strip()) and bool(settings.get("header_description_from_summary"))
    plan.usings = list(parsed.usings)
    mode = settings.get("namespace_mode", "use")
    if mode == "none":
        plan.namespace = None
    elif mode == "keep":
        plan.namespace = parsed.namespace or settings.get("namespace") or None
    else:
        plan.namespace = settings.get("namespace") or None
    return plan


# ---------------------------------------------------------------------------------------------------------------------
# Rendering
# ---------------------------------------------------------------------------------------------------------------------

def doc_lines(fmt, text):
    return [ln.replace("{text}", text) for ln in fmt.split("\n")]


def style_for(m, cat, settings):
    ds = settings.get("doc_style", {})
    style = None
    if cat is not None:
        style = ds.get(cat.id) or ds.get(cat.title)
    if style is None or style == "auto":
        if m.section == "function" or m.kind in ("nested", "delegate"):
            style = "summary"
        else:
            style = "tooltip"
    if style == "tooltip":
        if m.kind == "property" and "[field:" in m.attrs:
            return "tooltip"
        if m.kind != "field" or {"const", "static", "readonly"} & m.mods:
            return "comment"
    return style


def apply_doc(m, text, style, settings):
    # Returns (lead lines, body lines, body flags). Flags mark lines inside a multi-line string.
    lead = list(m.lead)
    body = m.body.split("\n")
    body[0] = " " * m.indent0 + body[0]
    flags = [False] + [k in m.verb_rel for k in range(1, len(body))]
    text = (text or "").strip()
    if text == "" or text == m.existing_text:
        return lead, body, flags
    pad = " " * m.indent0
    fmt = settings["doc_formats"][style]
    if style == "tooltip":
        safe = text.replace("\\", "\\\\").replace('"', '\\"')
        if m.existing_style == "tooltip":
            joined = "\n".join(body)
            joined = TOOLTIP_RE.sub(lambda mo: mo.group(0).split("(")[0] + '("' + safe + '")]', joined, count=1)
            return lead, joined.split("\n"), flags
        new_lines = [pad + x for x in doc_lines(fmt, safe)]
        if m.kind == "property":
            new_lines = [x.replace("[Tooltip", "[field: Tooltip") for x in new_lines]
        idx = 0
        while idx < len(body) and re.match(r"\s*\[(Header|Space)\b", body[idx]):
            idx += 1
        body = body[:idx] + new_lines + body[idx:]
        flags = flags[:idx] + [False] * len(new_lines) + flags[idx:]
        return lead, body, flags
    new_lines = [pad + x for x in doc_lines(fmt, text)]
    if m.existing_style == style:
        while lead:
            s = lead[-1].strip()
            if (style == "summary" and s.startswith("///")) or \
                    (style == "comment" and s.startswith("//") and not s.startswith("///")):
                lead.pop()
            else:
                break
    return lead + new_lines, body, flags


def member_output(m, doc_text, style, target_indent, settings):
    lead, body, flags = apply_doc(m, doc_text, style, settings)
    delta = target_indent - m.indent0
    out = [shift_line(ln, delta) for ln in lead]
    for ln, fixed in zip(body, flags):
        out.append(ln if fixed else shift_line(ln, delta))
    if m.trail:
        out.append("")
        out.extend(shift_line(ln, delta) for ln in m.trail)
    return out


def merge_usings(plan, template, settings):
    seen = set()
    plain = []
    cond = {}
    items = [(u, None) for u in template.template_usings] + list(plan.usings)
    for text, c in items:
        key = (text, c)
        if key in seen:
            continue
        seen.add(key)
        if c is None:
            plain.append(text)
        else:
            cond.setdefault(c, []).append(text)
    if settings.get("sort_usings", True):
        def k(u):
            body = u[len("using"):].strip().rstrip(";").strip()
            is_static_or_alias = body.startswith("static ") or "=" in body
            return (1 if is_static_or_alias else 0, 0 if body.startswith("System") else 1, body.lower())
        plain.sort(key=k)
        for c in cond:
            cond[c].sort(key=k)
    lines = list(plain)
    for c, us in cond.items():
        lines.append("#if " + c)
        lines.extend(us)
        lines.append("#endif")
    return lines


def render(plan):
    tmpl = plan.tmpl
    settings = plan.settings
    parsed = plan.parsed
    L = tmpl.lines
    main = plan.main
    name = main.name if main is not None else plan.new_name
    out = []
    has_ns_tokens = tmpl.ns_begin is not None and tmpl.ns_end is not None
    ns = plan.namespace if has_ns_tokens else None
    delta_body = -tmpl.base_indent if (has_ns_tokens and not ns) else 0

    cats_members = {}
    if main is not None:
        for m in plan.members:
            cats_members.setdefault(m.category, []).append(m)
        for m in plan.extras:
            if m.placement and m.placement != "below":
                cats_members.setdefault(m.placement, []).append(m)
    cat_ids = {c.id for c in tmpl.cats}
    for cid in cats_members:
        if cid not in cat_ids:
            raise Unsupported("category '%s' is not in the template" % cid)

    desc = header_desc_lines(plan)
    i = 0
    n = len(L)
    while i < n:
        ln = L[i]
        inside_ns = has_ns_tokens and tmpl.ns_begin < i < tmpl.ns_end
        shift = delta_body if inside_ns else 0
        if i in tmpl.template_using_lines:
            i += 1
            continue
        if tmpl.usings_idx == i:
            out.extend(merge_usings(plan, tmpl, settings))
            i += 1
            continue
        if tmpl.ns_begin == i and has_ns_tokens:
            if ns:
                out.extend(["namespace " + ns, "{"])
            i += 1
            continue
        if tmpl.ns_end == i and has_ns_tokens:
            if ns:
                out.append("}")
            i += 1
            continue
        if tmpl.addl_idx == i:
            extras = [e for e in plan.extras if e.placement in ("", "below", None)] if main is not None else []
            if extras:
                target = expand_indent(ln) + shift
                for bi, e in enumerate(extras):
                    lines = member_output(e, e.doc, "summary", target, settings)
                    if e.cond:
                        pad = " " * target
                        lines = [pad + "#if " + e.cond] + lines + [pad + "#endif"]
                    if bi > 0:
                        out.extend([""] * max(1, settings.get("blank_between_types", 1)))
                    out.extend(lines)
            else:
                while out and out[-1] == "":
                    out.pop()
            i += 1
            continue
        if i == tmpl.class_line:
            indent = expand_indent(ln) + shift
            if main is None:
                out.append(shift_line(ln.replace("#SCRIPTNAME#", name), shift))
            else:
                mm = main.member
                lead = list(mm.lead)
                doc_text = (plan.class_doc or "").strip()
                if doc_text and doc_text != mm.existing_text:
                    if mm.existing_style == "summary":
                        while lead and lead[-1].strip().startswith("///"):
                            lead.pop()
                    lead += [" " * mm.indent0 + x for x in doc_lines(settings["doc_formats"]["summary"], doc_text)]
                delta = indent - mm.indent0
                out.extend(shift_line(x, delta) for x in lead)
                out.extend(" " * indent + a for a in main.attr_lines)
                out.append(" " * indent + main.decl)
            i += 1
            continue
        if i in tmpl.cat_by_start:
            cat = tmpl.cat_by_start[i]
            out.append(shift_line(L[i], shift) if shift else L[i])
            for k in range(i + 1, cat.end):
                out.append(shift_line(L[k], shift) if (shift and L[k].strip()) else L[k])
            members = sorted(cats_members.get(cat.id, []), key=lambda x: x.order)
            target = cat.indent + shift + settings.get("member_indent", 4)
            pad = " " * target
            blank = settings.get("blank_between_vars", 0) if cat.section == "variable" else settings.get("blank_between_funcs", 1)
            cur_cond = None
            prev = None
            for m in members:
                lines = member_output(m, m.doc, style_for(m, cat, settings), target, settings)
                if prev is not None:
                    if cur_cond is not None and m.cond != cur_cond:
                        out.append(pad + "#endif")
                        cur_cond = None
                    gap = blank
                    if m.kind in ("nested", "delegate") or prev.kind in ("nested", "delegate"):
                        gap = max(gap, 1)
                    out.extend([""] * gap)
                if m.cond and m.cond != cur_cond:
                    out.append(pad + "#if " + m.cond)
                    cur_cond = m.cond
                out.extend(lines)
                prev = m
            if cur_cond is not None:
                out.append(pad + "#endif")
            out.append(shift_line(L[cat.end], shift) if shift else L[cat.end])
            i = cat.end + 1
            continue
        line = ln.replace("#SCRIPTNAME#", name)
        out.append(shift_line(line, shift) if (shift and line.strip()) else line)
        i += 1
        if parsed is not None and heading_re(settings, "author").match(ln):
            i = replace_names(L, i, out, plan.authors, settings)
        elif parsed is not None and heading_re(settings, "contributors").match(ln):
            i = replace_names(L, i, out, plan.contributors, settings)
        if desc and heading_re(settings, "description").match(ln):
            out.extend(desc)
            desc = []
            if i < n and L[i].strip() in ("//", ""):
                i += 1
    while out and out[-1] == "":
        out.pop()
    return "\n".join(out) + "\n"


# ---------------------------------------------------------------------------------------------------------------------
# Checking the result
# ---------------------------------------------------------------------------------------------------------------------

def norm_code(masked):
    s = re.sub(r'\[(?:field:\s*)?Tooltip\(\s*"\s*"\s*\)\]', "", masked)
    s = re.sub(r"^\s*#\s*(region|endregion)\b.*$", "", s, flags=re.M)
    return re.sub(r"\s+", "", s)


def verify(plan, new_text):
    # Re-reads the result and checks that every member, extra class and using survived untouched
    parsed2 = parse_source(new_text, plan.parsed.filename, plan.settings)
    main_name = plan.main.name
    main2 = None
    for t in parsed2.types:
        if t.decl == plan.main.decl and t.name == main_name:
            main2 = t
            break
    if main2 is None:
        raise Unsupported("self-check: the main class is missing from the result")
    mem2 = parse_members_for(parsed2, main2, plan.settings)
    orig = [norm_code(mask_source(m.body)[0]) for m in plan.members if m.kind != "comment"]
    # extras placed inside the class are members of the result
    inside = [m for m in plan.extras if m.placement and m.placement != "below"]
    orig += [norm_code(mask_source(m.body)[0]) for m in inside]
    got = [norm_code(mask_source(m.body)[0]) for m in mem2 if m.kind != "comment"]
    if sorted(orig) != sorted(got):
        missing = [o[:60] for o in orig if o not in got]
        extra = [g[:60] for g in got if g not in orig]
        raise Unsupported("self-check: members differ (missing %s, extra %s)" % (missing[:2], extra[:2]))
    below = [norm_code(mask_source(m.body)[0]) for m in plan.extras if m.placement in ("", "below", None)]
    got_types = [norm_code(mask_source(t.member.body)[0]) for t in parsed2.types if t is not main2]
    if sorted(below) != sorted(got_types):
        raise Unsupported("self-check: the extra classes differ")
    u1 = sorted(set(plan.usings))
    u2 = sorted(set(parsed2.usings))
    for u in u1:
        if u not in u2:
            raise Unsupported("self-check: using statement lost: %s" % u[0])
    T2 = Text(new_text)
    if T2.masked.count("{") != T2.masked.count("}"):
        raise Unsupported("self-check: unbalanced braces")


# ---------------------------------------------------------------------------------------------------------------------
# Whole-file helpers
# ---------------------------------------------------------------------------------------------------------------------

def read_source(path):
    raw = open(path, "rb").read()
    bom = raw.startswith(b"\xef\xbb\xbf")
    text = raw.decode("utf-8-sig")
    crlf = "\r\n" in text
    if crlf and re.search(r"(?<!\r)\n", text):
        raise Unsupported("the file has mixed line endings")
    return text.replace("\r\n", "\n"), bom, crlf


def write_result(path, text, bom, crlf, backup):
    if backup:
        try:
            with open(path, "rb") as f:
                old = f.read()
            with open(path + ".bak", "wb") as f:
                f.write(old)
        except OSError:
            pass
    if crlf:
        text = text.replace("\n", "\r\n")
    data = text.encode("utf-8")
    if bom:
        data = b"\xef\xbb\xbf" + data
    with open(path, "wb") as f:
        f.write(data)


def convert_text(text, filename, settings, template, main_name=None):
    parsed = parse_source(text, filename, settings)
    plan = build_plan(parsed, template, settings, main_name)
    result = render(plan)
    verify(plan, result)
    return plan, result


def new_script(name, settings, template):
    plan = Plan()
    plan.tmpl = template
    plan.settings = settings
    plan.new_name = name
    plan.parsed = None
    plan.main = None
    mode = settings.get("namespace_mode", "use")
    plan.namespace = None if mode == "none" else (settings.get("namespace") or None)
    return render(plan)


# ---------------------------------------------------------------------------------------------------------------------
# How well a file already follows the template
# ---------------------------------------------------------------------------------------------------------------------

def compare_key(text):
    # Ignores blank lines, indentation and the order of using lines
    lines = [ln.strip() for ln in text.split("\n") if ln.strip()]
    usings = sorted(ln for ln in lines if re.match(r"using\s[^;()]*;$", ln))
    rest = [ln for ln in lines if not re.match(r"using\s[^;()]*;$", ln)]
    return usings, rest


def header_description_empty(T, settings=None):
    lines = T.lines
    head = heading_re(settings, "description")
    for idx, ln in enumerate(lines):
        if head.match(ln):
            for ln2 in lines[idx + 1:]:
                if re.match(r"^\s*//=", ln2) or not ln2.strip().startswith("//"):
                    break
                if ln2.strip().strip("/").strip():
                    return False
            return True
        if ln.strip() and not ln.strip().startswith("//"):
            break
    return False


def assess(text, filename, settings, template):
    # Returns ("green" | "yellow" | "red", [reasons])
    try:
        parsed = parse_source(text, filename, settings)
        plan = build_plan(parsed, template, settings)
    except Unsupported as e:
        return "red", ["can't be read automatically: %s" % e]
    reasons = []
    T = parsed.T
    main = plan.main
    first = T.line_of(main.open)
    last = T.line_of(main.close)
    found = set()
    for ln in range(first, last + 1):
        path = [x.lower() for x in parsed.dirs[1][ln]]
        for k in range(1, len(path) + 1):
            found.add("/".join(path[:k]))
    missing = [c.id for c in template.cats if c.id.lower() not in found]
    if missing:
        return "red", ["missing sections: " + ", ".join(missing)]
    loose = [m for m in plan.members if m.kind != "comment" and region_hint(m, template) is None]
    if loose:
        return "red", ["%d member(s) are not inside a section" % len(loose)]
    if template.ns_begin is not None and plan.namespace and parsed.namespace != plan.namespace:
        reasons.append("namespace is %s, expected %s" % (parsed.namespace or "none", plan.namespace))
    if header_end_line(T, settings) == -1:
        reasons.append("no header comment")
    elif header_description_empty(T, settings):
        reasons.append("header description is empty")
    cats = {c.id.lower(): c for c in template.cats}
    nodoc_v = nodoc_f = 0
    for m in plan.members:
        hint = region_hint(m, template)
        if hint is None or m.kind == "comment":
            continue
        title = hint.title.lower()
        skip_v = [x.lower() for x in settings.get("describe_variables_except", [])]
        need_f = [x.lower() for x in settings.get("describe_functions_in", [])]
        if m.section == "variable" and title not in skip_v and not m.existing_text:
            nodoc_v += 1
        if m.section == "function" and title in need_f and not m.existing_text:
            nodoc_f += 1
    if nodoc_v:
        reasons.append("%d variable(s) without a tooltip or comment" % nodoc_v)
    if nodoc_f:
        reasons.append("%d external function(s) without a summary" % nodoc_f)
    try:
        result = render(plan)
        if compare_key(result) != compare_key(text):
            reasons.append("layout differs from what the template would produce")
    except Unsupported as e:
        reasons.append("could not rebuild: %s" % e)
    return ("yellow" if reasons else "green"), reasons


# ---------------------------------------------------------------------------------------------------------------------
# Editable rule text
# ---------------------------------------------------------------------------------------------------------------------

def rules_to_text(rules):
    lines = []
    for r in rules:
        conds = " && ".join("%s %s %s" % (c["field"], "!~" if c.get("not") else "~", c["pattern"]) for c in r.get("when", []))
        lines.append("%s | %s | %s | %s" % (r.get("label", ""), r.get("section", "any"), r.get("category", ""), conds))
    return "\n".join(lines)


def text_to_rules(text):
    rules = []
    for n, raw in enumerate(text.split("\n"), start=1):
        ln = raw.strip()
        if not ln or ln.startswith("#"):
            continue
        parts = [x.strip() for x in ln.split("|", 3)]
        if len(parts) < 3:
            raise ValueError("line %d: write it as  label | section | category | conditions" % n)
        label, section, category = parts[0], parts[1].lower(), parts[2]
        if section not in ("variable", "function", "any"):
            raise ValueError("line %d: section must be variable, function or any" % n)
        if not category:
            raise ValueError("line %d: no category name" % n)
        conds = []
        if len(parts) == 4 and parts[3]:
            for chunk in parts[3].split(" && "):
                m = re.match(r"^\s*(\w+)\s*(!~|~)\s*(.+?)\s*$", chunk)
                if not m:
                    raise ValueError("line %d: can't read the condition '%s'. Use  field ~ pattern" % (n, chunk.strip()))
                if m.group(1) not in RULE_FIELDS:
                    raise ValueError("line %d: unknown field '%s'. Fields: %s" % (n, m.group(1), ", ".join(RULE_FIELDS)))
                pat = m.group(3)
                if not pat.startswith("@"):
                    try:
                        re.compile(pat)
                    except re.error as e:
                        raise ValueError("line %d: bad pattern '%s' (%s)" % (n, pat, e))
                conds.append({"field": m.group(1), "pattern": pat, "not": m.group(2) == "!~"})
        rules.append({"label": label, "section": section, "category": category, "when": conds})
    return rules


# ---------------------------------------------------------------------------------------------------------------------
# Window
# ---------------------------------------------------------------------------------------------------------------------

def run_gui(paths=None):
    import tkinter as tk
    import tkinter.font as tkfont
    import difflib
    from tkinter import ttk, filedialog, messagebox

    BG = "#1b1c20"
    PANEL = "#26282d"
    RAISED = "#33363d"
    FIELD = "#2f3238"
    FG = "#dadce0"
    MUTED = "#8e939b"
    ACCENT = "#6d7cff"
    LIST_COLORS = {
        "green": ("#1f4a2b", "#d6f5df"),
        "yellow": ("#5b4a12", "#fdf0c2"),
        "red": ("#5c2024", "#fbd6d8"),
        "gray": (FIELD, MUTED),
    }

    settings = load_settings()
    S = {"template": None, "tmpl_text": None, "files": [], "cur": None, "job": None, "tjob": None, "assess_job": None}

    root = tk.Tk()
    root.title("Riven Format")
    root.geometry("1560x880")
    root.configure(bg=BG)

    # ---- dark theme ----
    style = ttk.Style(root)
    try:
        style.theme_use("clam")
    except tk.TclError:
        pass
    style.configure(".", background=PANEL, foreground=FG, fieldbackground=FIELD, bordercolor=BG,
                    lightcolor=PANEL, darkcolor=PANEL, troughcolor=BG, focuscolor=PANEL, insertcolor=FG)
    style.configure("TFrame", background=PANEL)
    style.configure("Bg.TFrame", background=BG)
    style.configure("TLabel", background=PANEL, foreground=FG)
    style.configure("Muted.TLabel", background=PANEL, foreground=MUTED)
    style.configure("Bold.TLabel", background=PANEL, foreground=FG, font=("TkDefaultFont", 10, "bold"))
    style.configure("TButton", background=RAISED, foreground=FG, padding=(10, 5), relief="flat", borderwidth=0)
    style.map("TButton", background=[("active", "#444851"), ("disabled", PANEL)], foreground=[("disabled", MUTED)])
    style.configure("Accent.TButton", background=ACCENT, foreground="#ffffff")
    style.map("Accent.TButton", background=[("active", "#8591ff"), ("disabled", PANEL)])
    style.configure("TEntry", fieldbackground=FIELD, foreground=FG, insertcolor=FG, bordercolor=RAISED)
    style.configure("TCombobox", fieldbackground=FIELD, background=RAISED, foreground=FG, arrowcolor=FG,
                    selectbackground=FIELD, selectforeground=FG, bordercolor=RAISED)
    style.map("TCombobox", fieldbackground=[("readonly", FIELD)], foreground=[("readonly", FG)],
              selectbackground=[("readonly", FIELD)], selectforeground=[("readonly", FG)])
    style.configure("TSpinbox", fieldbackground=FIELD, foreground=FG, arrowcolor=FG, background=RAISED)
    style.configure("TCheckbutton", background=PANEL, foreground=FG)
    style.map("TCheckbutton", background=[("active", PANEL)])
    style.configure("TRadiobutton", background=PANEL, foreground=FG)
    style.map("TRadiobutton", background=[("active", PANEL)])
    style.configure("TNotebook", background=BG, borderwidth=0)
    style.configure("TNotebook.Tab", background=PANEL, foreground=MUTED, padding=(16, 7), borderwidth=0)
    style.map("TNotebook.Tab", background=[("selected", RAISED)], foreground=[("selected", FG)])
    style.configure("Vertical.TScrollbar", background=RAISED, troughcolor=BG, arrowcolor=FG, bordercolor=BG)
    style.configure("Horizontal.TScrollbar", background=RAISED, troughcolor=BG, arrowcolor=FG, bordercolor=BG)
    style.configure("TPanedwindow", background=BG)
    style.configure("Sash", sashthickness=6, background=BG)
    style.configure("TSeparator", background=RAISED)
    root.option_add("*TCombobox*Listbox.background", FIELD)
    root.option_add("*TCombobox*Listbox.foreground", FG)
    root.option_add("*TCombobox*Listbox.selectBackground", ACCENT)
    root.option_add("*TCombobox*Listbox.selectForeground", "#ffffff")

    mono = tkfont.nametofont("TkFixedFont").copy()
    mono.configure(size=10)

    def make_text(parent, **kw):
        t = tk.Text(parent, bg="#17181b", fg=FG, insertbackground=FG, selectbackground="#3d4a99",
                    relief="flat", highlightthickness=0, borderwidth=0, font=mono, wrap="none", **kw)
        return t

    def with_scroll(parent, widget, horizontal=True):
        vs = ttk.Scrollbar(parent, orient="vertical", command=widget.yview)
        widget.configure(yscrollcommand=vs.set)
        vs.pack(side="right", fill="y")
        if horizontal:
            hs = ttk.Scrollbar(parent, orient="horizontal", command=widget.xview)
            widget.configure(xscrollcommand=hs.set)
            hs.pack(side="bottom", fill="x")
        widget.pack(side="left", fill="both", expand=True)

    # ---- template ----

    def set_template_text(text):
        tmpl = parse_template(text, settings)
        S["template"] = tmpl
        S["tmpl_text"] = text
        return tmpl

    try:
        set_template_text(load_template_text(settings))
    except Unsupported:
        set_template_text(DEFAULT_TEMPLATE)

    # ---- layout ----
    toolbar = ttk.Frame(root, padding=(10, 8))
    toolbar.pack(fill="x")
    status_var = tk.StringVar(value="Add some scripts to get started.")

    main_pane = ttk.PanedWindow(root, orient="horizontal")
    main_pane.pack(fill="both", expand=True, padx=8)

    left = ttk.Frame(main_pane, padding=6)
    center = ttk.Frame(main_pane)
    right = ttk.Frame(main_pane, padding=6)
    main_pane.add(left, weight=1)
    main_pane.add(center, weight=3)
    main_pane.add(right, weight=3)

    nav = ttk.Frame(root, padding=(10, 8))
    nav.pack(fill="x")
    ttk.Label(root, textvariable=status_var, style="Muted.TLabel", padding=(12, 0, 12, 8)).pack(fill="x")

    # ---- file list ----
    legend = ttk.Frame(left)
    legend.pack(fill="x", pady=(0, 6))
    for name, text in (("green", "Matches"), ("yellow", "Missing some things"), ("red", "Doesn't follow it")):
        tk.Label(legend, text=" " + text + " ", bg=LIST_COLORS[name][0], fg=LIST_COLORS[name][1],
                 font=("TkDefaultFont", 8)).pack(side="left", padx=2)
    list_wrap = ttk.Frame(left)
    list_wrap.pack(fill="both", expand=True)
    lb = tk.Listbox(list_wrap, selectmode="browse", bg=FIELD, fg=FG, relief="flat", highlightthickness=0,
                    activestyle="none", selectbackground=ACCENT, selectforeground="#ffffff", exportselection=False)
    lvs = ttk.Scrollbar(list_wrap, orient="vertical", command=lb.yview)
    lb.configure(yscrollcommand=lvs.set)
    lvs.pack(side="right", fill="y")
    lb.pack(side="left", fill="both", expand=True)

    # ---- center notebook ----
    nb = ttk.Notebook(center)
    nb.pack(fill="both", expand=True)
    review_tab = ttk.Frame(nb)
    template_tab = ttk.Frame(nb)
    settings_tab = ttk.Frame(nb)
    nb.add(review_tab, text="Review")
    nb.add(template_tab, text="Template")
    nb.add(settings_tab, text="Settings")

    # ---- preview ----
    pv_head = ttk.Frame(right)
    pv_head.pack(fill="x", pady=(0, 6))
    ttk.Label(pv_head, text="Live preview", style="Bold.TLabel").pack(side="left")
    pv_mode = tk.StringVar(value="Result")
    pv_box = ttk.Combobox(pv_head, textvariable=pv_mode, values=["Result", "Original", "Changes"], state="readonly", width=10)
    pv_box.pack(side="right")
    pv_err = tk.StringVar(value="")
    tk.Label(right, textvariable=pv_err, bg=PANEL, fg="#ff8b8b", anchor="w", justify="left", wraplength=600).pack(fill="x")
    pv_wrap = ttk.Frame(right)
    pv_wrap.pack(fill="both", expand=True)
    pv = make_text(pv_wrap)
    with_scroll(pv_wrap, pv)
    pv.tag_configure("comment", foreground="#6a9955")
    pv.tag_configure("directive", foreground="#c586c0")
    pv.tag_configure("add", background="#1d3a29")
    pv.tag_configure("del", background="#47222a")
    pv.tag_configure("meta", foreground=MUTED)
    pv.configure(state="disabled")

    # =================================================================================================================
    # Files
    # =================================================================================================================

    def key_of(m):
        return "%s|%s|%s" % (m.kind, m.name, m.sig)

    def new_entry(path):
        return {"path": path, "text": None, "bom": False, "crlf": False, "parsed": None, "plan": None,
                "error": None, "choices": {}, "main": None, "level": "gray", "reasons": [], "stale": True}

    def current():
        i = S["cur"]
        if i is None or i >= len(S["files"]):
            return None
        return S["files"][i]

    def paint(i):
        e = S["files"][i]
        bg, fg = LIST_COLORS.get(e["level"], LIST_COLORS["gray"])
        lb.itemconfig(i, background=bg, foreground=fg, selectbackground=ACCENT, selectforeground="#ffffff")

    def load_text(e):
        text, bom, crlf = read_source(e["path"])
        e["text"], e["bom"], e["crlf"] = text, bom, crlf

    def rebuild(e):
        # Parses the file and builds the plan, keeping the choices already made
        e["plan"] = None
        e["parsed"] = None
        try:
            if e["text"] is None:
                load_text(e)
            parsed = parse_source(e["text"], e["path"], settings)
            plan = build_plan(parsed, S["template"], settings, e["main"])
            ch = e["choices"]
            valid = {c.id for c in S["template"].cats}
            for m in plan.members:
                c = ch.get(key_of(m))
                if c:
                    if c["cat"] in valid:
                        m.category = c["cat"]
                    m.doc = c["doc"]
            for x in plan.extras:
                c = ch.get("x:" + x.name)
                if c:
                    x.placement = c["place"] if (c["place"] in ("below", "") or c["place"] in valid) else "below"
                    x.doc = c["doc"]
            if "class_doc" in ch:
                plan.class_doc = ch["class_doc"]
            if "people" in ch:
                plan.authors = [x.strip() for x in ch["people"]["authors"].split("\n") if x.strip()]
                plan.contributors = [x.strip() for x in ch["people"]["contributors"].split("\n") if x.strip()]
            if "header" in ch:
                plan.header_desc = ch["header"]["text"]
                plan.header_from_summary = ch["header"]["same"]
            e["parsed"], e["plan"], e["error"] = parsed, plan, None
        except Unsupported as err:
            e["error"] = str(err)
        except Exception as err:
            e["error"] = "%s: %s" % (type(err).__name__, err)

    def refresh_level(e):
        if e["text"] is None:
            try:
                load_text(e)
            except Exception as err:
                e["level"], e["reasons"] = "red", ["can't read the file: %s" % err]
                e["stale"] = False
                return
        try:
            e["level"], e["reasons"] = assess(e["text"], e["path"], settings, S["template"])
        except Exception as err:
            e["level"], e["reasons"] = "red", ["%s: %s" % (type(err).__name__, err)]
        e["stale"] = False

    def assess_next():
        S["assess_job"] = None
        done = 0
        for i, e in enumerate(S["files"]):
            if e["stale"]:
                refresh_level(e)
                paint(i)
                done += 1
                if done >= 4:
                    break
        if any(e["stale"] for e in S["files"]):
            S["assess_job"] = root.after(15, assess_next)
        else:
            update_status_text()

    def start_assess():
        for e in S["files"]:
            e["stale"] = True
        if S["assess_job"] is None:
            S["assess_job"] = root.after(15, assess_next)

    def update_status_text():
        e = current()
        if e is None:
            counts = {"green": 0, "yellow": 0, "red": 0}
            for f in S["files"]:
                if f["level"] in counts:
                    counts[f["level"]] += 1
            status_var.set("%d green, %d yellow, %d red" % (counts["green"], counts["yellow"], counts["red"]))
            return
        why = "; ".join(e["reasons"]) if e["reasons"] else "looks right"
        status_var.set("%s  -  %s" % (os.path.basename(e["path"]), why))

    def add_paths(items):
        added = False
        known = {e["path"] for e in S["files"]}
        for f in collect_files(items):
            if f not in known:
                known.add(f)
                S["files"].append(new_entry(f))
                lb.insert("end", "  " + os.path.basename(f))
                paint(len(S["files"]) - 1)
                added = True
        if added:
            start_assess()
            if S["cur"] is None:
                go(0)

    def add_files():
        add_paths(list(filedialog.askopenfilenames(title="Choose scripts", filetypes=[("C# scripts", "*.cs")])))

    def add_folder():
        d = filedialog.askdirectory(title="Choose a folder")
        if d:
            add_paths([d])

    def remove_current():
        i = S["cur"]
        if i is None:
            return
        lb.delete(i)
        del S["files"][i]
        S["cur"] = None
        if S["files"]:
            go(min(i, len(S["files"]) - 1))
        else:
            show_review()
            refresh_preview()
            update_status_text()

    def clear_all():
        lb.delete(0, "end")
        S["files"] = []
        S["cur"] = None
        show_review()
        refresh_preview()
        update_status_text()

    # =================================================================================================================
    # Review tab
    # =================================================================================================================

    review_head = ttk.Frame(review_tab, padding=(10, 10, 10, 4))
    review_head.pack(fill="x")
    review_body = ttk.Frame(review_tab)
    review_body.pack(fill="both", expand=True, padx=8)
    rcanvas = tk.Canvas(review_body, bg=PANEL, highlightthickness=0)
    rvs = ttk.Scrollbar(review_body, orient="vertical", command=rcanvas.yview)
    rcanvas.configure(yscrollcommand=rvs.set)
    rvs.pack(side="right", fill="y")
    rcanvas.pack(side="left", fill="both", expand=True)
    rinner = ttk.Frame(rcanvas)
    rwin = rcanvas.create_window((0, 0), window=rinner, anchor="nw")
    rinner.bind("<Configure>", lambda ev: rcanvas.configure(scrollregion=rcanvas.bbox("all")))
    rcanvas.bind("<Configure>", lambda ev: rcanvas.itemconfigure(rwin, width=ev.width))

    def wheel(ev):
        if getattr(ev, "num", 0) == 4:
            rcanvas.yview_scroll(-3, "units")
        elif getattr(ev, "num", 0) == 5:
            rcanvas.yview_scroll(3, "units")
        else:
            rcanvas.yview_scroll(int(-ev.delta / 40) or (-1 if ev.delta > 0 else 1), "units")

    def bind_wheel(ev=None):
        rcanvas.bind_all("<MouseWheel>", wheel)
        rcanvas.bind_all("<Button-4>", wheel)
        rcanvas.bind_all("<Button-5>", wheel)

    def unbind_wheel(ev=None):
        rcanvas.unbind_all("<MouseWheel>")
        rcanvas.unbind_all("<Button-4>")
        rcanvas.unbind_all("<Button-5>")

    review_body.bind("<Enter>", bind_wheel)
    review_body.bind("<Leave>", unbind_wheel)

    rows = {"members": [], "extras": [], "main": None, "class_doc": None, "hdr_text": None, "hdr_same": None, "authors": None, "contribs": None}
    suppress = {"on": False}

    def collect():
        e = current()
        if e is None or e["plan"] is None or suppress["on"]:
            return
        pl = e["plan"]
        ch = e["choices"]
        if rows["class_doc"] is not None:
            pl.class_doc = rows["class_doc"].get()
            ch["class_doc"] = pl.class_doc
        if rows.get("authors") is not None:
            pl.authors = [x.strip() for x in rows["authors"].get("1.0", "end-1c").split("\n") if x.strip()]
            pl.contributors = [x.strip() for x in rows["contribs"].get("1.0", "end-1c").split("\n") if x.strip()]
            ch["people"] = {"authors": "\n".join(pl.authors), "contributors": "\n".join(pl.contributors)}
        if rows.get("hdr_text") is not None:
            pl.header_from_summary = bool(rows["hdr_same"].get())
            if not pl.header_from_summary:
                pl.header_desc = rows["hdr_text"].get("1.0", "end-1c")
            ch["header"] = {"text": pl.header_desc, "same": pl.header_from_summary}
        for m, cv, dv in rows["members"]:
            m.category = cv.get()
            m.doc = dv.get()
            ch[key_of(m)] = {"cat": m.category, "doc": m.doc}
        for x, pv_, dv in rows["extras"]:
            v = pv_.get()
            x.placement = "below" if v.startswith("Below") else v[len("Inside the class: "):]
            x.doc = dv.get()
            ch["x:" + x.name] = {"place": x.placement, "doc": x.doc}

    def schedule_preview(*_a):
        collect()
        if S["job"] is not None:
            root.after_cancel(S["job"])
        S["job"] = root.after(180, refresh_preview)

    def on_main_change(ev=None):
        e = current()
        if e is None or rows["main"] is None:
            return
        collect()
        e["main"] = rows["main"].get()
        e["choices"] = {}
        rebuild(e)
        show_review()
        refresh_preview()

    def show_review():
        for w in review_head.winfo_children():
            w.destroy()
        for w in rinner.winfo_children():
            w.destroy()
        rows["members"], rows["extras"] = [], []
        rows["main"] = None
        rows["class_doc"] = None
        rows["hdr_text"] = None
        rows["hdr_same"] = None
        rows["authors"] = None
        rows["contribs"] = None
        e = current()
        if e is None:
            ttk.Label(rinner, text="Add scripts with the buttons at the top, then pick one from the list.",
                      style="Muted.TLabel", padding=16).grid(row=0, column=0)
            return
        ttk.Label(review_head, text=e["path"], style="Muted.TLabel").grid(row=0, column=0, columnspan=4, sticky="w")
        if e["plan"] is None:
            tk.Label(rinner, text="This file can't be converted automatically:\n" + (e["error"] or "unknown problem"),
                     bg=PANEL, fg="#ff8b8b", justify="left", anchor="w", wraplength=520).grid(row=0, column=0, padx=16, pady=16, sticky="w")
            return
        pl = e["plan"]
        tmpl = S["template"]
        cat_ids = [c.id for c in tmpl.cats]
        func_ids = [c.id for c in tmpl.cats if c.section != "variable"] or cat_ids
        ttk.Label(review_head, text="Main class").grid(row=1, column=0, sticky="w", pady=6)
        names = [t.name for t in e["parsed"].types if t.kind in ("class", "struct")]
        mv = tk.StringVar(value=pl.main.name)
        mb = ttk.Combobox(review_head, textvariable=mv, values=names, state="readonly", width=26)
        mb.grid(row=1, column=1, sticky="w", padx=8)
        mb.bind("<<ComboboxSelected>>", on_main_change)
        rows["main"] = mv
        ttk.Label(review_head, text="Class summary").grid(row=1, column=2, sticky="e", padx=(14, 0))
        cdv = tk.StringVar(value=pl.class_doc)
        cdv.trace_add("write", schedule_preview)
        ttk.Entry(review_head, textvariable=cdv).grid(row=1, column=3, sticky="we", padx=8)
        review_head.columnconfigure(3, weight=1)
        rows["class_doc"] = cdv
        ttk.Label(review_head, text="Header description").grid(row=2, column=0, sticky="nw", pady=4)
        ht = tk.Text(review_head, height=3, bg=FIELD, fg=FG, insertbackground=FG, relief="flat", highlightthickness=0,
                     wrap="word", font=("TkDefaultFont", 9))
        ht.grid(row=2, column=1, columnspan=3, sticky="we", padx=8, pady=4)
        ht.insert("1.0", pl.header_desc)
        hs = tk.BooleanVar(value=pl.header_from_summary)

        def sync_hdr(*_a):
            ht.configure(state="disabled" if hs.get() else "normal")
            schedule_preview()

        hs.trace_add("write", sync_hdr)
        ht.bind("<KeyRelease>", schedule_preview)
        ttk.Checkbutton(review_head, text="Same as class summary", variable=hs).grid(row=3, column=1, columnspan=3, sticky="w", padx=8)
        rows["hdr_text"], rows["hdr_same"] = ht, hs
        if pl.header_from_summary:
            ht.configure(state="disabled")
        ttk.Label(review_head, text="Authors (one per line)").grid(row=4, column=0, sticky="nw", pady=(8, 0))
        at = tk.Text(review_head, height=2, bg=FIELD, fg=FG, insertbackground=FG, relief="flat", highlightthickness=0,
                     font=("TkDefaultFont", 9))
        at.grid(row=4, column=1, sticky="we", padx=8, pady=(8, 0))
        at.insert("1.0", "\n".join(pl.authors))
        at.bind("<KeyRelease>", schedule_preview)
        ttk.Label(review_head, text="Contributors").grid(row=4, column=2, sticky="ne", padx=(14, 0), pady=(8, 0))
        ct = tk.Text(review_head, height=2, bg=FIELD, fg=FG, insertbackground=FG, relief="flat", highlightthickness=0,
                     font=("TkDefaultFont", 9))
        ct.grid(row=4, column=3, sticky="we", padx=8, pady=(8, 0))
        ct.insert("1.0", "\n".join(pl.contributors))
        ct.bind("<KeyRelease>", schedule_preview)
        rows["authors"], rows["contribs"] = at, ct

        ttk.Label(rinner, text="Member", style="Bold.TLabel").grid(row=0, column=0, sticky="w", padx=(4, 6))
        ttk.Label(rinner, text="Goes in", style="Bold.TLabel").grid(row=0, column=1, sticky="w", padx=6)
        ttk.Label(rinner, text="Description (blank for none)", style="Bold.TLabel").grid(row=0, column=2, sticky="w", padx=6)
        rinner.columnconfigure(2, weight=1)
        r = 1
        for m in pl.members:
            ttk.Label(rinner, text=m.label(), width=38, anchor="w").grid(row=r, column=0, sticky="w", padx=(4, 6), pady=1)
            cv = tk.StringVar(value=m.category)
            cv.trace_add("write", schedule_preview)
            ttk.Combobox(rinner, textvariable=cv, values=cat_ids, state="readonly", width=22).grid(row=r, column=1, padx=6)
            dv = tk.StringVar(value=m.doc)
            dv.trace_add("write", schedule_preview)
            ttk.Entry(rinner, textvariable=dv).grid(row=r, column=2, sticky="we", padx=6)
            rows["members"].append((m, cv, dv))
            r += 1
        if pl.extras:
            ttk.Separator(rinner).grid(row=r, column=0, columnspan=3, sticky="we", pady=10)
            r += 1
            ttk.Label(rinner, text="Other classes in this file", style="Bold.TLabel").grid(row=r, column=0, sticky="w", padx=(4, 6))
            r += 1
            options = ["Below main class"] + ["Inside the class: " + i for i in func_ids]
            for x in pl.extras:
                ttk.Label(rinner, text=x.label(), width=38, anchor="w").grid(row=r, column=0, sticky="w", padx=(4, 6), pady=1)
                pvv = tk.StringVar(value="Below main class" if x.placement in ("", "below", None)
                                   else "Inside the class: " + x.placement)
                pvv.trace_add("write", schedule_preview)
                ttk.Combobox(rinner, textvariable=pvv, values=options, state="readonly", width=34).grid(row=r, column=1, padx=6)
                dv = tk.StringVar(value=x.doc)
                dv.trace_add("write", schedule_preview)
                ttk.Entry(rinner, textvariable=dv).grid(row=r, column=2, sticky="we", padx=6)
                rows["extras"].append((x, pvv, dv))
                r += 1

    # =================================================================================================================
    # Preview
    # =================================================================================================================

    def tag_preview(text, diff=False):
        for tag in ("comment", "directive", "add", "del", "meta"):
            pv.tag_remove(tag, "1.0", "end")
        for n, ln in enumerate(text.split("\n"), start=1):
            st = ln.strip()
            if diff and ln[:1] in ("+", "-") and not ln.startswith(("+++", "---")):
                pv.tag_add("add" if ln[0] == "+" else "del", "%d.0" % n, "%d.end" % n)
                st = ln[1:].strip()
            elif diff and ln.startswith(("@@", "+++", "---")):
                pv.tag_add("meta", "%d.0" % n, "%d.end" % n)
                continue
            if st.startswith("//"):
                pv.tag_add("comment", "%d.0" % n, "%d.end" % n)
            elif st.startswith("#"):
                pv.tag_add("directive", "%d.0" % n, "%d.end" % n)

    def set_preview(text, diff=False, editable=False):
        pv.configure(state="normal")
        pv.delete("1.0", "end")
        pv.insert("1.0", text)
        tag_preview(text, diff)
        pv.configure(state="normal" if editable else "disabled")

    def generated_result(e):
        # The text the template would produce, or None with the reason shown in the error line
        try:
            result = render(e["plan"])
            verify(e["plan"], result)
            e["result"] = result
            return result
        except Unsupported as err:
            pv_err.set("Can't build this file with these choices: %s" % err)
        except Exception as err:
            pv_err.set("%s: %s" % (type(err).__name__, err))
        e["result"] = None
        return None

    def refresh_preview(*_a):
        S["job"] = None
        e = current()
        pv_err.set("")
        if e is None:
            set_preview("")
            show_revert(False)
            return
        mode = pv_mode.get()
        manual = e.get("manual")
        show_revert(manual is not None)
        if e["plan"] is None and manual is None:
            set_preview(e["text"] or "")
            pv_err.set("Showing the original. " + (e["error"] or ""))
            return
        if mode == "Original":
            set_preview(e["text"])
            return
        if manual is not None:
            result = manual
            pv_err.set("You edited this by hand, so changes in the Review tab are paused. Press 'Undo my edits' to go back to the generated version.")
        else:
            result = generated_result(e)
            if result is None:
                set_preview(e["text"])
                return
        if mode == "Changes":
            diff = "\n".join(difflib.unified_diff(e["text"].split("\n"), result.split("\n"),
                                                  "original", "result", n=2, lineterm=""))
            set_preview(diff or "(no changes)", diff=True)
        else:
            set_preview(result, editable=True)

    def on_preview_edit(ev=None):
        e = current()
        if e is None or pv_mode.get() != "Result":
            return
        text = pv.get("1.0", "end-1c")
        if e.get("manual") is None:
            base = e.get("result")
            if base is None or text == base.rstrip("\n") or text == base:
                return
            show_revert(True)
            pv_err.set("You edited this by hand, so changes in the Review tab are paused. Press 'Undo my edits' to go back to the generated version.")
        e["manual"] = text if text.endswith("\n") else text + "\n"
        tag_preview(text)

    def undo_manual():
        e = current()
        if e is None:
            return
        e["manual"] = None
        refresh_preview()

    revert_btn = ttk.Button(pv_head, text="Undo my edits", command=undo_manual)

    def show_revert(on):
        if on:
            revert_btn.pack(side="right", padx=8)
        else:
            revert_btn.pack_forget()

    pv.bind("<KeyRelease>", on_preview_edit)
    ttk.Label(pv_head, text="(Result can be edited directly)", style="Muted.TLabel").pack(side="left", padx=10)

    pv_box.bind("<<ComboboxSelected>>", lambda ev: refresh_preview())

    # =================================================================================================================
    # Navigation and saving
    # =================================================================================================================

    nav_label = tk.StringVar(value="")

    def go(i):
        if not S["files"]:
            return
        i = max(0, min(i, len(S["files"]) - 1))
        collect()
        S["cur"] = i
        e = S["files"][i]
        if e["plan"] is None and e["error"] is None:
            rebuild(e)
        lb.selection_clear(0, "end")
        lb.selection_set(i)
        lb.see(i)
        nav_label.set("File %d of %d" % (i + 1, len(S["files"])))
        show_review()
        refresh_preview()
        update_status_text()

    def on_list_select(ev=None):
        sel = lb.curselection()
        if sel and sel[0] != S["cur"]:
            go(sel[0])

    lb.bind("<<ListboxSelect>>", on_list_select)

    def write_entry(e):
        # Builds and writes the file. Returns True when it was written.
        manual = e.get("manual")
        if manual is not None:
            result = manual
        else:
            if e["plan"] is None:
                return False
            result = render(e["plan"])
            verify(e["plan"], result)
        if result != e["text"]:
            write_result(e["path"], result, e["bom"], e["crlf"], settings.get("backup", False))
        e["text"] = result
        e["manual"] = None
        e["choices"] = {}
        e["main"] = None
        rebuild(e)
        refresh_level(e)
        return True

    def save_current(advance=False):
        e = current()
        if e is None:
            return
        collect()
        try:
            write_entry(e)
        except Unsupported as err:
            messagebox.showerror("Can't save", str(err))
            return
        paint(S["cur"])
        status_var.set("Saved " + os.path.basename(e["path"]))
        if advance and S["cur"] + 1 < len(S["files"]):
            go(S["cur"] + 1)
        else:
            show_review()
            refresh_preview()

    def save_all():
        if not S["files"]:
            return
        collect()
        if not messagebox.askyesno("Save all", "Write all %d files using your choices (defaults where you haven't looked)?" % len(S["files"])):
            return
        done = failed = 0
        for i, e in enumerate(S["files"]):
            if e["plan"] is None and e["error"] is None:
                rebuild(e)
            try:
                if write_entry(e):
                    done += 1
            except Unsupported:
                failed += 1
            paint(i)
        status_var.set("Saved %d file(s). %d couldn't be converted." % (done, failed + sum(1 for x in S["files"] if x["plan"] is None)))
        show_review()
        refresh_preview()

    ttk.Button(nav, text="< Back", command=lambda: go((S["cur"] or 0) - 1)).pack(side="left")
    ttk.Button(nav, text="Next >", command=lambda: go((S["cur"] or 0) + 1)).pack(side="left", padx=6)
    ttk.Label(nav, textvariable=nav_label, style="Muted.TLabel").pack(side="left", padx=10)
    ttk.Button(nav, text="Save all", command=save_all).pack(side="right")
    ttk.Button(nav, text="Save and next", command=lambda: save_current(True)).pack(side="right", padx=6)
    ttk.Button(nav, text="Save this file", style="Accent.TButton", command=lambda: save_current(False)).pack(side="right")

    ttk.Button(toolbar, text="Add files...", command=add_files).pack(side="left")
    ttk.Button(toolbar, text="Add folder...", command=add_folder).pack(side="left", padx=6)
    ttk.Button(toolbar, text="Remove", command=remove_current).pack(side="left")
    ttk.Button(toolbar, text="Clear", command=clear_all).pack(side="left", padx=6)

    new_name = tk.StringVar(value="")

    def create_new():
        name = new_name.get().strip()
        if not re.fullmatch(r"[A-Za-z_]\w*", name):
            messagebox.showerror("New script", "Type a class name first (letters, numbers and underscores).")
            return
        path = filedialog.asksaveasfilename(initialfile=name + ".cs", defaultextension=".cs",
                                            filetypes=[("C# scripts", "*.cs")])
        if not path:
            return
        write_result(path, new_script(name, settings, S["template"]), False, False, False)
        new_name.set("")
        add_paths([path])
        status_var.set("Created " + path)

    ttk.Button(toolbar, text="Create", command=create_new).pack(side="right")
    ttk.Entry(toolbar, textvariable=new_name, width=22).pack(side="right", padx=6)
    ttk.Label(toolbar, text="New script from template:").pack(side="right")

    # =================================================================================================================
    # Template tab
    # =================================================================================================================

    HELP = """HOW THE TEMPLATE WORKS

The template is the skeleton every script is rebuilt into. Edit it on the left; the preview on the right updates as you type. Nothing is written to your scripts until you save them.

TOKENS (each on its own line)
#USINGS#
  Where all using statements go. Any "using X;" lines you write in the template are merged in too.
#ROOTNAMESPACEBEGIN#  and  #ROOTNAMESPACEEND#
  The namespace wrapper. Everything between them is written already indented for the namespace. If you turn the namespace off in Settings, that indent is removed again.
#SCRIPTNAME#
  The name of the main class.
#ADDITIONAL CLASSES#
  Where the other classes, structs and enums from the file go. If there are none, the line is dropped.

THE CLASS LINE
The line containing "class #SCRIPTNAME#" is replaced with the real declaration from the script: modifiers (public, static, partial), generics, everything after the colon, and any attributes above it. Use it to set the indent. For a brand new script the line is used as you typed it.

SECTIONS
Every #region ... #endregion pair that has no other #region inside it is a section that code can be sorted into. The text inside the brackets of the #region line is the section's name, for example ( Inspector ). The region around them, for example ( Variables ), decides what kind of code the sections hold:
  a name containing "variable", "field" or "propert"  ->  variables
  a name containing "function" or "method"           ->  functions
You can rename, add, remove or reorder sections freely. Which kind of description each section gets (tooltip, comment or summary) is set in Settings.
Empty sections are always kept in the output.

HOW CODE IS SORTED
Nothing is built in. Settings has a "Where code goes" box with one rule per line, checked top to bottom, first match wins. Members that match nothing go to the default category. If the script already has regions with the same names as yours, its members stay in the region they are in. You can still change any single member in the Review tab.

HEADER
The header comment is copied from your template. The old Description, Author and Contributors are carried over automatically (turn that off in Settings). You can set the header description, authors and contributors per file in the Review tab, or tick "Same as class summary" to copy the class summary into it. The heading names it looks for are in Settings.

DESCRIPTIONS
Variables get a tooltip, functions and classes get a summary. How each one is written is set in Settings, under the formats."""

    t_top = ttk.Frame(template_tab, padding=(10, 10, 10, 4))
    t_top.pack(fill="x")
    t_status = tk.StringVar(value="")
    t_status_label = tk.Label(t_top, textvariable=t_status, bg=PANEL, fg="#7ee0a0", anchor="w", justify="left")
    t_status_label.pack(side="left", fill="x", expand=True)
    t_split = ttk.PanedWindow(template_tab, orient="horizontal")
    t_split.pack(fill="both", expand=True, padx=8)
    t_edit_wrap = ttk.Frame(t_split)
    t_help_wrap = ttk.Frame(t_split)
    t_split.add(t_edit_wrap, weight=3)
    t_split.add(t_help_wrap, weight=2)
    t_edit = make_text(t_edit_wrap, undo=True)
    with_scroll(t_edit_wrap, t_edit)
    t_help = tk.Text(t_help_wrap, bg=PANEL, fg=FG, relief="flat", highlightthickness=0, wrap="word",
                     font=("TkDefaultFont", 9), padx=8, pady=4)
    with_scroll(t_help_wrap, t_help, horizontal=False)
    t_help.insert("1.0", HELP)
    t_help.configure(state="disabled")
    t_edit.insert("1.0", S["tmpl_text"])
    t_state = {"saved": S["tmpl_text"]}

    def describe_template(tmpl):
        parts = ["%s (%s)" % (c.id, c.section) for c in tmpl.cats]
        extra = "" if t_edit.get("1.0", "end-1c") == t_state["saved"] else "   [unsaved]"
        return "Template OK. Sections: " + ", ".join(parts) + extra

    def apply_template_text():
        S["tjob"] = None
        text = t_edit.get("1.0", "end-1c")
        try:
            tmpl = parse_template(text, settings)
        except Unsupported as err:
            t_status.set("Template problem: %s" % err)
            t_status_label.configure(fg="#ff8b8b")
            return
        S["template"] = tmpl
        S["tmpl_text"] = text
        t_status.set(describe_template(tmpl))
        t_status_label.configure(fg="#7ee0a0")
        build_style_rows()
        e = current()
        if e is not None:
            rebuild(e)
            show_review()
        refresh_preview()
        start_assess()

    def on_template_key(ev=None):
        if S["tjob"] is not None:
            root.after_cancel(S["tjob"])
        S["tjob"] = root.after(350, apply_template_text)

    t_edit.bind("<KeyRelease>", on_template_key)

    def insert_token(tok):
        t_edit.insert("insert", tok)
        on_template_key()

    def save_template():
        text = t_edit.get("1.0", "end-1c")
        try:
            parse_template(text, settings)
        except Unsupported as err:
            messagebox.showerror("Template problem", str(err))
            return
        path = settings.get("template_path") or TEMPLATE_PATH
        with open(path, "w", encoding="utf-8") as f:
            f.write(text)
        t_state["saved"] = text
        apply_template_text()
        status_var.set("Template saved to " + path)

    def reset_template():
        if messagebox.askyesno("Reset", "Replace the template text with the built-in one?"):
            t_edit.delete("1.0", "end")
            t_edit.insert("1.0", DEFAULT_TEMPLATE)
            apply_template_text()

    def reload_template():
        t_edit.delete("1.0", "end")
        t_edit.insert("1.0", load_template_text(settings))
        t_state["saved"] = t_edit.get("1.0", "end-1c")
        apply_template_text()

    t_btns = ttk.Frame(template_tab, padding=(10, 6))
    t_btns.pack(fill="x")
    ttk.Button(t_btns, text="Save template", style="Accent.TButton", command=save_template).pack(side="left")
    ttk.Button(t_btns, text="Reload from file", command=reload_template).pack(side="left", padx=6)
    ttk.Button(t_btns, text="Reset to built-in", command=reset_template).pack(side="left")
    for tok in ("#USINGS#", "#ROOTNAMESPACEBEGIN#", "#ROOTNAMESPACEEND#", "#SCRIPTNAME#", "#ADDITIONAL CLASSES#"):
        ttk.Button(t_btns, text=tok, command=lambda t_=tok: insert_token(t_)).pack(side="right", padx=2)
    ttk.Label(t_btns, text="Insert:", style="Muted.TLabel").pack(side="right", padx=4)
    t_status.set(describe_template(S["template"]))

    # =================================================================================================================
    # Settings tab
    # =================================================================================================================

    sc = tk.Canvas(settings_tab, bg=PANEL, highlightthickness=0)
    svs = ttk.Scrollbar(settings_tab, orient="vertical", command=sc.yview)
    sc.configure(yscrollcommand=svs.set)
    svs.pack(side="right", fill="y")
    sc.pack(side="left", fill="both", expand=True)
    sf = ttk.Frame(sc, padding=12)
    swin = sc.create_window((0, 0), window=sf, anchor="nw")
    sf.bind("<Configure>", lambda ev: sc.configure(scrollregion=sc.bbox("all")))
    sc.bind("<Configure>", lambda ev: sc.itemconfigure(swin, width=ev.width))
    sf.columnconfigure(1, weight=1)
    pad = {"padx": 6, "pady": 3}
    sv = {}

    soon = {"job": None}

    def settings_soon(*_a):
        if soon["job"] is not None:
            root.after_cancel(soon["job"])
        soon["job"] = root.after(300, settings_changed)

    def settings_changed(*_a):
        soon["job"] = None
        settings["namespace_mode"] = sv["ns_mode"].get()
        settings["namespace"] = sv["ns_name"].get().strip()
        for k in ("remove_banners", "carry_description", "sort_usings", "backup"):
            settings[k] = bool(sv[k].get())
        for k in ("member_indent", "blank_between_vars", "blank_between_funcs", "blank_between_types"):
            try:
                settings[k] = int(sv[k].get())
            except (ValueError, tk.TclError):
                pass
        for k in ("tooltip", "comment", "summary"):
            settings["doc_formats"][k] = sv["fmt_" + k].get().replace("\\n", "\n")
        ds = {}
        for cid, v in sv.get("styles", {}).items():
            ds[cid] = v.get()
        settings["doc_style"] = ds
        h = settings["header"]
        for k in ("detect", "author", "contributors", "description", "description_line", "name_line"):
            val = sv["hdr_" + k].get()
            if val.strip():
                if k not in ("description_line", "name_line"):
                    try:
                        re.compile(val)
                    except re.error:
                        continue
                h[k] = val
        settings["header_description_from_summary"] = bool(sv["hdr_from_summary"].get())
        settings["header_author_mode"] = sv["author_mode"].get()
        settings["placement_default"] = sv["place_default"].get().strip() or "Internal"
        settings["describe_variables_except"] = [x.strip() for x in sv["desc_v"].get().split(",") if x.strip()]
        settings["describe_functions_in"] = [x.strip() for x in sv["desc_f"].get().split(",") if x.strip()]
        e = current()
        if e is not None:
            rebuild(e)
            show_review()
        refresh_preview()
        start_assess()

    r = 0
    sv["ns_mode"] = tk.StringVar(value=settings.get("namespace_mode", "use"))
    sv["ns_name"] = tk.StringVar(value=settings.get("namespace", ""))
    ttk.Label(sf, text="Namespace", style="Bold.TLabel").grid(row=r, column=0, sticky="w", **pad)
    nsf = ttk.Frame(sf)
    nsf.grid(row=r, column=1, sticky="w")
    ttk.Radiobutton(nsf, text="Use", variable=sv["ns_mode"], value="use").pack(side="left")
    ttk.Entry(nsf, textvariable=sv["ns_name"], width=22).pack(side="left", padx=4)
    ttk.Radiobutton(nsf, text="Keep the file's own", variable=sv["ns_mode"], value="keep").pack(side="left", padx=8)
    ttk.Radiobutton(nsf, text="None", variable=sv["ns_mode"], value="none").pack(side="left")
    r += 1
    for key, label in (("remove_banners", "Remove old section banner comments (//=----=)"),
                       ("carry_description", "Keep the old header's description and contributors"),
                       ("sort_usings", "Sort using statements (System first)"),
                       ("backup", "Make a .bak copy before overwriting a file")):
        sv[key] = tk.BooleanVar(value=bool(settings.get(key, False)))
        ttk.Checkbutton(sf, text=label, variable=sv[key]).grid(row=r, column=0, columnspan=2, sticky="w", **pad)
        r += 1
    ttk.Separator(sf).grid(row=r, column=0, columnspan=2, sticky="we", pady=8)
    r += 1
    for key, label in (("member_indent", "Extra indent for code inside a section"),
                       ("blank_between_vars", "Blank lines between variables"),
                       ("blank_between_funcs", "Blank lines between functions"),
                       ("blank_between_types", "Blank lines between extra classes")):
        sv[key] = tk.IntVar(value=int(settings.get(key, 0)))
        ttk.Label(sf, text=label).grid(row=r, column=0, sticky="w", **pad)
        ttk.Spinbox(sf, from_=0, to=12, textvariable=sv[key], width=5, command=settings_changed).grid(row=r, column=1, sticky="w", padx=6)
        r += 1
    ttk.Separator(sf).grid(row=r, column=0, columnspan=2, sticky="we", pady=8)
    r += 1
    ttk.Label(sf, text="How descriptions are written ({text} is what you type, \\n is a new line)", style="Bold.TLabel").grid(row=r, column=0, columnspan=2, sticky="w", **pad)
    r += 1
    for key, label in (("tooltip", "Tooltip"), ("comment", "Comment"), ("summary", "Summary")):
        sv["fmt_" + key] = tk.StringVar(value=settings["doc_formats"].get(key, "").replace("\n", "\\n"))
        ttk.Label(sf, text=label).grid(row=r, column=0, sticky="w", **pad)
        ttk.Entry(sf, textvariable=sv["fmt_" + key]).grid(row=r, column=1, sticky="we", padx=6)
        r += 1
    ttk.Separator(sf).grid(row=r, column=0, columnspan=2, sticky="we", pady=8)
    r += 1
    ttk.Label(sf, text="Which kind of description each section gets", style="Bold.TLabel").grid(row=r, column=0, columnspan=2, sticky="w", **pad)
    r += 1
    styles_frame = ttk.Frame(sf)
    styles_frame.grid(row=r, column=0, columnspan=2, sticky="we")
    styles_frame.columnconfigure(0, weight=1)
    r += 1
    sv["styles"] = {}

    def build_style_rows():
        for w in styles_frame.winfo_children():
            w.destroy()
        sv["styles"] = {}
        for k, c in enumerate(S["template"].cats):
            ds0 = settings.get("doc_style", {})
            cur = ds0.get(c.id) or ds0.get(c.title) or "auto"
            v = tk.StringVar(value=cur)
            v.trace_add("write", settings_changed)
            sv["styles"][c.id] = v
            ttk.Label(styles_frame, text=c.id).grid(row=k, column=0, sticky="w", **pad)
            ttk.Combobox(styles_frame, textvariable=v, values=["auto", "tooltip", "comment", "summary"],
                         state="readonly", width=12).grid(row=k, column=1, sticky="e", padx=6)

    build_style_rows()
    ttk.Separator(sf).grid(row=r, column=0, columnspan=2, sticky="we", pady=8)
    r += 1
    # ---- header comment ----
    ttk.Label(sf, text="Header comment (headings are regexes matched against the text after //)", style="Bold.TLabel").grid(row=r, column=0, columnspan=2, sticky="w", **pad)
    r += 1
    hd = settings["header"]
    for key, label in (("author", "Author heading"), ("contributors", "Contributors heading"),
                       ("description", "Description heading"), ("detect", "A comment at the top is a header if it matches"),
                       ("description_line", "How a description line is written ({text})"),
                       ("name_line", "How an author or contributor line is written ({text})")):
        sv["hdr_" + key] = tk.StringVar(value=hd.get(key, ""))
        ttk.Label(sf, text=label).grid(row=r, column=0, sticky="w", **pad)
        ttk.Entry(sf, textvariable=sv["hdr_" + key]).grid(row=r, column=1, sticky="we", padx=6)
        r += 1
    sv["author_mode"] = tk.StringVar(value=settings.get("header_author_mode", "keep"))
    ttk.Label(sf, text="When a file already has an author").grid(row=r, column=0, sticky="w", **pad)
    amf = ttk.Frame(sf)
    amf.grid(row=r, column=1, sticky="w")
    ttk.Radiobutton(amf, text="Keep it", variable=sv["author_mode"], value="keep").pack(side="left")
    ttk.Radiobutton(amf, text="Use the template's author, move the old one to contributors",
                    variable=sv["author_mode"], value="template").pack(side="left", padx=8)
    r += 1
    sv["hdr_from_summary"] = tk.BooleanVar(value=bool(settings.get("header_description_from_summary")))
    ttk.Checkbutton(sf, text="Files with no header description start with it set to copy the class summary",
                    variable=sv["hdr_from_summary"]).grid(row=r, column=0, columnspan=2, sticky="w", **pad)
    r += 1
    ttk.Separator(sf).grid(row=r, column=0, columnspan=2, sticky="we", pady=8)
    r += 1

    # ---- placement rules ----
    ttk.Label(sf, text="Where code goes", style="Bold.TLabel").grid(row=r, column=0, columnspan=2, sticky="w", **pad)
    r += 1
    ttk.Label(sf, style="Muted.TLabel", justify="left", wraplength=620,
              text="One rule per line, checked top to bottom. The first rule that matches wins.\n"
                   "label | section | category | field ~ pattern && field !~ pattern\n"
                   "section: variable, function or any.  category: a section name from your template.\n"
                   "fields: name, kind, access, mods, attrs, type, text.  ~ means 'matches', !~ means 'does not match'.\n"
                   "A pattern is a regex. A pattern like @unity_messages checks against that list (edit lists below).\n"
                   "Lines starting with # are ignored. Delete a line to remove a rule.").grid(row=r, column=0, columnspan=2, sticky="w", padx=6)
    r += 1
    rules_box = tk.Text(sf, height=16, bg=FIELD, fg=FG, insertbackground=FG, relief="flat", highlightthickness=0,
                        wrap="none", font=mono, undo=True)
    rules_box.grid(row=r, column=0, columnspan=2, sticky="we", **pad)
    rules_box.insert("1.0", rules_to_text(settings.get("placement_rules", [])))
    r += 1
    rules_msg = tk.StringVar(value="")
    tk.Label(sf, textvariable=rules_msg, bg=PANEL, fg="#ff8b8b", anchor="w", justify="left", wraplength=620).grid(row=r, column=0, columnspan=2, sticky="w", padx=6)
    r += 1
    sv["place_default"] = tk.StringVar(value=settings.get("placement_default", "Internal"))
    ttk.Label(sf, text="If no rule matches, use").grid(row=r, column=0, sticky="w", **pad)
    ttk.Entry(sf, textvariable=sv["place_default"]).grid(row=r, column=1, sticky="we", padx=6)
    r += 1

    def reset_rules():
        if messagebox.askyesno("Reset rules", "Replace the rules with the built-in ones?"):
            rules_box.delete("1.0", "end")
            rules_box.insert("1.0", rules_to_text(DEFAULT_RULES))
            apply_rules()

    def apply_rules(ev=None):
        try:
            settings["placement_rules"] = text_to_rules(rules_box.get("1.0", "end-1c"))
        except ValueError as err:
            rules_msg.set(str(err))
            return
        rules_msg.set("")
        settings_soon()

    rules_box.bind("<KeyRelease>", apply_rules)
    ttk.Button(sf, text="Reset rules to built-in", command=reset_rules).grid(row=r, column=0, sticky="w", padx=6, pady=4)
    r += 1
    ttk.Separator(sf).grid(row=r, column=0, columnspan=2, sticky="we", pady=8)
    r += 1

    # ---- lists ----
    ttk.Label(sf, text="Lists (one entry per line). Rules use them as @name.", style="Bold.TLabel").grid(row=r, column=0, columnspan=2, sticky="w", **pad)
    r += 1

    def list_names():
        return sorted(k for k, v in settings.items() if isinstance(v, list) and all(isinstance(x, str) for x in v)
                      and k not in ("describe_variables_except", "describe_functions_in"))

    list_var = tk.StringVar(value="unity_messages")
    lf = ttk.Frame(sf)
    lf.grid(row=r, column=0, columnspan=2, sticky="we", padx=6)
    list_box = ttk.Combobox(lf, textvariable=list_var, values=list_names(), width=28)
    list_box.pack(side="left")
    ttk.Label(lf, text="  type a new name to make a new list", style="Muted.TLabel").pack(side="left")
    r += 1
    list_text = tk.Text(sf, height=8, bg=FIELD, fg=FG, insertbackground=FG, relief="flat", highlightthickness=0, wrap="none", font=mono)
    list_text.grid(row=r, column=0, columnspan=2, sticky="we", **pad)
    r += 1

    def load_list(ev=None):
        list_text.delete("1.0", "end")
        list_text.insert("1.0", "\n".join(settings.get(list_var.get(), [])))

    def edit_list(ev=None):
        name = list_var.get().strip()
        if not re.fullmatch(r"[A-Za-z_]\w*", name):
            return
        settings[name] = [x.strip() for x in list_text.get("1.0", "end-1c").split("\n") if x.strip()]
        list_box.configure(values=list_names())
        settings_soon()

    list_box.bind("<<ComboboxSelected>>", load_list)
    list_text.bind("<KeyRelease>", edit_list)
    load_list()
    ttk.Separator(sf).grid(row=r, column=0, columnspan=2, sticky="we", pady=8)
    r += 1

    # ---- what counts as described ----
    ttk.Label(sf, text="Which sections should have descriptions (for the yellow check, comma separated)", style="Bold.TLabel").grid(row=r, column=0, columnspan=2, sticky="w", **pad)
    r += 1
    sv["desc_v"] = tk.StringVar(value=", ".join(settings.get("describe_variables_except", [])))
    ttk.Label(sf, text="Variables, in every section except").grid(row=r, column=0, sticky="w", **pad)
    ttk.Entry(sf, textvariable=sv["desc_v"]).grid(row=r, column=1, sticky="we", padx=6)
    r += 1
    sv["desc_f"] = tk.StringVar(value=", ".join(settings.get("describe_functions_in", [])))
    ttk.Label(sf, text="Functions, only in").grid(row=r, column=0, sticky="w", **pad)
    ttk.Entry(sf, textvariable=sv["desc_f"]).grid(row=r, column=1, sticky="we", padx=6)
    r += 1
    sv["template_path"] = tk.StringVar(value=settings.get("template_path", ""))
    ttk.Label(sf, text="Template file (blank = next to this script)").grid(row=r, column=0, sticky="w", **pad)
    ttk.Entry(sf, textvariable=sv["template_path"]).grid(row=r, column=1, sticky="we", padx=6)
    r += 1

    def save_settings_now():
        settings["template_path"] = sv["template_path"].get().strip()
        settings_changed()
        save_settings(settings)
        status_var.set("Settings saved to " + CONFIG_PATH)

    ttk.Button(sf, text="Save settings to file", style="Accent.TButton", command=save_settings_now).grid(row=r, column=0, sticky="w", padx=6, pady=12)
    ttk.Label(sf, text="Changes already apply live. Saving keeps them for next time.", style="Muted.TLabel").grid(row=r, column=1, sticky="w")

    for k in ("hdr_author", "hdr_contributors", "hdr_description", "hdr_detect", "hdr_description_line", "hdr_name_line",
              "hdr_from_summary", "author_mode", "place_default", "desc_v", "desc_f"):
        sv[k].trace_add("write", settings_soon)
    for k in ("ns_mode", "ns_name", "remove_banners", "carry_description", "sort_usings", "backup",
              "member_indent", "blank_between_vars", "blank_between_funcs", "blank_between_types",
              "fmt_tooltip", "fmt_comment", "fmt_summary"):
        sv[k].trace_add("write", settings_changed)

    # ---- start ----
    show_review()
    refresh_preview()
    if paths:
        add_paths(list(paths))
    root.mainloop()
    return 0


# ---------------------------------------------------------------------------------------------------------------------
# Command line
# ---------------------------------------------------------------------------------------------------------------------

def collect_files(paths):
    files = []
    for p in paths:
        if os.path.isdir(p):
            for d, dn, fn in os.walk(p):
                dn[:] = [x for x in dn if not x.startswith(".") and not x.endswith("~")]
                for f in sorted(fn):
                    if f.endswith(".cs"):
                        files.append(os.path.join(d, f))
        elif p.endswith(".cs"):
            files.append(p)
    return files


def run_cli(args):
    settings = load_settings(args.config)
    if args.namespace is not None:
        settings["namespace"] = args.namespace
        settings["namespace_mode"] = "use"
    if args.keep_namespace:
        settings["namespace_mode"] = "keep"
    if args.no_namespace:
        settings["namespace_mode"] = "none"
    if args.template:
        settings["template_path"] = args.template
    template = parse_template(load_template_text(settings), settings)
    if args.new:
        sys.stdout.write(new_script(args.new, settings, template))
        return 0
    files = collect_files(args.paths)
    if not files:
        print("No .cs files found.")
        return 1
    ok = fail = same = 0
    for path in files:
        try:
            text, bom, crlf = read_source(path)
            plan, result = convert_text(text, path, settings, template)
        except Unsupported as e:
            fail += 1
            print("MANUAL  %s  (%s)" % (path, e))
            continue
        except Exception as e:
            fail += 1
            print("ERROR   %s  (%s: %s)" % (path, type(e).__name__, e))
            continue
        if result == text:
            same += 1
            print("same    " + path)
            continue
        ok += 1
        print("CHANGE  " + path)
        if args.apply:
            write_result(path, result, bom, crlf, settings.get("backup", False))
    verb = "Changed" if args.apply else "Would change"
    print("\n%s %d file(s). %d already match. %d need a manual look." % (verb, ok, same, fail))
    if not args.apply and ok:
        print("Nothing was written. Add --apply to write the changes.")
    return 0


def main():
    ap = argparse.ArgumentParser(description="Rebuild C# scripts to follow a template.")
    ap.add_argument("paths", nargs="*", help="files or folders")
    ap.add_argument("--auto", action="store_true", help="no window: use the default choices")
    ap.add_argument("--apply", action="store_true", help="write the changes (with --auto)")
    ap.add_argument("--template", help="template file to use")
    ap.add_argument("--config", help="settings file to use")
    ap.add_argument("--namespace", help="namespace to wrap the code in")
    ap.add_argument("--keep-namespace", action="store_true")
    ap.add_argument("--no-namespace", action="store_true")
    ap.add_argument("--new", metavar="NAME", help="print a new script made from the template")
    args = ap.parse_args()
    if args.auto or args.new:
        return run_cli(args)
    return run_gui(args.paths)


if __name__ == "__main__":
    sys.exit(main())
