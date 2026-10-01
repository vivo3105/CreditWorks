@echo off
setlocal EnableDelayedExpansion

rem Runs all automated tests (backend MSTest + frontend Vitest) and writes the
rem reports to Scripts\TestReports\<yyyyMMdd-HHmmss>\:
rem   backend.html        backend results, open in a browser
rem   backend.trx         backend results, open in Visual Studio
rem   frontend-junit.xml  frontend results (JUnit XML, readable by CI tools)
rem   summary.txt         pass/fail of each part
rem
rem Usage: RunTest.bat [Debug|Release] [nopause]
rem   Debug|Release  backend build configuration (default Debug). Use Release
rem                  while the API runs in Visual Studio (it locks Debug files).
rem   nopause        don't wait for a key at the end (for automation/CI).
rem Exit code: 0 = all tests passed, 1 = a test run failed.

cd /d "%~dp0"

set "CONFIG=Debug"
set "PAUSE_AT_END=1"
for %%a in (%*) do (
    if /i "%%~a"=="Debug" (set "CONFIG=Debug") else if /i "%%~a"=="Release" (set "CONFIG=Release") else if /i "%%~a"=="nopause" (set "PAUSE_AT_END=") else (
        echo Unknown argument "%%~a". Usage: RunTest.bat [Debug^|Release] [nopause]
        exit /b 2
    )
)

set "TEST_PROJECT=%~dp0..\VehicleManagement\VehicleManagement.Test"
set "WEB_DIR=%~dp0..\VehicleManagement\VehicleManagement.Web"

for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd-HHmmss"') do set "STAMP=%%i"
set "REPORT_DIR=%~dp0TestReports\%STAMP%"
mkdir "%REPORT_DIR%" || (echo Could not create "%REPORT_DIR%". & exit /b 1)

set "BACKEND_RESULT=FAILED"
set "FRONTEND_RESULT=FAILED"

echo.
echo ============================================================
echo  Step 1/2: Backend tests ^(MSTest, %CONFIG%^)
echo ============================================================
echo.

dotnet test "%TEST_PROJECT%" --configuration %CONFIG% --nologo ^
    --results-directory "%REPORT_DIR%" ^
    --logger "trx;LogFileName=backend.trx" ^
    --logger "html;LogFileName=backend.html" ^
    --logger "console;verbosity=normal"
if not errorlevel 1 set "BACKEND_RESULT=PASSED"

echo.
echo ============================================================
echo  Step 2/2: Frontend tests ^(Vitest^)
echo ============================================================
echo.

if not exist "%WEB_DIR%\node_modules\@angular\core" (
    echo npm packages are not installed. Run Start_Angular_Webapp.bat once, or
    echo "npm install" in VehicleManagement.Web, then try again.
    set "FRONTEND_RESULT=NOT RUN (npm packages missing)"
    goto :summary
)

pushd "%WEB_DIR%"
rem outputFile applies to the first reporter, so junit goes first; "default"
rem keeps the normal console output.
call npx ng test --watch=false --reporters=junit --reporters=default --output-file="%REPORT_DIR%\frontend-junit.xml"
if not errorlevel 1 set "FRONTEND_RESULT=PASSED"
popd

:summary
rem !var! (not %%var%%) inside the block: a result such as "NOT RUN (...)"
rem contains ")" which would otherwise end the block early.
(
    echo Test run %STAMP%
    echo Backend  ^(MSTest, %CONFIG%^): !BACKEND_RESULT!
    echo Frontend ^(Vitest^):          !FRONTEND_RESULT!
) > "%REPORT_DIR%\summary.txt"

echo.
echo ============================================================
echo  Backend  ^(MSTest, %CONFIG%^): %BACKEND_RESULT%
echo  Frontend ^(Vitest^):          %FRONTEND_RESULT%
echo.
echo  Reports: %REPORT_DIR%
echo ============================================================
echo.

set "EXIT_CODE=0"
if not "%BACKEND_RESULT%"=="PASSED" set "EXIT_CODE=1"
if not "%FRONTEND_RESULT%"=="PASSED" set "EXIT_CODE=1"

if defined PAUSE_AT_END pause
endlocal & exit /b %EXIT_CODE%
