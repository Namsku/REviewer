@echo off
echo Killing REviewer Server instances...
FOR /F "tokens=5" %%P IN ('netstat -a -n -o ^| findstr :8000') DO taskkill /F /PID %%P
FOR /F "tokens=5" %%P IN ('netstat -a -n -o ^| findstr :5006') DO taskkill /F /PID %%P
echo Done.
pause
