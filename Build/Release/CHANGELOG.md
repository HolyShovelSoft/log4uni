# 1.0.5

* [Fix] Exception messages now has clickable source code links for Unity 2020+ versions

# 1.0.4

* [Deploy] Now github actions not build release if no changes in source files detected
* [Deploy] Now github actions not try upload new release if version of package not changed
* [Update] Update log4net to 2.0.12
* [Fix] Remove all ASP, System.Web, Mutex and some OS related features from log4net for better compatibility with some Unity platforms
* [Fix] All dll's now has better linker information for code stripping process 

# 1.0.3

* [Feature] log4net.editor and log4net.runtime files now supported as config files in default configurators

# 1.0.2

* [Deploy] Add some lost meta files
* [Info] Add author info 

# 1.0.1

* [Deploy] Add auto deployment

# 1.0.0

* [Refactor] Rename project and all assemblies to match unity convention.
* [WIP] Build folder now can be used as local package for unity. Package branch and publication in public npm repository still in progress.

# 0.3.3

* [Fix] Silent crash of editor when start with non compiled scripts no longer happens.

# 0.3.2

* [Add] Add context object as log event property.

# 0.3.1

* [Fix] Infinity exception loops in editor start or recompile no longer happens.

# 0.2.2

* [Fix] Change editor initialization algorithm.

# 0.2.1

* [Feature] Exception stack trace now clickable in editor console.

# 0.0.0

* Begin of journey =)