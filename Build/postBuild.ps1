param(
    [Parameter(Mandatory=$True)]
    [System.String]
    $solutionDir,
    [Parameter(Mandatory=$True)]
    [System.String]
    $targetDir,
    [Parameter(Mandatory=$True)]
    [System.String]
    $targetFile
)

$target = "$targetDir\$targetFile";
$output = "$solutionDir\Out\";

if(-not (Test-Path $output)){
    New-Item $output -ItemType Directory;
}

Copy-Item -Path $target -Destination $output -Force;