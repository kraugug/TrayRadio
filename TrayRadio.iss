;
; Copyright(C) 2019 Michal Heczko
; All rights reserved.
;
; This software may be modified and distributed under the terms
; of the BSD license.  See the LICENSE file for details.
;
#define AppId "{CF837F0A-2961-4BB9-8A07-51A16D9C2D75}"
#define AppName "Tray Radio"
#define AppVersion "1.6.0"
#define AppPublisher "Michal Heczko"
#define AppURL "http://trayradio.kraugug.net"
#define AppExeName "TrayRadio.exe"

#define Path32Binary SourcePath + "\TrayRadio\bin\x86\Release\"
#define Path64Binary SourcePath + "\TrayRadio\bin\x64\Release\"

[Setup]
AppId={{#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
ArchitecturesInstallIn64BitMode=x64
CloseApplications=yes
Compression=lzma
DefaultDirName={pf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
InfoBeforeFile={#SourcePath}\TrayRadio\Changelog.txt
LicenseFile={#SourcePath}\License
OutputDir=.\
OutputBaseFilename={#AppName} {#AppVersion}
SetupIconFile={#SourcePath}\TrayRadio\antenna (2)_signal_ico.ico
SolidCompression=yes
Uninstallable=yes
UninstallDisplayIcon={#SourcePath}\TrayRadio\antenna (2)_signal_ico.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[Components]
Name: "src"; Description: "Source Codes";

[Files]
; Source Codes
Source: "{#SourcePath}\TrayRadio\*"; DestDir: "{app}\Sources\TrayRadio"; Components: "src"; Flags: recursesubdirs; Excludes: "bin\*,Help\*,obj\*,packages\*,*.exe,.vs\*";
Source: "{#SourcePath}\www\*"; DestDir: "{app}\Sources\www"; Components: "src"; Flags: recursesubdirs;
Source: "{#SourcePath}\TrayRadio\Changelog.txt"; DestDir: "{app}";
Source: "{#SourcePath}\License"; DestDir: "{app}"; DestName: "License.txt";
; Binaries
; 32bit
Source: "{#Path32Binary}\TrayRadio.exe"; DestDir: "{app}"; Check: not Is64BitInstallMode;
Source: "{#Path32Binary}\Bass.Net.dll"; DestDir: "{app}"; Check: not Is64BitInstallMode;
Source: "{#Path32Binary}\\Bass.dll"; DestDir: "{syswow64}"; Check: not Is64BitInstallMode;
; 64 bit
Source: "{#Path64Binary}\TrayRadio.exe"; DestDir: "{app}"; Check: Is64BitInstallMode;
Source: "{#Path64Binary}\Bass.Net.dll"; DestDir: "{app}"; Check: Is64BitInstallMode;
Source: "{#Path64Binary}\Bass.dll"; DestDir: "{sys}"; Check: Is64BitInstallMode;

[Icons]
Name: {group}\{#AppName}; Filename: "{app}\{#AppExeName}"; Flags: foldershortcut;
Name: {group}\Changelog; Filename: {app}\Changelog.txt
Name: {group}\License; Filename: {app}\License.txt
Name: {group}\{cm:UninstallProgram,{#AppName} v{#AppVersion}}; Filename: {uninstallexe}
Name: {commondesktop}\{#AppName}; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon;

[Registry]
Root: HKCU; SubKey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: {#AppName}; Flags: dontcreatekey uninsdeletevalue;

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent;

[UninstallDelete]
Type: dirifempty; Name: "{group}";
Type: filesandordirs; Name: "{app}";
Type: filesandordirs; Name: "{localappdata}\Michal_Heczko";

[Code]
function InitializeSetup: Boolean;
var
	Uninstaller: String;
	ErrorCode: Integer;
begin
    Result := not RegQueryStringValue(HKLM, ExpandConstant('SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{#AppId}_is1\'), 'UninstallString', Uninstaller);
	if not Result then
		if (MsgBox(ExpandConstant('{#AppName} is already installed, you must uninstall it first! Do you wish to uninstall it now?'), mbConfirmation, MB_YESNO) = IDYES) then begin
			Result := ShellExec('open', Uninstaller, '/SILENT', '', SW_SHOW, ewWaitUntilTerminated, ErrorCode)
			if not Result then
				MsgBox(SysErrorMessage(ErrorCode), mbError, MB_OK)
		end else
			Result := False;
end;

procedure RegisterExtraCloseApplicationsResources;
begin
	RegisterExtraCloseApplicationsResource(true, ExpandConstant('{app}\TrayRadio.exe'));
end;
