#define AppName "PRISM"
#define AppVersion "1.1.0"
#define AppPublisher "PRISM"
#define AppExe "Prism.exe"

[Setup]
AppId={{2E3A8D6A-6E1B-4A59-9F77-8F8A6C2A1B77}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppComments=PRISM Crosshair Studio
DefaultDirName={localappdata}\Programs\PRISM
DefaultGroupName=PRISM
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=..
OutputBaseFilename=PRISM-Setup-{#AppVersion}
SetupIconFile=..\..\App\Assets\prism.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
WizardImageFile=..\Assets\WizardImage.bmp
WizardSmallImageFile=..\Assets\WizardSmallImage.bmp
WizardImageStretch=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
CloseApplications=yes
RestartApplications=no
AppMutex=Local\PrismCrosshairStudio
VersionInfoVersion=1.1.0.0
VersionInfoCompany={#AppPublisher}
VersionInfoDescription=PRISM Crosshair Studio Setup
VersionInfoProductName=PRISM
VersionInfoProductVersion={#AppVersion}
VersionInfoOriginalFileName=PRISM-Setup-1.1.0.exe
SetupLogging=yes
ShowLanguageDialog=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[CustomMessages]
english.AdditionalIcons=Additional shortcuts:
english.DesktopIcon=Create a desktop shortcut
english.UninstallIcon=Uninstall PRISM
english.LaunchApp=Launch PRISM
russian.AdditionalIcons=Дополнительные ярлыки:
russian.DesktopIcon=Создать ярлык на рабочем столе
russian.UninstallIcon=Удалить PRISM
russian.LaunchApp=Запустить PRISM

[Tasks]
Name: "desktopicon"; Description: "{cm:DesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "..\..\Portable\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "README*.md,START.txt,*.zip,screenshots\*"

[Icons]
Name: "{autoprograms}\PRISM"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\PRISM"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon
Name: "{group}\{cm:UninstallIcon}"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchApp}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\logs"

