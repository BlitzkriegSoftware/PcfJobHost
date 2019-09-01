#!/bin/bash
publishDir="bin/linux-x64"

# Build your jobs
pushd ../ExampleJob
dotnet build -o ${publishDir} --configuration debug --framework netstandard2.0 --runtime linux-x64 -restore:True ExampleJob.csproj 
popd

# Build host
dotnet publish -o ${publishDir} --configuration debug --framework netcoreapp2.2 --runtime linux-x64  -restore:True  PcfJobHostApp.csproj 

# Copy job(s) to Jobs/ folder
cp ../ExampleJob/${publishDir}/Example*.* ${publishDir}/Jobs

# Push results to PCF
# cf push -f Manifest.yml 