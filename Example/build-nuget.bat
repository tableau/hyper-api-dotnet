REM This script uses the Hyper API package from nuget to build the example project.
SETLOCAL EnableDelayedExpansion
dotnet add package Tableau.HyperAPI.NET || exit /b !ERRORLEVEL!
dotnet build Example.csproj || exit /b !ERRORLEVEL!
