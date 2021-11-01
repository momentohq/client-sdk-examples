# Momento Sdk Examples

## Prerequisites
1. [.NET SDK](https://dotnet.microsoft.com/download)

## How To Run
1. `cd MomentoExamples`
1. `dotnet nuget add source https://momento.jfrog.io/artifactory/api/nuget/nuget-public --name Artifactory`
1. `dotnet build`
1. `MOMENTO_AUTH_TOKEN=<auth token> dotnet run --project MomentoApplication`

## Sample Code
[MomentoApplication](MomentoExamples/MomentoApplication/Program.cs)

## Configuration for your Project
SDK is built for target framework [.NET Standard 2.1 ](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md)

The MomentoSdk is available at https://momento.jfrog.io/ui/repos/tree/General/nuget-public

To add to your project

### CLI
```
dotnet add package MomentoSdk -s https://momento.jfrog.io/artifactory/api/nuget/nuget-public
```
