@echo off
REM Build CitySO as self-contained executable
REM This script creates a standalone .exe file that includes .NET runtime

setlocal enabledelayedexpansion

set Configuration=Release
set Runtime=win-x64
set OutputPath=publish

echo Building CitySO as self-contained executable...
echo Configuration: %Configuration%
echo Runtime: %Runtime%
echo Output: %OutputPath%
echo.

REM Get the project directory
set ProjectDir=%~dp0

REM Set the project file path
set ProjectFile=CitySO\CitySO.csproj

REM Check if project file exists
if not exist "%ProjectDir%%ProjectFile%" (
    echo Error: %ProjectFile% not found in %ProjectDir%
    exit /b 1
)

echo Project: %ProjectFile%

REM Clean previous build
if exist "%OutputPath%" (
    echo Cleaning previous build...
    rmdir /s /q "%OutputPath%"
)

REM Build and publish as self-contained executable
echo Publishing application...
dotnet publish "%ProjectDir%%ProjectFile%" ^
    -c %Configuration% ^
    --self-contained ^
    -r %Runtime% ^
    -o %OutputPath% ^
    --p:PublishSingleFile=true ^
    --p:IncludeNativeLibrariesForSelfExtract=true ^
    --p:PublishTrimmed=false

if %ERRORLEVEL% neq 0 (
    echo Build failed!
    exit /b 1
)

REM Find the executable
for /f "delims=" %%F in ('dir /b "%OutputPath%\*.exe" 2^>nul') do (
    set ExePath=%%F
    goto :found_exe
)

echo Error: Executable not found in output directory
exit /b 1

:found_exe
echo.
echo Build completed successfully!
echo Executable: %OutputPath%\%ExePath%
echo.
echo The .exe file is ready to use on any Windows system with no dependencies.
echo File includes .NET runtime and all required libraries.
echo.
pause
