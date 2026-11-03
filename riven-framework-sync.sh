#!/usr/bin/env bash
# Riven Framework 2027

set -u

REPO="$(git rev-parse --show-toplevel 2>/dev/null)" || { echo "Run this from inside a git repo!"; exit 1; }
cd "$REPO" || exit 1

GITDIR="$(git rev-parse --absolute-git-dir)"
CONFIG="$GITDIR/riven-sync-target"

push_changes() {
    local branch
    branch="$(git rev-parse --abbrev-ref HEAD)"
    echo
    echo "Repo:   $REPO"
    echo "Branch: $branch"
    echo
    git status --short
    echo

    if [ -n "$(git status --porcelain)" ]; then
        read -r -p "Commit message (leave empty to cancel): " msg
        [ -z "$msg" ] && { echo "Cancelled"; return; }
        git add -A || return
        git commit -m "$msg" || return
    else
        echo "Nothing new to commit"
    fi

    echo
    echo "Pulling remote changes..."
    if git ls-remote --exit-code --heads origin "$branch" >/dev/null 2>&1; then
        git pull --rebase origin "$branch" || {
            echo
            echo "The pull conflict detected! Run 'git rebase --continue', then run this again to fix it"
            return
        }
    fi

    git push -u origin "$branch" && echo && echo "Pushed '$branch'."
}

update_package() {
    local branch target
    branch="$(git rev-parse --abbrev-ref HEAD)"

    # If this IS the package folder inside the game project, a plain pull is all that is needed.
    if [[ "$REPO" == */Packages/com.neverway.riven ]]; then
        if [ -n "$(git status --porcelain)" ]; then
            echo "This folder has uncommitted changes! Push them with option 1 before continuing"
            return
        fi
        git fetch origin && git checkout "$branch" && git pull --rebase origin "$branch"
        return
    fi

    target=""
    [ -f "$CONFIG" ] && target="$(cat "$CONFIG")"

    if [ -z "$target" ] || [ ! -d "$target/.git" ] && [ ! -f "$target/.git" ]; then
        echo
        echo "Where is the package folder inside your game project?"
        echo "Example: /c/Projects/CorridorGeodesic/RivenFramework-Unity/Packages/com.neverway.riven"
        read -r -p "Path: " target
        target="${target%/}"
        if [ ! -d "$target/.git" ] && [ ! -f "$target/.git" ]; then
            echo "That folder is not a git repo! Nothing was changed"
            return
        fi
        echo "$target" > "$CONFIG"
    fi

    if [ -n "$(git -C "$target" status --porcelain)" ]; then
        echo
        echo "The package in the game project has uncommitted changes:"
        git -C "$target" status --short
        echo "Commit or push those first (run this script from that folder, option 1)"
        return
    fi

    echo
    echo "Updating $target to '$branch'..."
    git -C "$target" fetch origin || return
    git -C "$target" checkout "$branch" || return
    git -C "$target" pull --rebase origin "$branch" || return
    echo
    echo "Done! Switch back to Unity to let it reimport"
    echo "The game project will show the package as modified! Don't forget to commit the package update!"
}

while true; do
    echo
    echo "=============================="
    echo " Riven Framework Sync"
    echo "=============================="
    echo " 1) Push changes to Riven Framework Package Repository"
    echo " 2) Update Riven Framework Package in current project"
    echo " q) Quit"
    echo
    read -r -p "Choose: " choice
    case "$choice" in
        1) push_changes ;;
        2) update_package ;;
        q|Q) exit 0 ;;
        *) echo "Pick 1, 2 or q" ;;
    esac
done
