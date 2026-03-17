; DashLook Inno Setup Script
; Developer: ARG RABBI — https://itrabbi24.github.io/

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\dist\win-setup"
#endif

#define MyAppName      "DashLook"
#define MyAppPublisher "ARG RABBI"
#define MyAppURL       "https://itrabbi24.github.io/"
#define MyAppExe       "DashLook.exe"
#define MyAppGUID      "{DCA7F3B1-4E2A-4F3D-9B1C-8E2D7A4B5C6F}"

[Setup]
AppId={#MyAppGUID}
AppName={#MyAppName}
AppVersion={#AppVersion}
AppVerName={#MyAppName} {#AppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=License.rtf
OutputDir=bin\Release
OutputBaseFilename=DashLook-{#AppVersion}-Setup
SetupIconFile=..\assets\DashLook.ico
UninstallDisplayIcon={app}\{#MyAppExe}
UninstallDisplayName={#MyAppName}
WizardStyle=modern
WizardResizable=yes
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
WizardImageFile=compiler:WizModernImage.bmp
WizardSmallImageFile=compiler:WizModernSmallImage.bmp

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce
Name: "startup"; Description: "Start DashLook automatically when Windows starts"; GroupDescription: "Options:"

[Files]
Source: "{#PublishDir}\{#MyAppExe}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "DashLook"; ValueData: """{app}\{#MyAppExe}"""; Tasks: startup; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\DashLook"; ValueType: string; ValueName: "Developer"; ValueData: "ARG RABBI"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\DashLook"; ValueType: string; ValueName: "Version"; ValueData: "{#AppVersion}"

[Run]
Filename: "{#MyAppURL}"; Description: "Visit developer website (itrabbi24.github.io)"; Flags: nowait postinstall shellexec skipifsilent unchecked
Filename: "{app}\{#MyAppExe}"; Description: "Launch DashLook"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "taskkill.exe"; Parameters: "/F /IM {#MyAppExe}"; Flags: runhidden; RunOnceId: "KillDashLook"
