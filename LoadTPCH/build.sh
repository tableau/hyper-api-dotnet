set -e
dotnet build LoadTPCH.csproj
cp -R ../lib/hyper bin/Debug/netcoreapp6.0/hyper
cp -R ../lib/libtableauhyperapi.* bin/Debug/netcoreapp6.0/
