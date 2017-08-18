;
; Copyright(C) 2017 Michal Heczko
; All rights reserved.
;
; This software may be modified and distributed under the terms
; of the BSD license.  See the LICENSE file for details.
;
#define AppId "{CF837F0A-2961-4BB9-8A07-51A16D9C2D75}"
#define AppName "Tray Radio"
#define AppVersion "1.5.1"
#define AppPublisher "Michal Heczko"
#define AppURL "https://trayradio.000webhostapp.com/"
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
DefaultDirName={pf}\{#AppName}
DisableProgramGroupPage=yes
OutputDir=.\
OutputBaseFilename={#AppName} {#AppVersion}
DefaultGroupName={#AppName}
SetupIconFile={#SourcePath}\TrayRadio\antenna (2)_signal_ico.ico
Compression=lzma
SolidCompression=yes
Uninstallable=yes
UninstallDisplayIcon={#SourcePath}\TrayRadio\antenna (2)_signal_ico.ico
ArchitecturesInstallIn64BitMode=x64
InfoBeforeFile={#SourcePath}\TrayRadio\Changelog.txt
LicenseFile={#SourcePath}\License

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Components]
Name: "src"; Description: "Source Codes";

[Files]
; Source Codes
Source: "{#SourcePath}\TrayRadio\*"; DestDir: "{app}\Sources\TrayRadio"; Components: "src"; Flags: recursesubdirs; Excludes: "bin\*,Help\*,obj\*,packages\*,*.exe";
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
Name: "{commonprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{commondesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; SubKey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: {#AppName}; Flags: dontcreatekey uninsdeletevalue;

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

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
