REM This script uses a local copy of the Hyper API package to build the example project. Use "build-nuget.bat" to download the package from nuget instead.
SETLOCAL EnableDelayedExpansion
dotnet build Example.csproj || exit /b !ERRORLEVEL!
xcopy /Y ..\lib\tableauhyperapi.dll bin\Debug\netcoreapp6.0\ || exit /b !ERRORLEVEL!
xcopy /E /Y ..\lib\hyper bin\Debug\netcoreapp6.0\hyper\ || exit /b !ERRORLEVEL!
