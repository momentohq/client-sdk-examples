## Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download)
- A Momento Auth Token is required, you can generate one using the [Momento CLI](https://github.com/momentohq/momento-cli)

## Running the Example
```bash
cd MomentoExamples
dotnet nuget add source https://momento.jfrog.io/artifactory/api/nuget/nuget-public --name Momento-Artifactory
dotnet build
MOMENTO_AUTH_TOKEN=<YOUR AUTH TOKEN> dotnet run --project MomentoApplication
```

Example Code: [MomentoApplication](MomentoExamples/MomentoApplication/Program.cs)

## Using the .NET SDK in your project
SDK is built for target framework [.NET Standard 2.1 ](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md)

The Momento SDK is available at https://momento.jfrog.io/ui/repos/tree/General/nuget-public

### CLI command to add to your project
```
dotnet nuget add source https://momento.jfrog.io/artifactory/api/nuget/nuget-public --name Momento-Artifactory
dotnet add package MomentoSdk
```
