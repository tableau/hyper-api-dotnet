SETLOCAL EnableDelayedExpansion
dotnet build LoadTPCH.csproj || exit /b !ERRORLEVEL!
xcopy /Y ..\lib\tableauhyperapi.dll bin\Debug\netcoreapp6.0\ || exit /b !ERRORLEVEL!
xcopy /E /Y ..\lib\hyper bin\Debug\netcoreapp6.0\hyper\ || exit /b !ERRORLEVEL!
