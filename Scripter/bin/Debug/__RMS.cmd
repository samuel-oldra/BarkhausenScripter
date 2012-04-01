@echo off
echo I: %date% %time%
for %%f in (*.lvm) do _Scripter.exe /RMS %%f
echo F: %date% %time%