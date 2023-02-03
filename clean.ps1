Param(
  [Alias("a")]   [string] $action = "clean" 
)

$component_bin_folder = "samples\components\bin"
$component_bin_trunk_folder = "samples.trunk\components\bin"


function DeleteSubFolder($folders) {
    foreach($folder in $folders) {
    
        DeleteFolder $folder
    }        
}
function fileExists([string]$fileName) {

    return test-path -PathType Leaf $fileName # << not reliable if we only path a path
}
function directoryExists([string]$fileName) {

    return test-path $fileName
}
function DeleteFiles($files) {

    foreach($file in $files) {
        DeleteFile $file
    }        
}

function DeleteFolder([string]$f) {
    
    if(directoryExists($f)) {
        if(($f.ToLowerInvariant().contains($component_bin_folder) -or $f.ToLowerInvariant().contains($component_bin_trunk_folder))) {
            "Never delete $f" 
        }
        else {
            "Deleting folder: $f"
            Remove-Item "$f" -Force -Recurse
        }
    }
}
function DeleteFile([string]$file) {

    if(fileExists([string]$file)) {
        "Deleting file: $file"
        Remove-Item "$file" -Force
    }
}

cls
"MUSB Source Code Management Tool"
$ROOT = "C:\DVT\MadeInTheUSB\FT232H.NET"
cd "$ROOT"

switch($action.ToLowerInvariant()) {

    "clean" { 
        "Cleaning... "

        #md "$ROOT\MadeInTheUSB.WebSite.Deployed"
        #DeleteFile "$ROOT\MUSB_FS.VC.db"
        DeleteSubFolder(Get-ChildItem -path "$ROOT" -rec -Directory -Include obj,bin)
        DeleteFiles(Get-ChildItem -path "$ROOT" -rec -File -Include *.sdf,*.ipch)
    }
    default { "invalid command line" }
}
