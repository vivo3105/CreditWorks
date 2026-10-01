@echo off
setlocal EnableDelayedExpansion

rem Checks everything needed to build and run the solution.
rem Usage: CheckPrerequisites.bat [nopause]
rem   nopause  don't wait for a key at the end (for automation/CI).
rem Exit code: 0 = all required checks passed, 1 = at least one failed.

cd /d "%~dp0"

set "PAUSE_AT_END=1"
if /i "%~1"=="nopause" set "PAUSE_AT_END="

set "ROOT=%~dp0.."
set "WEB_DIR=%ROOT%\VehicleManagement\VehicleManagement.Web"
set "DB_FILE=%ROOT%\Database\CreditWorksDb.mdf"
set /a FAILED=0
set /a WARNED=0

echo.
echo ============================================================
echo  Vehicle Management - prerequisite check
echo ============================================================
echo.

rem ---- .NET 8 SDK --------------------------------------------------------
where dotnet >nul 2>&1
if errorlevel 1 (
    call :fail ".NET SDK" "dotnet not found. Install the .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0"
) else (
    set "SDK8="
    for /f "tokens=1" %%v in ('dotnet --list-sdks ^| findstr /b "8."') do set "SDK8=%%v"
    if defined SDK8 (
        call :pass ".NET 8 SDK" "!SDK8!"
    ) else (
        call :fail ".NET 8 SDK" "dotnet is installed but no 8.x SDK was found. Install the .NET 8 SDK."
    )
)

rem ---- EF Core CLI -------------------------------------------------------
set "EFVER="
for /f "delims=" %%v in ('dotnet ef --version 2^>nul ^| findstr /r "^[0-9]"') do set "EFVER=%%v"
if defined EFVER (
    call :pass "EF Core CLI (dotnet-ef)" "!EFVER!"
) else (
    call :fail "EF Core CLI (dotnet-ef)" "Not found. Install: dotnet tool install --global dotnet-ef --version 8.*"
)

rem ---- Node.js (Angular 22 needs 20.19+ / 22.12+ / 24+) -------------------
where node >nul 2>&1
if errorlevel 1 (
    call :fail "Node.js" "node not found. Install Node.js 22 LTS or 24: https://nodejs.org"
) else (
    for /f "delims=" %%v in ('node --version') do set "NODEVER=%%v"
    for /f "tokens=1 delims=v." %%m in ("!NODEVER!") do set /a NODEMAJOR=%%m
    if !NODEMAJOR! GEQ 20 (
        call :pass "Node.js" "!NODEVER!"
    ) else (
        call :fail "Node.js" "!NODEVER! is too old. Install Node.js 22 LTS or 24."
    )
)

rem ---- npm ---------------------------------------------------------------
where npm >nul 2>&1
if errorlevel 1 (
    call :fail "npm" "npm not found. It is installed with Node.js."
) else (
    for /f "delims=" %%v in ('npm --version 2^>nul') do set "NPMVER=%%v"
    call :pass "npm" "!NPMVER!"
)

rem ---- SQL Server Express instance "SQLEXPRESS" ---------------------------
sc query "MSSQL$SQLEXPRESS" >nul 2>&1
if errorlevel 1 (
    call :fail "SQL Server Express" "Service MSSQL$SQLEXPRESS not found. Install SQL Server Express with instance name SQLEXPRESS."
) else (
    sc query "MSSQL$SQLEXPRESS" | findstr /c:"RUNNING" >nul
    if errorlevel 1 (
        call :fail "SQL Server Express" "Installed but not running. Start it: net start MSSQL$SQLEXPRESS (as administrator)"
    ) else (
        call :pass "SQL Server Express" "MSSQL$SQLEXPRESS is running"
    )
)

rem ---- Database file (warning only: migrations can create it) -------------
if exist "%DB_FILE%" (
    call :pass "Database file" "Database\CreditWorksDb.mdf"
) else (
    call :warn "Database file" "Database\CreditWorksDb.mdf not found. Create it with CreateDatabase.bat."
)

rem ---- Angular packages (warning only: Start_Angular_Webapp.bat installs) -
if exist "%WEB_DIR%\node_modules\@angular\core" (
    call :pass "Web packages" "node_modules installed"
) else (
    call :warn "Web packages" "Not installed yet. Run 'npm install' in VehicleManagement.Web (Start_Angular_Webapp.bat does this)."
)

rem ---- Ports (information only) ------------------------------------------
call :port 5255 "API port"
call :port 4200 "Web app port"

echo.
echo ------------------------------------------------------------
set "EXIT_CODE=0"
if %FAILED% GTR 0 (
    echo  RESULT: %FAILED% required check^(s^) failed, %WARNED% warning^(s^).
    set "EXIT_CODE=1"
) else (
    echo  RESULT: all required checks passed, %WARNED% warning^(s^).
)
echo ------------------------------------------------------------
echo.

if defined PAUSE_AT_END pause
endlocal & exit /b %EXIT_CODE%

rem ---- helpers ------------------------------------------------------------
:pass
echo  [ OK ]  %~1: %~2
exit /b 0

:warn
echo  [WARN]  %~1: %~2
set /a WARNED+=1
exit /b 0

:fail
echo  [FAIL]  %~1: %~2
set /a FAILED+=1
exit /b 0

:port
netstat -ano | findstr /r /c:":%~1 .*LISTENING" >nul
if errorlevel 1 (
    echo  [INFO]  %~2 %~1 is free.
) else (
    echo  [INFO]  %~2 %~1 is already in use ^(the app may already be running^).
)
exit /b 0
pause
