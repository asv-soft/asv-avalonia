# 🖥 For Developers

## Work with this checkout

Install a .NET SDK capable of building `net10.0` and a desktop development environment for the example application. `src/Directory.Build.props` defines the target framework and package versions. `src/global.json` permits SDK roll-forward to the latest major version; its minimum version is not the application target framework.

From the repository root:

```bash
dotnet --info
dotnet restore src/Asv.Avalonia.slnx
dotnet build src/Asv.Avalonia.slnx
dotnet test src/Asv.Avalonia.Test/Asv.Avalonia.Test.csproj
```

Run the desktop example to inspect the controls and modules:

```bash
dotnet run --project src/Asv.Avalonia.Example.Desktop/Asv.Avalonia.Example.Desktop.csproj
```

The example includes services, plugin configuration, IO, and other modules. Review its `Program.cs` when reproducing application setup. An application launch requires a graphical desktop; a successful build or unit-test run alone does not verify interactive behavior. Preserve local settings and unrelated changes when preparing a patch.

### Documentation workflow

Open `src/Writerside` in Writerside or an IDE with Writerside support and select the `am` instance. Topics belong in `topics`, images in `images`, and navigation entries in `am.tree`. Use English documentation and preserve canonical API names. Update the topic and navigation entry together when adding a guide, then check links, code examples, images, and the rendered page in preview.

The release workflow in `.github/workflows/docs.yml` builds web and PDF documentation for release tags. `.github/workflows/docs-pages.yml` builds and checks documentation for pull requests and publishes from `main` on matching pushes or an eligible manual run. Inspect the build report as well as preview output. Editing or previewing documentation locally does not publish it.

### Upgrade an application

Start with the versions in the application checkout and the target version's release notes. Align related module references with the main framework version where required, then restore and rebuild before interpreting compiler errors. Launcher packages use their own `LauncherVersion`; do not apply the main framework version to them automatically.

Compare the target checkout's example host and module registration hierarchy with your application. Verify page IDs, navigation arguments, service registrations, view mappings, resources, and extension ownership when changing these areas. Rebuild plugins against the intended host contracts and test loading them in that host. This guide does not promise binary compatibility across framework or Avalonia upgrades.

Run focused tests for changed behavior, then the relevant solution checks. Launch the application and exercise navigation, close approval, persisted layout, and the modules used by your product. For applications using a launcher, also verify direct startup policy and startup through the launcher. Keep migration instructions for any application-specific configuration or stored-data changes with the release.

## Contributing

We would love for you to contribute to our projects and help make it even better than it is today! As a contributor, here are the guidelines we would like you to follow:

* [Submission Guidelines](for-developers.md#submit)
* [Missing a feature?](for-developers.md#feature)
* [Coding rules](for-developers.md#rules)
* [Commit Message Guidelines](for-developers.md#commit)
* [Branches naming](for-developers.md#branches)
* [Semantic versioning](for-developers.md#semver)

### Found a Bug? { #issue }

If you find a bug in the source code, you can help us by [submitting an issue](for-developers.md#submit-issue) to our [GitHub Repository](https://github.com/asv-soft).
It is even better if you can submit a Pull Request with a fix.

### Missing a Feature? { #feature }

You can _request_ a new feature by [submitting an issue](for-developers.md#submit-issue) to our GitHub Repository. If you would like to _implement_ a new feature, please consider the size of the change in order to determine the right steps to proceed:

*   For a **Major Feature**, first open an issue and outline your proposal so that it can be discussed. This process allows us to better coordinate our efforts, prevent duplication of work, and help you to craft the change so that it is successfully accepted into the project.

    **Note**: Adding a new topic to the documentation, or significantly re-writing a topic, counts as a major feature.
* **Small Features** can be crafted and directly submitted as a Pull Request.

### Submission Guidelines { #submit }

#### Submitting an Issue { #submit-issue }

Before you submit an issue, please search the issue tracker. An issue for your problem might already exist and the discussion might inform you of workarounds readily available.

We want to fix all the issues as soon as possible, but before fixing a bug, we need to reproduce and confirm it. In order to reproduce bugs, we require that you provide a minimal reproduction. Having a minimal reproducible scenario gives us a wealth of important information without going back and forth to you with additional questions.

A minimal reproduction allows us to quickly confirm a bug (or point out a coding problem) as well as confirm that we are fixing the right problem.

We require a minimal reproduction to save maintainers' time and ultimately be able to fix more bugs. Often, developers find coding problems themselves while preparing a minimal reproduction. We understand that sometimes it might be hard to extract essential bits of code from a larger codebase, but we really need to isolate the problem before we can fix it.

Unfortunately, we are not able to investigate / fix bugs without a minimal reproduction, so if we don't hear back from you, we are going to close an issue that doesn't have enough info to be reproduced.

### Coding Rules { #rules }

To ensure consistency throughout the source code, keep these rules in mind as you are working:

* All features or bug fixes **must be tested** by one or more specs (unit-tests).
* All public API methods **must be documented**.

#### Commit Message Format { #commit }

_This specification is inspired by and supersedes the_ [_AngularJS commit message format_](https://docs.google.com/document/d/1QrDFcIiPjSLDn3EL15IJygNPiHORgU1\_OOAqWjiDU5Y/edit)_._

We have very precise rules over how our Git commit messages must be formatted. This format leads to **easier to read commit history**.

Each commit message consists of a **header**, a **body**, and a **footer**.

```
<header>
<BLANK LINE>
<body>
<BLANK LINE>
<footer>
```

The `header` is mandatory and must conform to the Commit Message Header format.

The `body` is mandatory for all commits except for those of type "docs". When the body is present it must be at least 20 characters long and must conform to the Commit Message Body format.

The `footer` is mandatory for all commits. The Commit Message Footer format describes what the footer is used for and the structure it must have.

**Commit Message Header**

```
<type>(<scope>): <short summary>
  │       │             │
  │       │             └─⫸ Summary in present tense. Not capitalized. No period at the end.
  │       │
  │       └─⫸ Commit Scope: core|io|geomap|plugins|launcher|example|docs|tests
  │
  └─⫸ Commit Type: build|ci|docs|feat|fix|perf|refactor|test
```

The `<type>` and `<summary>` fields are mandatory, the `(<scope>)` field is optional.

**Type**

Must be one of the following:

* **build**: Changes that affect the build system or external dependencies
* **ci**: Changes to our CI configuration files and scripts
* **docs**: Documentation only changes
* **feat**: A new feature
* **fix**: A bug fix
* **perf**: A code change that improves performance
* **refactor**: A code change that neither fixes a bug nor adds a feature
* **test**: Adding missing tests or correcting existing tests

**Scope**

The scope should be the name of the project affected (as perceived by the person reading the changelog generated from commit messages). If the scope of changes is project scope you can skip writing this. For all other scopes - you can skip main project scope e.g: asv-drones-gui-core -> core.

Use a scope that identifies the affected area of this repository, for example:

* `core` for `Asv.Avalonia`
* `io` for `Asv.Avalonia.IO`
* `geomap` for `Asv.Avalonia.GeoMap`
* `plugins` for `Asv.Avalonia.Plugins`
* `launcher` for the launcher and readiness projects
* `example` for example applications
* `docs` for Writerside topics
* `tests` for shared test infrastructure

**Summary**

Use the summary field to provide a succinct description of the change:

* use the imperative, present tense: "change" not "changed" nor "changes"
* don't capitalize the first letter
* no dot (.) at the end

**Commit Message Body**

Just as in the summary, use the imperative, present tense: "fix" not "fixed" nor "fixes".

Explain the motivation for the change in the commit message body. This commit message should explain _why_ you are making the change. You can include a comparison of the previous behavior with the new behavior in order to illustrate the impact of the change.

**Commit Message Footer**

The footer can contain information about breaking changes and deprecations and is also the place to reference GitHub issues, Asana tickets, and other PRs that this commit closes or is related to. For example:

```
BREAKING CHANGE: <breaking change summary>
<BLANK LINE>
<breaking change description + migration instructions>
<BLANK LINE>
<BLANK LINE>
Fixes #<issue number>
```

or

```
DEPRECATED: <what is deprecated>
<BLANK LINE>
<deprecation description + recommended update path>
<BLANK LINE>
<BLANK LINE>
Closes #<pr number>
```

Breaking Change section should start with the phrase "BREAKING CHANGE: " followed by a summary of the breaking change, a blank line, and a detailed description of the breaking change that also includes migration instructions.

Similarly, a Deprecation section should start with "DEPRECATED: " followed by a short description of what is deprecated, a blank line, and a detailed description of the deprecation that also mentions the recommended update path.

Use the footer to reference the relevant issue or task in the project's issue tracker. A GitHub issue URL, an internal tracker reference, or another agreed task link can provide that context; Asana is not a required tracker.

For example:

```
fix(io): preserve packet recording state

Keep the recording state when refreshing the inspector page

Issue: https://github.com/asv-soft/asv-avalonia/issues/<issue-number>
```

### Revert commits

If the commit reverts a previous commit, it should begin with `revert:` , followed by the header of the reverted commit.

The content of the commit message body should contain:

* information about the SHA of the commit being reverted in the following format: `This reverts commit <SHA>`,
* a clear description of the reason for reverting the commit message.

### Branches naming { #branches }

There is a short list of branch names to create:

* (Feature) - used when adding new functionality on branch
* (Hotfix) - used when fixing existed functionality on branch

### Semantic versioning { #semver }

We use semantic versioning in our projects. If you want to read more about it - try visit [this site](https://semver.org/).