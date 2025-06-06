#!/usr/bin/env bash

# ------------------------- Variables ------------------------- #

declare -rA versions=(
    #["0.5.x"]="OTD05"
    ["0.6.x"]="OTD06"
)

declare -rA targetFrameworks=(
    #["0.5.x"]="net5.0"
    ["0.6.x"]="net6.0"
)

declare -rA suffixes=(
    #["OTD05"]=""
    ["OTD06"]="-0.6.x"
)

declare -rA installers_needed=(
    #["OTD05"]=true
    ["OTD06"]=false
)

main=("ClickTap.dll"
      "ClickTap.pdb" 
      "ClickTap.Lib.dll" 
      "ClickTap.Lib.pdb"
      "OpenTabletDriver.External.Common.dll" 
      "OpenTabletDriver.External.Common.pdb")

# ------------------------- Arguments ------------------------- #

donotzip=false

if [ $# -eq 1 ] && [ "$1" == "--no-zip" ];
then
    donotzip=true
fi

# ------------------------- Functions ------------------------- #

create_build_structure () {
    # Check if build directory exists
    if [ -d "build" ];
    then
        # Clear the build directory if it exists
        echo "Removing all files in build"
        rm -rf build/plugin/$version/*
        rm -rf build/installer/$version/*
    else
        # Attempt to create the build directory if it does not exist
        if ! mkdir build 2> /dev/null;
        then
            if [ ! -d "build" ];
            then
                echo "Failed to create the 'build' directory"
                exit 1
            fi
        fi
    fi
}

create_plugin_structure () {
    (
        cd build

        #create plugin folder for the specified version
        if ! mkdir -p plugin/$version 2> /dev/null;
        then
            if [ ! -d "plugin/$version" ];
            then
                echo "Failed to create the 'build/plugin/$version' directory"
                exit 1
            fi
        fi

        #create installer folder for the specified version
        if ! mkdir -p installer/$version 2> /dev/null;
        then
            if [ ! -d "installer/$version" ];
            then
                echo "Failed to create the 'build/installer/$version' directory"
                exit 1
            fi
        fi
    )
}

move_files () {
    (
        local temp=temp/plugin/$version
        local build=build/plugin/$version

        if [ -d $temp ] && [ -d $build ];
        then
            cd $temp

            # Move specific files to the build/plugin directory
            for file in "${main[@]}"
            do
                # If file exist, move it to the build/plugin directory
                if [ -f "$file" ];
                then
                    mv $file ../../../$build/$file
                else
                    # If the file extension is not .pdb, then exit
                    if [[ $file != *.pdb ]];
                    then
                        echo "Failed to find $file in $version"
                        exit 1
                    fi
                fi
            done

            #check if donotzip is set to true
            if [ $donotzip == false ];
            then
                # Zip the files
                cd ../../../$build
                zip -r ClickTap-$version.zip ./*
            fi

        else
            echo "Failed to find temp or build folder for $version"
            exit 1
        fi
    )
}

build_installer () {
    echo ""
    echo "Building the installer for $version"
    echo ""

    # Build the installer
    if ! dotnet publish ClickTap.Installer -c Release -f $targetFramework -p:noWarn='"NETSDK1138;VSTHRD200"' -o build/installer/$version;
    then
        echo "Failed to build ClickTap.Installer for $version"
        exit 1
    fi

    (
        cd build/installer/$version

        # Zip the installer
        if ! zip -r "ClickTap.Installer-$version.zip" "ClickTap.Installer.dll"
        then
            echo "Failed to zip the installer"
            exit 1
        fi
    )
}

# ------------------------- Main ------------------------- #

echo ""
echo "Creating build directory structure"
echo ""

create_build_structure

for version in "${!versions[@]}"
do
    # Get suffix for the version (0.5.x -> OTD05 -> "")
    version_code=${versions[${version}]}
    targetFramework=${targetFrameworks[${version}]}
    suffix=${suffixes[${version_code}]}
    installer_needed=${installers_needed[${version_code}]}

    create_plugin_structure

    echo ""
    echo "Building Click & Tap $version"
    echo ""

    #Build the plugin, exit on failure
    if ! dotnet publish "ClickTap$suffix" -c Release -p:noWarn='"NETSDK1138;VSTHRD200"' -o temp/plugin/$version ;
    then
        echo "Failed to build Click & Tap for $version"
        exit 1
    fi

    echo ""
    echo "Moving necessary files to build/plugin"
    echo ""

    # Move files contained in variable main to build/plugin
    
    if ! move_files;
    then
        echo "Failed to move necessary files to build/plugin"
        exit 1
    fi

    # if donotzip is set to true, then skip the installer build
    if [ $donotzip == true ];
    then
        echo "Skipping installer build for $version"
        continue
    fi

    if [ $installer_needed == true ];
    then
        build_installer $version $suffix
    fi

done

# remove the temp directory
rm -rf "temp/"

echo ""
echo "Plugins Build complete"
echo ""