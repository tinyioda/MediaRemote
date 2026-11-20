; Inno Setup script for MediaRemote.
;
; This is intentionally NOT part of MediaRemote.slnx and is never built by
; `dotnet build` or an IDE's Debug/Release run — it's only compiled on demand
; by build-installer.ps1 when packaging a release. See installer/README.md.

#define MyAppName "MediaRemote"
#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#define MyAppPublisher "Justin Antoszek"
#define MyAppExeName "MediaRemote.exe"
#define PublishDir "..\src\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish"

[Setup]
AppId={{D3D2FD15-7B30-4DEF-9879-9E5CDE7E9DEB}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=Output
OutputBaseFilename=MediaRemoteSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; Lets the user choose at install time between an admin, all-users install
; (Program Files) and a non-admin, per-user install (AppData\Local\Programs) —
; the same choice apps like Chrome or VS Code offer.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#PublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

[Tasks]
Name: "startup_user"; Description: "Start automatically when I log in"; GroupDescription: "Startup options:"; Flags: exclusive
Name: "startup_all"; Description: "Start automatically when any user logs in (requires installing for all users)"; GroupDescription: "Startup options:"; Flags: exclusive unchecked; Check: IsAdminInstallMode
Name: "startup_none"; Description: "Do not start automatically"; GroupDescription: "Startup options:"; Flags: exclusive unchecked

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Tasks: startup_user; Flags: uninsdeletevalue
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Tasks: startup_all; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName} now"; Flags: nowait postinstall skipifsilent

[Code]
// Keeps the two Run-key entries in sync with whichever startup option is
// selected, including when a user re-runs the installer and switches
// options — [Tasks]/[Registry] alone only ever *add* a value, never remove
// the one left over from a previous choice.
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    if not WizardIsTaskSelected('startup_user') then
      RegDeleteValue(HKEY_CURRENT_USER, 'Software\Microsoft\Windows\CurrentVersion\Run', '{#MyAppName}');
    if not WizardIsTaskSelected('startup_all') then
      RegDeleteValue(HKEY_LOCAL_MACHINE, 'Software\Microsoft\Windows\CurrentVersion\Run', '{#MyAppName}');
  end;
end;
