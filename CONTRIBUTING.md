# Contributing Guide For Hyper API .Net

This page lists the operational governance model of this project, as well as the recommendations and requirements for how to best contribute to `hyper-api-dotnet`. We strive to obey these as best as possible. As always, thanks for contributing – we hope these guidelines make it easier and shed some light on our approach and processes.

# Governance Model: Published but not supported

The intent and goal of open sourcing this project is that it may contain useful or interesting code/concepts that Salesforce wishes to share with the larger open source community. Although occasional work may be done on it, we will not be actively looking for or soliciting contributions. Forks or pull requests are welcome, though.

# Getting started

Please join the community on [Slack](https://join.slack.com/t/tableau-datadev/shared_invite/zt-1q4rrimsh-lHHKzrhid1MR4aMOkrnAFQ). Also please make sure to take a look at [README](README.md) to see where we are headed.

# Contribution Checklist

- [x] Clean, simple, well styled code
- [x] Commits should be atomic and messages must be descriptive. Related issues should be mentioned by Issue number.
- [x] Comments
  - Module-level & function-level comments.
  - Comments on complex blocks of code or algorithms (include references to sources).
- [x] Tests
  - The test suite, if provided, must be complete and pass
  - Increase code coverage, not versa.
  - Use any of our testkits that contains a bunch of testing facilities you would need and borrow inspiration from existing tests.
- [x] Dependencies
  - Minimize number of dependencies.
  - Prefer Apache 2.0, BSD3, MIT, ISC and MPL licenses.
- [x] Reviews
  - Changes must be approved via peer code review

# Creating a Pull Request

1. **Ensure the bug/feature was not already reported** by searching on GitHub under Issues.  If none exists, create a new issue so that other contributors can keep track of what you are trying to add/fix and offer suggestions (or let you know if there is already an effort in progress).
3. **Clone** the forked repo to your machine.
4. **Create** a new branch to contain your work (e.g. `git br fix-issue-11`)
4. **Commit** changes to your own branch.
5. **Push** your work back up to your fork. (e.g. `git push fix-issue-11`)
6. **Submit** a Pull Request against the `main` branch and refer to the issue(s) you are fixing. Try not to pollute your pull request with unintended changes. Keep it simple and small.
7. **Sign** the Salesforce CLA (you will be prompted to do so when submitting the Pull Request)

> **NOTE**: Be sure to [sync your fork](https://help.github.com/articles/syncing-a-fork/) before making a pull request.


# Code of Conduct
Please follow our [Code of Conduct](CODE_OF_CONDUCT.md).

# License
By contributing your code, you agree to license your contribution under the terms of our project [LICENSE](LICENSE.txt) and to sign the [Salesforce CLA](https://cla.salesforce.com/sign-cla)
