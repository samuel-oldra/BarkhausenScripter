@echo off
echo I: %date% %time%
for %%f in (*.rms) do _Scripter.exe /RMSN %%f
echo F: %date% %time%