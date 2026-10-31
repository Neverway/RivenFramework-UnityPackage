# Riven Framework (Unity)

The Neverway Riven Framework! This package provides a sort of project structure, game architecture stuffs, and templates acting as a perfect starting point for all your side-project needs! (And non-side-projects too I guess >:P)

## Requirements

- Unity 2022.3 (developed on 2022.3.15f1)
- Unity package dependencies: Input System 1.7.0, TextMeshPro 3.0.9, uGUI 1.0.0
- The game project's `Packages/manifest.json` must also contain this line, because the Unity Package Manager cannot install git dependencies from inside a package:
  `"com.gasgiant.markupattributes": "https://github.com/gasgiant/Markup-Attributes.git#upm"`

## Using it in a project

Add it as a git submodule at `Packages/com.neverway.riven`:

    git submodule add https://github.com/Neverway/RivenFramework-UnityPackage.git <UnityProject>/Packages/com.neverway.riven

After cloning a project that already uses the framework, run this to download the framework files:

    git submodule update --init --recursive

## Updating the framework in a project

    cd <UnityProject>/Packages/com.neverway.riven
    git checkout main
    git pull
    cd <back to the game repo root>
    git add <UnityProject>/Packages/com.neverway.riven
    git commit -m "Updated framework"
    git tag v#.#.#
    git push --tags

## Making changes to the framework

The folder `Packages/com.neverway.riven` and the files within it are part of their own git repo.
If you are editing them, commit and push inside the package first, then commit the updated pointer in the game repo!
Please keep game-specific code out of the package!
Do not use `Remove` on the package inside of Unity's Package Manager as it can break references and deletes the folder!
When you update the package repository, don't forget to bump `version` in `package.json` and tag the release on git!

## Example assets

The `Resources` folder should be copied out of the package folder and into the root of your unity assets folder. It contains the structure for the project and some template assets for getting started (things like widgets, sample levels, etc.) This is obviously not required, but heavily encouraged!

## Contributors

Liz M., Errynei, Connorses, Soulex
