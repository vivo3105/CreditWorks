@echo off
setlocal

rem Starts the Angular web app with the dev server (ng serve).
rem Installs npm packages first if they are missing.
rem Usage: Start_Angular_Webapp.bat [port]   (default: 4200)
rem Press Ctrl+C to stop. The API must be running too (StartApi.bat).
rem
rem Note: inside ( ) blocks, literal parentheses in echo text must be
rem escaped as ^( ^) or they end the block early.

set "PORT=%~1"
if not defined PORT set "PORT=4200"

rem npm start needs to run inside the web project folder.
cd /d "%~dp0..\VehicleManagement\VehicleManagement.Web"
if errorlevel 1 (
    echo Could not find VehicleManagement\VehicleManagement.Web.
    exit /b 1
)

rem No -p filter: the dev server listens on IPv6 ([::1]) as well as IPv4.
netstat -ano | findstr /r /c:":%PORT% .*LISTENING" >nul
if not errorlevel 1 (
    echo.
    echo Port %PORT% is already in use - the web app may already be running.
    echo Stop it first, or start on another port, e.g.: Start_Angular_Webapp.bat 4300
    echo.
    endlocal & exit /b 1
)

if not exist "node_modules\@angular\core" (
    echo.
    echo ============================================================
    echo  Installing npm packages ^(first run^)
    echo ============================================================
    echo.
    call npm install
    if errorlevel 1 (
        echo.
        echo npm install FAILED. See the errors above.
        endlocal & exit /b 1
    )
)

echo.
echo ============================================================
echo  Starting Angular app on http://localhost:%PORT%
echo  API expected on http://localhost:5255 ^(StartApi.bat^)
echo  Press Ctrl+C to stop.
echo ============================================================
echo.

call npm start -- --port %PORT%
set "RESULT=%ERRORLEVEL%"

endlocal & exit /b %RESULT%
