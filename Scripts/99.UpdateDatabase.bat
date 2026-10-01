@echo off
setlocal

rem Generates an EF Core migration for any model changes and applies it
rem to the database.
rem Usage: UpdateDatabase.bat [MigrationName]

rem Run from this script's folder so the relative project paths resolve
rem regardless of where the script is launched from.
cd /d "%~dp0"

set "API_DIR=..\VehicleManagement\VehicleManagement.Api"
set "INFRA_DIR=..\VehicleManagement\VehicleManagement.Infrastructure"
set "MIGRATIONS_DIR=%INFRA_DIR%\Persistence\Migrations"
set "EF_ARGS=--project %INFRA_DIR% --startup-project %API_DIR%"

rem Migration name: first argument, otherwise prompt (default is timestamped).
set "NAME=%~1"
if not defined NAME (
    for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMddHHmmss"') do set "DEFAULT_NAME=Update%%i"
    call set /p "NAME=Migration name [%%DEFAULT_NAME%%]: "
)
if not defined NAME set "NAME=%DEFAULT_NAME%"

echo.
echo ============================================================
echo  Step 1/2: Generating migration '%NAME%'
echo ============================================================
echo.

dotnet ef migrations add %NAME% %EF_ARGS% --output-dir Persistence/Migrations
if errorlevel 1 goto :failed

rem EF Core 8 generates an empty migration when the model has not changed.
rem Detect that and remove it instead of applying a no-op.
set "MIGRATION_FILE="
for /f "delims=" %%f in ('dir /b /o-d "%MIGRATIONS_DIR%\*_%NAME%.cs" 2^>nul ^| findstr /v /i "Designer"') do (
    if not defined MIGRATION_FILE set "MIGRATION_FILE=%MIGRATIONS_DIR%\%%f"
)

if defined MIGRATION_FILE (
    findstr /c:"migrationBuilder." "%MIGRATION_FILE%" >nul
    if errorlevel 1 (
        echo.
        echo No model changes detected - removing the empty migration.
        dotnet ef migrations remove %EF_ARGS%
        if errorlevel 1 goto :failed
        rem Still apply any migrations that exist but are not in the database yet.
        goto :apply
    )
)

echo.
echo ============================================================
echo  Migration generated. Review it before applying:
echo  %MIGRATION_FILE%
echo.
echo  Press any key to apply it to the database,
echo  or close this window to stop (then remove it with:
echo  dotnet ef migrations remove %EF_ARGS%)
echo ============================================================
pause >nul

:apply
echo.
echo ============================================================
echo  Step 2/2: Updating database
echo ============================================================
echo.

dotnet ef database update %EF_ARGS%
if errorlevel 1 goto :failed

echo.
echo ============================================================
echo  SUCCESS - database structure is up to date.
echo ============================================================
goto :done

:failed
echo.
echo ************************************************************
echo  FAILED - review the errors above.
echo ************************************************************

:done
echo.
pause
endlocal
