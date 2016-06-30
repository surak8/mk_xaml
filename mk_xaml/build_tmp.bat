@echo off
setlocal
set DIR=%~dp0
set FILES=%DIR%App.xaml.cs %DIR%Source\Views\ViewFileName.xaml.cs %DIR%Source\Models\ViewFileNameViewModel.cs %DIR%MainWindow.xaml.cs %DIR%MainWindowViewModel.cs
set FILES=%DIR%Source\Views\MainWindow.xaml.cs %DIR%Source\Models\MainWindowViewModel.cs
set REFS=-r:system.dll -r:System.Xaml.dll -r:PresentationFramework.dll -r:presentationcore.dll -r:windowsbase.dll
set LIBPATH=-lib:"%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0"
csc -nologo -t:library -d:debug;trace -debug+ -debug:full -doc:zzz.xml %FILES% %LIBPATH% %REFS% 
