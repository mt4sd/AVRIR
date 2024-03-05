echo off


set pathFile=%~dp0Desktop\AVRIR.exe

netsh advfirewall firewall show rule name=%pathFile% > nul 2>&1
echo %pathFile%
if '%errorlevel%' NEQ '0' (
    start %~dp0Desktop/AVRIR-AdminRequest.bat
) else (
    echo Exception already added to firewall. Continue execution...
    start %pathFile%
)
