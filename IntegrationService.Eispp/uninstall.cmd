@ECHO OFF

echo RUN AS Administrator
echo Uninstalling WindowsService...
echo ---------------------------------------------------
 C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\InstallUtil /u IntegrationService.Eispp.exe
echo IntegrationService.Eispp.exe - This service parses and persists all schedule changes.
echo ---------------------------------------------------
echo Done.
pause