#!/bin/bash

echo "================================================"
echo "    petpal API - Pet Mutual Help Platform"
echo "================================================"
echo

echo "[1/4] Checking .NET environment..."
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not detected. Please install .NET 6.0 or higher."
    echo "Download: https://dotnet.microsoft.com/download"
    exit 1
fi
echo ".NET environment check passed."
echo

echo "[2/4] Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to restore NuGet packages."
    exit 1
fi
echo "NuGet packages restored successfully."
echo

echo "[3/4] Running database migration..."
echo "Using ApplicationDbContext for migration..."
dotnet ef database update --context ApplicationDbContext --no-build
if [ $? -ne 0 ]; then
    echo "WARNING: Database migration failed. You may need to configure the database connection string manually."
    echo "Please check the connection string in appsettings.json:"
    echo "- MySQL Server: 121.40.86.149:3306"
    echo "- Database: petpal"
    echo "- Make sure your MySQL server is running and accessible"
    echo
else
    echo "Database migration completed successfully."
fi
echo

echo "[4/4] Starting the application..."
echo "================================================"
echo "Application is starting..."
echo
echo "API Documentation: http://127.0.0.1:5002 (Swagger UI)"
echo "Or visit: https://127.0.0.1:5003 (if HTTPS is available)"
echo
echo "Press Ctrl+C to stop the service"
echo "================================================"
echo

echo "Starting application with custom URLs..."
dotnet run --urls "http://127.0.0.1:5002;https://127.0.0.1:5003"
