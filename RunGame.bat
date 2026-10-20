@echo off
setlocal enabledelayedexpansion

REM --- Locate Godot Mono / .NET Executable ---
if defined GODOT_BIN (
    if exist "%GODOT_BIN%" (
        set "GODOT_EXE=%GODOT_BIN%"
        goto :launch
    )
)

where godot >nul 2>nul
if %ERRORLEVEL% equ 0 (
    set "GODOT_EXE=godot"
    goto :launch
)

set "LOCAL_DEV=%USERPROFILE%\.gemini\antigravity\tools\godot_mono\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64.exe"
if exist "%LOCAL_DEV%" (
    set "GODOT_EXE=%LOCAL_DEV%"
    goto :launch
)

echo [ERROR] Godot 4 .NET executable not found in PATH or standard locations.
echo Please run this project using Godot Engine 4 .NET (with C# support).
echo Or specify the path: set GODOT_BIN="C:\path\to\Godot_v4.x_mono_win64.exe"
pause
exit /b 1

:launch
echo Launching Hanoi Towers with %GODOT_EXE%...
start "" "%GODOT_EXE%" --path "%~dp0."
exit /b 0
