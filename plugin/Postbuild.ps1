Param (
    [Parameter(Mandatory=$false, Position=0)]
    $Source,

    [Parameter(Mandatory=$false, Position=1)]
    $Target
)

foreach($SourceFile in gci -Path $Source) {
    $TargetFileName = Join-Path $Target $SourceFile.Name
    $TargetFile = $null
    $TargetFile = Get-Item $TargetFileName -ErrorAction Ignore
    if($TargetFile -and $TargetFile.LastWriteTime -ge $SourceFile.LastWriteTime) {
        echo "$($TargetFile.FullName) is up-to-date"
        continue
    }
    copy $SourceFile.FullName $TargetFileName
    echo "Copied $($SourceFile.FullName) -> $($TargetFileName)"
}
