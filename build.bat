@echo off
setlocal enabledelayedexpansion

rem Build all AutoCAD-version targets of Acad-TotalLength and collect the
rem produced DLLs into .\dist\AutoCAD <year>\.
rem
rem Usage:
rem   build.bat              -> Release (default)
rem   build.bat Debug        -> Debug
rem   build.bat Release      -> Release

set "CONFIG=%~1"
if "%CONFIG%"=="" set "CONFIG=Release"

set "REPO=%~dp0"
set "DIST=%REPO%dist"

echo ==^> Configuration: %CONFIG%
echo ==^> Repo:          %REPO%
echo ==^> Dist:          %DIST%
echo.

if not exist "%DIST%" mkdir "%DIST%"

call :build 2024 "src\TotalLength 2024\TotalLength 2024.csproj" "src\TotalLength 2024\bin\%CONFIG%\codehaks.TotalLength.dll" || goto :fail
call :build 2025 "src\TotalLength 2025\TotalLength 2025.csproj" "src\TotalLength 2025\bin\%CONFIG%\net8.0-windows\codehaks.TotalLength.dll" || goto :fail
call :build 2026 "src\TotalLength 2026\TotalLength 2026.csproj" "src\TotalLength 2026\bin\%CONFIG%\net8.0-windows\codehaks.TotalLength.dll" || goto :fail

echo.
echo ==^> All builds succeeded. Output: %DIST%
endlocal
exit /b 0

:build
set "YEAR=%~1"
set "PROJ=%~2"
set "OUTDLL=%~3"
set "DESTDIR=%DIST%\AutoCAD %YEAR%"

echo ----------------------------------------------------------------------
echo Building TotalLength %YEAR% (%CONFIG%)
echo ----------------------------------------------------------------------
dotnet msbuild "%REPO%%PROJ%" -t:Restore;Rebuild -p:Configuration=%CONFIG% -p:Platform="Any CPU" -nologo
if errorlevel 1 exit /b 1

if not exist "%REPO%%OUTDLL%" (
    echo [FAIL] Expected build output not found: %REPO%%OUTDLL%
    exit /b 1
)

if not exist "%DESTDIR%" mkdir "%DESTDIR%"
copy /Y "%REPO%%OUTDLL%" "%DESTDIR%\" >nul
rem Copy pdb if present.
set "OUTPDB=%REPO%%OUTDLL:.dll=.pdb%"
if exist "%OUTPDB%" copy /Y "%OUTPDB%" "%DESTDIR%\" >nul

echo [OK] %YEAR% -^> %DESTDIR%\codehaks.TotalLength.dll
exit /b 0

:fail
echo.
echo ==^> Build FAILED.
endlocal
exit /b 1
