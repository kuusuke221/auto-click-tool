---
applyTo: "**/*.csproj,**/*.xaml,**/AssemblyInfo.cs"
---

# Version and Release Instructions

When changing the application version:

1. Search the repository for the old version and update every user-visible or assembly version reference that represents the application release. Keep the main window title, `AssemblyVersion`, and `FileVersion` in sync. For a `MAJOR.MINOR.PATCH` release, use `MAJOR.MINOR.PATCH.0` for the assembly and file versions. Update other version metadata only when it exists and is intended to track the application release.
2. Before finishing, search again for stale release-version references and run a build for the affected project. Report any validation that could not be completed.
3. If a Git tag is requested, use the `vMAJOR.MINOR.PATCH` format and create an annotated tag only after the release changes have been committed. Ensure the tag points to the commit containing those changes. Never tag an earlier `HEAD` while the release changes are uncommitted, and never replace or move an existing tag; stop and report if the requested tag already exists.
4. Do not include unrelated working-tree changes in a release commit. Push the tag only when the user explicitly requests a push, and verify the tag locally or remotely as applicable.
