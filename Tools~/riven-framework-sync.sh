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
        read -r -p "Commit message: " msg
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
            echo "The pull conflict detected! Run 'git rebase --continue', then run this again to fix it!"
            return
        }
    fi

    git push -u origin "$branch" && echo && echo "Pushed '$branch'."
}

get_branch() {
    local b
    b="$(git rev-parse --abbrev-ref HEAD)"
    if [ "$b" = "HEAD" ]; then
        read -r -p "No branch checked out!" b
        b="${b:-refactor}"
        git checkout "$b" >/dev/null 2>&1 || git checkout -b "$b" "origin/$b" || return 1
    fi
    echo "$b"
}

update_package() {
    if [ -n "$(git status --porcelain)" ]; then
        echo
        echo "This folder has uncommitted changes! Please commit them first!"
        git status --short
        return
    fi

    local branch
    branch="$(get_branch)" || return
    echo
    echo "Updating '$branch' from origin..."
    git fetch origin || return
    git checkout "$branch" || return
    git pull --rebase origin "$branch" || return
    echo
    echo "Done."
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
    esac
done
