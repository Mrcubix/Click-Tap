#!/usr/bin/env bash

# ------------------------- Variables ------------------------- #

donotzip=false

if [ $# -eq 1 ] && [ "$1" == "--no-zip" ];
then
    donotzip=true
fi

# ------------------------- Functions ------------------------- #

zip_dir_contents () {
    echo "Zipping $f"
    (
        cd $1
        zip -r $2 ./*
    )
}

# ------------------------- Main ------------------------- #

if [ -d "./build/ux" ]; then
    rm -rf ./build/ux/*
fi

echo ""
echo "Building ClickTap.UX.Desktop"
echo ""

# build the desktop on linux & windows
platforms=("linux-x64" "linux-arm64" "linux-arm" "win-x64" "win-x86" "win-arm64")

for platform in "${platforms[@]}"
do
    dotnet publish ClickTap.UX.Desktop -c Release -p:noWarn='"NETSDK1138;VSTHRD200"' -r $platform -o build/ux/$platform 
done

# build the desktop on mac separately as they require specific parameters to work properly?
macplatforms=("osx-x64" "osx-arm64")

for platform in "${macplatforms[@]}"
do

    dotnet publish ClickTap.UX.Desktop -c Release -p:noWarn='"NETSDK1138;VSTHRD200"' -r $platform -o build/ux/$platform -p:SelfContained=true  -p:PublishSingleFile=false -p:PublishTrimmed=false -p:IncludeNativeLibrariesForSelfExtract=false
done

find ./build/ux -name "*.pdb" -type f -delete

if [ $donotzip == true ];
then
    echo "Skipping zipping of UX"
    exit 0
fi

# zip all the files
(
    cd ./build/ux

    for f in *; do
        if [ -d "$f" ]; then
            zip_dir_contents $f "../ClickTap.UX-$f.zip"
        fi
    done
)

echo ""
echo "UX built successfully"
echo ""