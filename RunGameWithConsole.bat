@echo off
setlocal enabledelayedexpansion

if defined GODOT_CONSOLE (
    if exist "%GODOT_CONSOLE%" (
        set "GODOT_EXE=%GODOT_CONSOLE%"
        goto :launch
    )
)

where godot_console >nul 2>nul
if %ERRORLEVEL% equ 0 (
    set "GODOT_EXE=godot_console"
    goto :launch
)

set "LOCAL_DEV=%USERPROFILE%\.gemini\antigravity\tools\godot_mono\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe"
if exist "%LOCAL_DEV%" (
    set "GODOT_EXE=%LOCAL_DEV%"
    goto :launch
)

echo [ERROR] Godot 4 .NET console executable not found.
echo Set GODOT_CONSOLE="C:\path\to\Godot_v4.x_mono_win64_console.exe"
pause
exit /b 1

:launch
echo Running Hanoi Towers with console logs (%GODOT_EXE%)...
"%GODOT_EXE%" --path "%~dp0."
pause
exit /b 0
