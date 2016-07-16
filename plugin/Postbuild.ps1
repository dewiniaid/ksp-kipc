# powershell $(SolutionDir)\Postbuild.ps1 -BuildDir $(ProjectDir)$(OutDir) -KSPDir $(SolutionDir)..\KSP -OutputDir $(SolutionDir)..\Output -ClientGen $(SolutionDir)..\Python\venv\scripts
Param (
    # Location of build artifacts
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]
    $BuildDir,

    # Solution dir.  Also used to generate defaults for other arguments.
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]
    $SolutionDir,

    # Where to store final output.
    [Parameter(Mandatory=$false)]
    [ValidateNotNullOrEmpty()]
    [string]
    $OutputDir,

    # Location of KSP
    [Parameter(Mandatory=$false)]
    [ValidateNotNullOrEmpty()]
    [string]
    $KSPDir,

    # Location of build scripts
    [Parameter(Mandatory=$false)]
    [ValidateNotNullOrEmpty()]
    [string]
    $ScriptsDir
)

$Languages = @(
    New-Object -TypeName PSObject -Property @{'Lang'='csharp'; 'File'='KIPC.cs'}
    New-Object -TypeName PSObject -Property @{'Lang'='cpp'; 'File'='KIPC.hpp'}
    New-Object -TypeName PSObject -Property @{'Lang'='java'; 'File'='KIPC.java'}
)

$BuildDir = (Get-Item $BuildDir).FullName
$SolutionDir = (Get-Item $SolutionDir).FullName

if(-not $OutputDir) { $OutputDir = Join-Path $SolutionDir "..\output" }
if(-not $KSPDir) { $KSPDir = Join-Path $SolutionDir "..\KSP" }
if(-not $ScriptsDir) { $ScriptsDir = Join-Path $SolutionDir "..\python/venv/scripts" }

$OutputDir = (Get-Item $OutputDir).FullName
$KSPDir = (Get-Item $KSPDir).FullName
$ScriptsDir = (Get-Item $ScriptsDir).FullName
$ClientGen = Join-Path $ScriptsDir "krpc-clientgen"

echo "BuildDir:    $BuildDir"
echo "SolutionDir: $SolutionDir"
echo "KSPDir:      $KSPDir"
echo "OutputDir:   $OutputDir"
echo "ScriptsDir:  $ScriptsDir"
echo "ClientGen:   $ClientGen"

# We're going to need a tempdir.  Make one
$TempPath = [System.IO.Path]::GetTempPath()
while($true) {
    $TempDir = Join-Path $TempPath ([System.IO.Path]::GetRandomFileName())
    mkdir $TempDir -ErrorVariable DirErr -ErrorAction SilentlyContinue | Out-Null
    if($DirErr) { continue }
    break
}
Register-EngineEvent -SourceIdentifier Powershell.Exiting -SupportEvent -Action {
    rmdir $TempDir -Recurse -Force
    echo "Removed $TempDir"
}
echo "TempDir:     $TempDir"

$ErrorActionPreference = "Stop"
$WarningPreference = "Stop"

# Create folder structure
mkdir $TempDir/Output | Out-Null
mkdir $TempDir/Output/Clients | Out-Null
mkdir $TempDir/Output/GameData/KIPC | Out-Null
mkdir $TempDir/Output/GameData/KIPC/Plugins | Out-Null
mkdir $TempDir/DLLs | Out-Null

# Copy DLLs.
$BuildDLLs = gci $BuildDir -Filter "*.dll"
$BuildDLLs | %{ Copy-Item $_.FullName $TempDir/Output/GameData/KIPC/Plugins -Force }
$BuildDLLs | %{ Copy-Item $_.FullName $TempDir/DLLs -Force }

$Dependencies = @(gci $KSPDir/GameData/KOS -Filter "*.dll" -Recurse) + @(gci $KSPDir/GameData/KRPC -Filter "*.dll" -Recurse)
$Dependencies | %{ Copy-Item $_.FullName $TempDir/DLLs -Force }

# Create AVC version file
$VersionInfo = $BuildDLLs | Where Name -Like "*KIPC*" | Select -First 1 | Select -ExpandProperty VersionInfo
$AVCJson = Get-Content $SolutionDir/KIPC.version.template | ConvertFrom-Json
$AVCJson.VERSION.MAJOR = $VersionInfo.FileMajorPart
$AVCJson.VERSION.MINOR = $VersionInfo.FileMinorPart
$AVCJson.VERSION.PATCH = $VersionInfo.FileBuildPart
$AVCJson.VERSION.BUILD = $VersionInfo.FilePrivatePart
$AVCJson | ConvertTo-Json -Compress | Set-Content $TempDir/Output/GameData/KIPC/KIPC.version 
Copy-Item $TempDir/Output/GameData/KIPC/KIPC.version $SolutionDir

# Build client libraries.
$ClientDefs = "$TempDir/Output/Clients/clientdefs.json"
Push-Location $TempDir/DLLs
$HaveJSON = $false
foreach($Language in $Languages) {
    $LangDir = Join-Path $TempDir/Output/Clients $Language.Lang
    $OutputFile = Join-Path $LangDir $Language.File
    mkdir $LangDir | Out-Null
    if($HaveJSON) {
        $Args = @($Language.Lang, "KIPC", $ClientDefs)
    } else {
        $HaveJSON = $true
        $Args = @($Language.Lang, "KIPC", "--ksp", $KSPDir, "--output-defs", $ClientDefs) + @($BuildDLLs | %{ $_.Name })
    }
    echo ("[clientgen] " + ($Args -Join " "))
    &$ClientGen $args | Out-File $OutputFile
}
Pop-Location

# Zip the output folder
$ZipName = "KIPC-$($VersionInfo.FileMajorPart).$($VersionInfo.FileMinorPart).$($VersionInfo.FileBuildPart).zip"
Compress-Archive -Path $TempDir/Output/* -DestinationPath "$TempDir/Output/$ZipName" -Force -Verbose
# Move-Item $TempDir/KIPC.zip $TempDir/Output/$ZipName

# Move the output dir to where it should be.
robocopy /MIR $Tempdir/Output $OutputDir /NJH /NJS /NFL /NDL /NP

# Mirror to KSP
robocopy /MIR $OutputDir/Gamedata/KIPC $KSPDir/Gamedata/KIPC /NJH /NJS /NFL /NDL /NP

# Remove temp directories
Remove-Item -Recurse $TempDir
