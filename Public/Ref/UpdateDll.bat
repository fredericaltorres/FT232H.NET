echo off
cls
echo copying
set DLL_64=C:\DVT\FT232H.NET\Private\MadeInTheUSB.FT232H.Lib\libmpsse-windows-1.0.3\release\Project\VisualStudio\libmpsse.dll\x64\Debug\libmpsse.dll
set DLL_32=C:\DVT\FT232H.NET\Private\MadeInTheUSB.FT232H.Lib\libmpsse-windows-1.0.3\release\Project\VisualStudio\libmpsse.dll\Win32\Debug\libmpsse.dll
copy  "%DLL_32%" .
echo done

pause
