# powershell $(SolutionDir)\Postbuild.ps1 -BuildDir $(ProjectDir)$(OutDir) -KSPDir $(SolutionDir)..\KSP -OutputDir $(SolutionDir)..\Output -ClientGen $(SolutionDir)..\Python\venv\scripts


Param (
    [Parameter(Mandatory=$false, Position=0)]
    $BuildDir = "D:\git\KIPC\plugin\KIPCPlugin\bin\Release\",

    [Parameter(Mandatory=$false, Position=1)]
    $OutputDir = "D:\git\KIPC\output\",

    [Parameter(Mandatory=$false, Position=2)]
    $KSPDir = "D:\git\KIPC\KSP\",

    [Parameter(Mandatory=$false, Position=3)]
    $ScriptsDir = "D:\git\KIPC\Python\venv\scripts\"
)

$BuildDir = (Get-Item $BuildDir).FullName
$OutputDir = (Get-Item $OutputDir).FullName
$KSPDir = (Get-Item $KSPDir).FullName
$ScriptsDir = (Get-Item $ScriptsDir).FullName

mkdir $BuildDir\temp -ErrorAction Ignore | Out-Null 
mkdir $BuildDir\temp\stubs  -ErrorAction Ignore | Out-Null 

# Identify build DLLs
$BuildDLLs = Get-Item $BuildDir/*.dll

# Clean and rebuild the output dir.
mkdir $OutputDir -ErrorAction Ignore
Remove-Item $OutputDir/Gamedata -Recurse -Verbose -Force

# Build our mirror of gamedata.
mkdir $OutputDir/GameData -Verbose -ErrorAction Ignore | Out-Null
mkdir $OutputDir/GameData/KIPC  -Verbose -ErrorAction Ignore | Out-Null
mkdir $OutputDir/GameData/KIPC/Plugins  -Verbose -ErrorAction Ignore | Out-Null
$BuildDLLs | %{ Copy-Item $_ $OutputDir/GameData/KIPC/Plugins -Verbose -Force }

Compress-Archive -Path $OutputDir/Gamedata -DestinationPath $Outputdir/KIPC.zip -Force

# Mirror to KSP
robocopy /MIR $OutputDir/Gamedata/KIPC $KSPDir/Gamedata/KIPC

# TODO: Fix the rest of this to generate client libraries correctly.


exit





# Find additional dependencies.
$DependencyDLLs = @(gci $KSPDir/GameData/kOS -recurse -Filter "*.dll")
foreach($Dep in "Assembly-CSharp*.dll", "KSP*.dll", "System*.dll", "UnityEngine*.dll") {
    $DependencyDLLs += gci $KSPDir/KSP_x64_Data/Managed -Filter $Dep
}
# + @(gci $KSPDir/KSP_Data/Managed -recurse -Filter "*.dll")
#  (gci $KSPDir/GameData/KRPC -recurse -Filter "*.dll") + 

# Move all to tempdir.
$BuildDLLs | %{ Copy-Item $_.FullName $BuildDir/temp -Verbose -Force }
$DependencyDLLs | %{ Copy-Item $_.FullName $BuildDir/temp -Verbose -Force }
Push-Location $BuildDir/temp
$Stubber = "D:\Git\KIPC\Build\Stubber.exe"
$StubberParams = @($BuildDLLs | %{ $_.Name }) + @("stubs\");
&$Stubber $StubberParams
$StubberParams = @($DependencyDLLs | %{ $_.Name }) + @("stubs\");
&$Stubber $StubberParams

$ClientGenArgs = @("--ksp", $KSPDir, "csharp", "KIPC", "--output", "$OutputDir/plugin.defs") + @($BuildDLLs | %{ $_.Name }) + @($DependencyDLLs | %{ "stubs\" + $_.Name })
$ClientGenArgs += @(gci $KSPDir/GameData/RRPC -recurse -Filter "*.dll" | %{ $_.FullName });
$ClientGen = (Get-Item "$ScriptsDir\krpc-clientgen.exe").FullName
&$ClientGen $ClientGenArgs
echo $ClientGen ($ClientGenArgs -join (" "))
Pop-Location

exit
