@echo off
chcp 65001 >nul
echo ================================================
echo    Environment Check - petpal API
echo ================================================
echo Note: If you see this message, the script is working correctly!
echo If you got an error before this, make sure to run it as: .\check-env.bat
echo.
echo.

echo [1/3] Checking .NET SDK...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 6.0 or higher from:
    echo https://dotnet.microsoft.com/download
    echo.
    echo Current PATH: %PATH%
    echo.
) else (
    echo SUCCESS: .NET SDK found.
    dotnet --version
)
echo.

echo [2/3] Checking MySQL connectivity...
echo Note: This requires MySQL to be running on 121.40.86.149:3306.
echo Testing basic network connectivity first...
ping -n 1 121.40.86.149 >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Cannot reach MySQL server at 121.40.86.149
    echo Check network connectivity and server status.
) else (
    echo SUCCESS: Server is reachable via network.
)
echo.

echo Testing MySQL port connectivity...
powershell -Command "Test-NetConnection -ComputerName 121.40.86.149 -Port 3306 -InformationLevel Quiet" >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Cannot connect to MySQL port 3306
    echo Check firewall settings and MySQL service status.
) else (
    echo SUCCESS: MySQL port is accessible.
)
echo.

echo Testing application database connection...
dotnet ef dbcontext info --no-build >nul 2>&1
if %errorlevel% neq 0 (
    echo WARNING: Application cannot connect to MySQL database.
    echo Possible issues:
    echo - MySQL server configuration problem
    echo - Connection string incorrect
    echo - SSL/TLS requirements
    echo - User permissions insufficient
    echo.
    echo Run .\test-mysql-connection.bat for detailed diagnostics.
) else (
    echo SUCCESS: Application can connect to MySQL database.
)
echo.

echo [3/3] Checking project structure...
if exist "petpal.csproj" (
    echo SUCCESS: Project file found.
) else (
    echo ERROR: Project file not found!
    echo Please make sure you're in the correct directory.
)
echo.

echo ================================================
echo Environment check completed.
echo If you see any ERROR messages above, please fix them before running the application.
echo ================================================
echo.
pause
