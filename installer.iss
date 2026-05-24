; InnoSetup script for Fluxstrap
; Requires InnoSetup 6+ (https://jrsoftware.org/isdl.php)
; Usage: ISCC installer.iss

#define MyAppName "Fluxstrap"
#define MyAppVersion "2.11.3"
#define MyAppPublisher "Fluxstrap"
#define MyAppURL "https://github.com/Fluxstrap-Lab/Fluxstrap"
#define MyAppExeName "Fluxstrap.exe"

[Setup]
AppId={{B1E8B5A2-8F1C-4A5E-9D3C-7F2E6A1B4D8C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={localappdata}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableDirPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=Output
OutputBaseFilename=Fluxstrap-{#MyAppVersion}-Setup
SetupIconFile=Fluxstrap\Fluxstrap.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ChangesAssociations=no
AllowNoIcons=yes

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "tr"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"
Name: "fr"; MessagesFile: "compiler:Languages\French.isl"
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "it"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "ru"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "pt_BR"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "nl"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "pl"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "ja"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "ko"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "ar"; MessagesFile: "compiler:Languages\Arabic.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"; Flags: checkedonce
Name: "startmenuicon"; Description: "Create a &Start Menu shortcut"; GroupDescription: "Additional icons:"; Flags: checkedonce

[Files]
Source: "Release\fdd\Fluxstrap.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Fluxstrap\Fluxstrap.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startmenuicon
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\{#MyAppExeName}"; Parameters: "--uninstall"; Flags: runhidden

[Code]
function InitializeUninstall(): Boolean;
begin
  Result := True;
end;
