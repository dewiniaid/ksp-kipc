# powershell $(SolutionDir)\Postbuild.ps1 -BuildDir $(ProjectDir)$(OutDir) -KSPDir $(SolutionDir)..\KSP -OutputDir $(SolutionDir)..\Output -ClientGen $(SolutionDir)..\Python\venv\scripts
Param (
    [Parameter(Mandatory=$true, Position=0)]
    $BuildDir,

    [Parameter(Mandatory=$true, Position=1)]
    $OutputDir,

    [Parameter(Mandatory=$true, Position=2)]
    $KSPDir,

    [Parameter(Mandatory=$true, Position=3)]
    $ScriptsDir
)

$Languages = @(
    New-Object -TypeName PSObject -Property @{'Lang'='csharp'; 'File'='KIPC.cs'}
    New-Object -TypeName PSObject -Property @{'Lang'='cpp'; 'File'='KIPC.hpp'}
    New-Object -TypeName PSObject -Property @{'Lang'='java'; 'File'='KIPC.java'}
)

$BuildDir = (Get-Item $BuildDir).FullName
$OutputDir = (Get-Item $OutputDir).FullName
$KSPDir = (Get-Item $KSPDir).FullName
$ScriptsDir = (Get-Item $ScriptsDir).FullName
$ClientGen = Join-Path $ScriptsDir "krpc-clientgen"

# We're going to need a tempdir.  Make one
$TempPath = [System.IO.Path]::GetTempPath()
while($true) {
    $TempDir = Join-Path $TempPath ([System.IO.Path]::GetRandomFileName())
    mkdir $TempDir -ErrorVariable DirErr -ErrorAction SilentlyContinue
    if($DirErr) { continue }
    break
}
Register-EngineEvent -SourceIdentifier Powershell.Exiting -SupportEvent -Action {
    rmdir $TempDir -Recurse -Verbose -Force
}

# Create folder structure
mkdir $TempDir/Output | Out-Null
mkdir $TempDir/Output/Clients | Out-Null
mkdir $TempDir/Output/GameData/KIPC | Out-Null
mkdir $TempDir/Output/GameData/KIPC/Plugins | Out-Null
mkdir $TempDir/DLLs | Out-Null

# Copy DLLs.
$BuildDLLs = gci $BuildDir -Filter "*.dll"
$BuildDLLs | %{ Copy-Item $_.FullName $TempDir/Output/GameData/KIPC/Plugins -Verbose -Force }
$BuildDLLs | %{ Copy-Item $_.FullName $TempDir/DLLs -Verbose -Force }

$Dependencies = @(gci $KSPDir/GameData/KOS -Filter "*.dll" -Recurse) + @(gci $KSPDir/GameData/KRPC -Filter "*.dll" -Recurse)
$Dependencies | %{ Copy-Item $_.FullName $TempDir/DLLs -Verbose -Force }

# Build client libraries.
$ClientDefs = "$TempDir/Output/Clients/clientdefs.json"
Push-Location $TempDir/DLLs
$HaveJSON = $false
foreach($Language in $Languages) {
    $LangDir = Join-Path $TempDir/Output/Clients $Language.Lang
    $OutputFile = Join-Path $LangDir $Language.File
    echo $OutputFile
    mkdir $LangDir | Out-Null
    if($HaveJSON) {
        $Args = @($Language.Lang, "KIPC", $ClientDefs)
    } else {
        $HaveJSON = $true
        $Args = @($Language.Lang, "KIPC", "--ksp", $KSPDir, "--output-defs", $ClientDefs) + @($BuildDLLs | %{ $_.Name })
    }
    echo ($ClientGen + " " + ($Args -Join " "))
    &$ClientGen $args | Out-File $OutputFile
}
Pop-Location

# zip the output folder
Compress-Archive -Path $TempDir/Output/* -DestinationPath $TempDir/KIPC.zip -Force -Verbose
Move-Item $TempDir/KIPC.zip $TempDir/Output/KIPC.zip

# Move the output dir to where it should be.
robocopy /MIR $Tempdir/Output $OutputDir

# Mirror to KSP
robocopy /MIR $OutputDir/Gamedata/KIPC $KSPDir/Gamedata/KIPC
