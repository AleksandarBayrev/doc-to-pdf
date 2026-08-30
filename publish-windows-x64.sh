#!/bin/bash
rm -rfv bin/
rm -rfv obj/
rm -rfv publish/
dotnet publish -c Release -r windows-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:PublishTrimmed=true --output ./publish/windows-x64
echo "Publish completed successfully."