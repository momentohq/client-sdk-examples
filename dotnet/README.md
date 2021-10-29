# Momento Sdk Examples

## Prerequisites
1. [dotnet](https://dotnet.microsoft.com/download)
1. on macos `brew install --cask dotnet`

## How To Run
1. `cd MomentoExamples`
1. `dotnet nuget add source https://momento.jfrog.io/artifactory/api/nuget/nuget-public --name Artifactory`
1. `dotnet build`
1. `MOMENTO_AUTH_TOKEN=<auth token> dotnet run --project MomentoApplication`

## Sample Code
[MomentoApplication](MomentoExamples/MomentoApplication/Program.cs)
