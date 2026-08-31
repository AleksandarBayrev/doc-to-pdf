#!/bin/bash
rm -rfv bin/
rm -rfv obj/
rm -rfv publish/
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --output ./publish/linux-x64
echo "Publish completed successfully."