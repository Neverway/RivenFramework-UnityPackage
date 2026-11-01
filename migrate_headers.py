#!/usr/bin/env python3
import argparse, os, subprocess, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import riven_lint as rl

SKIP_DIRS = {".git", "Library", "Temp", "obj", "Templates~", "node_modules"}


def main():
    ap = argparse.ArgumentParser(description="Upgrades old 2024 'Written by' headers to the new Neverway header. Run it from the package root with Unity closed. Needs riven_lint.py next to it.")
    ap.add_argument("--apply", action="store_true", help="actually change the files (without this it only shows what would change)")
    ap.add_argument("--year", help="use this year on every migrated header (default: keep each file's original year)")
    ap.add_argument("--default-author", default="Liz M.", help="author to use when the old header has none")
    ap.add_argument("--force", action="store_true", help="skip the branch and uncommitted changes check")
    a = ap.parse_args()

    if not os.path.isfile("package.json"):
        sys.exit("Run this from the package root, the folder that has package.json in it.")
    if a.apply and not a.force:
        branch = subprocess.run(["git", "rev-parse", "--abbrev-ref", "HEAD"], capture_output=True, text=True).stdout.strip()
        dirty = subprocess.run(["git", "status", "--porcelain"], capture_output=True, text=True).stdout.strip()
        if branch in ("main", "master", "HEAD"):
            sys.exit("You're on '%s'. Make a branch first:  git checkout -b header-migration" % branch)
        if dirty:
            sys.exit("You have uncommitted changes. Commit or stash them first.")

    changed = empty_desc = 0
    for dp, dn, fn in os.walk("."):
        dn[:] = sorted(d for d in dn if d not in SKIP_DIRS)
        for f in sorted(fn):
            if not f.endswith(".cs"):
                continue
            p = os.path.join(dp, f)
            text, nl, bom = rl.read_source(p)
            lines = text.split("\n")
            h = rl.find_header(lines)
            if not h or h["style"] != "old":
                continue
            year = a.year or h["year"]
            new = rl.apply_header(text, year, h["author"] or a.default_author, [], h["description"])
            changed += 1
            if not h["description"]:
                empty_desc += 1
            print(("MIGRATE " if a.apply else "would migrate ") + os.path.relpath(p).replace(os.sep, "/")
                  + ("" if h["description"] else "   (no description, fill it in later)"))
            if a.apply:
                rl.write_bytes_atomic(p, rl.encode_source(new, nl, bom))

    print("\n%d file(s) %s. %d of them have no description yet." % (changed, "migrated" if a.apply else "would change", empty_desc))
    if a.apply and changed:
        print("Next: look at git diff --stat, open Unity and make sure it still compiles, then commit.\n"
              "To find the empty descriptions: python3 riven_lint.py --check . --min-severity error")
    elif not a.apply:
        print("That was a dry run. Add --apply to do it for real.")


if __name__ == "__main__":
    main()
