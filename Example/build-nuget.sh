# This script uses the Hyper API package from nuget to build the example project.
set -e
dotnet add package Tableau.HyperAPI.NET
dotnet build Example.csproj
