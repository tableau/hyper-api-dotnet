# Hyper API for .NET

**This repository is unsupported by Tableau!**

Since the initial release of [Hyper API](https://tableau.github.io/hyper-db/) in 2019, Hyper API shipped for Python, Java, C++ and .Net.
Usage of .Net never really picked up and we were not able to identify any users using the .Net version in production, so the Hyper team decided in September 2023 to deprecate the .Net version of Hyper API.
We are releasing the source code to the open-source community, such that any interested .Net enthusiasts have an opportunity to pick up where we left.

In its current state, building the code in this repository is very rough and requires a couple of manual steps as described below.
If you are interested in the .Net Hyper API and want to continue this work, please feel free to open pull requests.
You can reach out to us via [Slack](https://join.slack.com/t/tableau-datadev/shared_invite/zt-1q4rrimsh-lHHKzrhid1MR4aMOkrnAFQ).
While we can't provide support to end users of this library, we are happy to support people who potentially want to take over maintenance of this project.


## Building / Installation

Hyper API library is a .NET Standard library which targets the .NET Standard 2.0, it is built with .NET Core 2.2.
It can be used in applications which target .NET Core 2.2 and newer or .NET Framework 4.6.1 and newer.
(Older .NET Core might work, but it has not been tested.)

The Hyper API supports only 64-bit host processes. If you use Visual Studio, set the platform to "x64" in the configuration manager. If you create a project with "Any CPU" architecture in Visual Studio, it will likely run a 32-bit process and Hyper API initialization will fail with a `bad image` exception.

Hyper API consists of three parts:

* HyperAPI.NET.dll assembly, which is the managed assembly to be linked in your application. Reference it in your project, and deploy it to the application binary directory. It is located in the `lib` folder of the binary package.
* Native `hyperapi` library: tableauhyperapi.dll on Windows, libtableauhyperapi.so on Linux, and libtableauhyperapi.dylib on OSX. It is required at runtime, and should be deployed next to the managed HyperAPI.NET.dll assembly. It is located in the `lib` folder of the binary package.
* Hyper server binaries. They are in the lib/hyper folder, and they can be deployed in two ways:
  * It can be put into an arbitrary location, and the application should specify the path to it as an argument to the `HyperProcess` constructor.
  * Alternatively, `hyper` folder can be deployed next to the HyperAPI.NET.dll assembly, where it will be found at runtime automatically.

The binaries can be taken, e.g. from the C++ `.zip` file of Hyper API which can be found on the [Hyper API releases page](https://tableau.github.io/hyper-db/docs/releases).
The latest version of Hyper API against the .Net library is known to work is v0.0.17782.

## Development in Visual Studio

HyperAPI.sln is the solution which includes Hyper API and test projects.

Hyper API was mostly developed using Visual Studio 2017. Later versions should work too, but they haven't been tested.
Some folks successfully use Visual Studio code.

.NET Core 2.2 needs to be installed if it wasn't installed as part of Visual Studio. Get it
[here](https://dotnet.microsoft.com/download/visual-studio-sdks).
Install it, restart Visual Studio, open the solution, you should see .Net Core 2.2 under Target Framework
in the properties of HyperAPITest project.

This will let you build the solution, but the `tableauhyperapi.dll` and the `hyperd` binary will still be missing.
You can get those binaries, e.g. from the C++ `.zip` file of Hyper API which can be found on the
[Hyper API releases page](https://tableau.github.io/hyper-db/docs/releases).
Running the tests will only be possible after copying over `hyperd` and `tableauhyperapi.dll`.
