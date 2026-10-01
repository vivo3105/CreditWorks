@echo off
setlocal

rem Starts the API on http://localhost:5255 (launch profile "http").
rem Swagger UI: http://localhost:5255/swagger
rem Usage: StartApi.bat [Debug|Release]   (default: Debug)
rem Press Ctrl+C to stop.

cd /d "%~dp0"

set "CONFIG=%~1"
if not defined CONFIG set "CONFIG=Debug"
if /i not "%CONFIG%"=="Debug" if /i not "%CONFIG%"=="Release" (
    echo Unknown configuration "%CONFIG%". Use Debug or Release.
    exit /b 2
)

set "API_PROJECT=..\VehicleManagement\VehicleManagement.Api"

rem The web app expects the API on port 5255; refuse to start a second copy.
netstat -ano | findstr /r /c:":5255 .*LISTENING" >nul
if not errorlevel 1 (
    echo.
    echo Port 5255 is already in use - the API is probably already running
    echo ^(e.g. from Visual Studio^). Stop it first, or use the running one.
    echo.
    endlocal & exit /b 1
)

echo.
echo ============================================================
echo  Starting API (%CONFIG%) on http://localhost:5255
echo  Swagger UI: http://localhost:5255/swagger
echo  Press Ctrl+C to stop.
echo ============================================================
echo.

dotnet run --project "%API_PROJECT%" --launch-profile http --configuration %CONFIG%
set "RESULT=%ERRORLEVEL%"

endlocal & exit /b %RESULT%
