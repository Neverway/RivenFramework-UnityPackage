#!/usr/bin/env python3


import argparse
import os
import re
import sys

NAMESPACE = "RivenFramework"

EXCLUDED_FOLDERS = [
    "NEW",
    "Tools~",
    "Templates~",
    "EasyEditorGUI",
    "SimpleFields",
    "Core/Utils",
    "LogicSystem/LinkSystem",
    "Library",
    "Temp",
    ".git",
    ".idea",
]

EXCLUDED_FILES = [
    "Core/TEMP___.cs",
]


def find_package_root(start):
    path = os.path.abspath(start)
    while True:
        if os.path.isfile(os.path.join(path, "package.json")):
            return path
        parent = os.path.dirname(path)
        if parent == path:
            return None
        path = parent


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


def count_braces(masked):
    return masked.count("{"), masked.count("}")


def process(path, namespace):
    raw = open(path, "rb").read()
    bom = raw.startswith(b"\xef\xbb\xbf")
    try:
        text = raw.decode("utf-8-sig")
    except UnicodeDecodeError:
        return "MANUAL", "not utf-8"

    crlf = "\r\n" in text
    if crlf and re.search(r"(?<!\r)\n", text):
        return "MANUAL", "mixed line endings"
    text = text.replace("\r\n", "\n")

    masked, verbatim_lines, raw_found = mask_source(text)

    if re.search(r"^\s*namespace\s", masked, re.M):
        return "SKIP", "already has a namespace"
    if re.search(r"^\s*//\s*namespace\s", text, re.M):
        return "MANUAL", "has a commented-out namespace"
    if raw_found:
        return "MANUAL", "contains a raw string literal"
    if not re.search(r"\b(class|struct|enum|interface|delegate)\b", masked):
        return "SKIP", "no types in file"

    lines = text.split("\n")
    mlines = masked.split("\n")
    ended_with_newline = text.endswith("\n")
    if ended_with_newline:
        lines.pop()
        mlines.pop()

    split = None
    open_stack = []
    for idx, ml in enumerate(mlines):
        s = ml.strip()
        if s == "":
            continue
        if s.startswith("#"):
            d = s[1:].strip().split()[0] if s[1:].strip() else ""
            if d == "if":
                open_stack.append("if")
            elif d == "region":
                open_stack.append("region")
            elif d in ("endif", "endregion"):
                if not open_stack:
                    return "MANUAL", "unbalanced directive in the usings"
                open_stack.pop()
            continue
        if re.match(r"using\s+[^(]*;$", s) and not s.startswith("using var"):
            continue
        split = idx
        break
    if split is None:
        return "SKIP", "no code found"
    if mlines[split].strip().startswith("[assembly"):
        return "MANUAL", "assembly attribute"

    insert_at = len(lines)
    if open_stack:
        need = len(open_stack)
        k = len(lines) - 1
        found = []
        while k >= split and len(found) < need:
            s = mlines[k].strip()
            if s == "":
                k -= 1
                continue
            if s.startswith("#") and s[1:].strip().split()[0:1] in (["endif"], ["endregion"]):
                found.append(k)
                k -= 1
                continue
            break
        if len(found) != need:
            return "MANUAL", "the file is wrapped in #if/#region that doesn't end the way I expect"
        insert_at = min(found)

    pre_end = split
    while pre_end > 0 and lines[pre_end - 1].strip() == "":
        pre_end -= 1
    pre = lines[:pre_end]

    body = list(range(split, insert_at))
    while body and lines[body[-1]].strip() == "" and mlines[body[-1]].strip() == "":
        body.pop()

    tabs = sum(1 for i in body if lines[i].startswith("\t"))
    spaces = sum(1 for i in body if lines[i].startswith("  "))
    unit = "\t" if tabs > spaces else "    "

    new_body = []
    for i in body:
        ln = lines[i]
        if ln.strip() == "" or i in verbatim_lines:
            new_body.append(ln)
        else:
            new_body.append(unit + ln)

    trailer = lines[insert_at:] if insert_at < len(lines) else []
    new_lines = pre + ([""] if pre else []) + ["namespace " + namespace, "{"] + new_body + ["}"] + trailer

    result = "\n".join(new_lines)
    if ended_with_newline:
        result += "\n"

    new_masked, _, _ = mask_source(result)
    ob, cb = count_braces(masked)
    nb, nc = count_braces(new_masked)
    if nb != ob + 1 or nc != cb + 1:
        return "MANUAL", "brace check failed"
    def flat(m):
        return re.sub(r"[\s{}]", "", m)

    if flat(masked) != flat(new_masked).replace("namespace" + namespace, "", 1):
        return "MANUAL", "content check failed"

    if crlf:
        result = result.replace("\n", "\r\n")
    data = result.encode("utf-8")
    if bom:
        data = b"\xef\xbb\xbf" + data
    return "WRAP", data


def main():
    ap = argparse.ArgumentParser(description="Wrap scripts that have no namespace in namespace %s." % NAMESPACE)
    ap.add_argument("folder", help="folder to process (searched recursively)")
    ap.add_argument("--apply", action="store_true", help="write the changes (default is a preview)")
    ap.add_argument("--namespace", default=NAMESPACE)
    args = ap.parse_args()

    root = find_package_root(args.folder)
    if root is None:
        print("Could not find package.json above that folder. Run this on a folder inside the package.")
        return 1

    excluded_dirs = [os.path.normpath(os.path.join(root, p)) for p in EXCLUDED_FOLDERS]
    excluded_files = [os.path.normpath(os.path.join(root, p)) for p in EXCLUDED_FILES]

    wrapped = skipped = manual = 0
    for dirpath, dirnames, filenames in os.walk(os.path.abspath(args.folder)):
        dirnames[:] = [d for d in dirnames if os.path.normpath(os.path.join(dirpath, d)) not in excluded_dirs]
        dirnames.sort()
        for fn in sorted(filenames):
            if not fn.endswith(".cs"):
                continue
            full = os.path.normpath(os.path.join(dirpath, fn))
            rel = os.path.relpath(full, root).replace(os.sep, "/")
            if full in excluded_files:
                continue
            status, info = process(full, args.namespace)
            if status == "WRAP":
                wrapped += 1
                print("WRAP    " + rel)
                if args.apply:
                    with open(full, "wb") as f:
                        f.write(info)
            elif status == "SKIP":
                skipped += 1
                print("skip    %s  (%s)" % (rel, info))
            else:
                manual += 1
                print("MANUAL  %s  (%s)" % (rel, info))

    print()
    verb = "Wrapped" if args.apply else "Would wrap"
    print("%s %d file(s). Skipped %d. %d need to be done by hand." % (verb, wrapped, skipped, manual))
    if not args.apply and wrapped:
        print("Nothing was changed. Run again with --apply to write the changes.")
    return 0


if __name__ == "__main__":
    sys.exit(main())