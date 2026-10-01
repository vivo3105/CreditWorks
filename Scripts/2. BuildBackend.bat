@echo off
setlocal

rem Builds the backend solution (Domain, Application, Infrastructure, Api, Test).
rem Usage: BuildBackend.bat [Debug|Release]   (default: Debug)
rem
rem If the API is running from Visual Studio, a Debug build can fail with
rem "file is locked"; stop the API or build Release instead.

cd /d "%~dp0"

set "CONFIG=%~1"
if not defined CONFIG set "CONFIG=Debug"
if /i not "%CONFIG%"=="Debug" if /i not "%CONFIG%"=="Release" (
    echo Unknown configuration "%CONFIG%". Use Debug or Release.
    exit /b 2
)

set "SOLUTION=..\VehicleManagement\VehicleManagement.sln"

echo.
echo ============================================================
echo  Building backend (%CONFIG%)
echo ============================================================
echo.

dotnet build "%SOLUTION%" --configuration %CONFIG% --nologo
if errorlevel 1 goto :failed

echo.
echo Build succeeded (%CONFIG%).
echo.
endlocal & exit /b 0

:failed
echo.
echo Build FAILED. See the errors above.
echo If you see "file is locked", stop the running API or run: BuildBackend.bat Release
echo.
endlocal & exit /b 1
