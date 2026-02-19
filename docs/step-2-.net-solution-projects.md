# Step 2 — .NET Solution + Projects

Run these commands from your repo root. I'll explain what each block does before you run it.

## 2.1 — Create the solution and projects

```bash
# Create the solution file
dotnet new sln -n HL7EndpointMirror

# Create the Azure Functions project (.NET 10 Isolated Worker)
dotnet new func --worker-runtime dotnet-isolated --target-framework net10.0 -n HL7EndpointMirror.Functions -o src/HL7EndpointMirror.Functions

# Create the xUnit test project
dotnet new xunit -n HL7EndpointMirror.Tests -o tests/HL7EndpointMirror.Tests --framework net10.0

# Add both projects to the solution
dotnet sln add src/HL7EndpointMirror.Functions/HL7EndpointMirror.Functions.csproj
dotnet sln add tests/HL7EndpointMirror.Tests/HL7EndpointMirror.Tests.csproj

# Add a project reference from Tests → Functions
dotnet add tests/HL7EndpointMirror.Tests/HL7EndpointMirror.Tests.csproj reference src/HL7EndpointMirror.Functions/HL7EndpointMirror.Functions.csproj
```

## 2.2 — Add NuGet packages to the Functions project

```bash
cd src/HL7EndpointMirror.Functions

# Application Insights
dotnet add package Microsoft.ApplicationInsights.WorkerService

# Azure App Configuration (for chaos config runtime updates)
dotnet add package Microsoft.Extensions.Configuration.AzureAppConfiguration

# Azure Functions App Configuration extension
dotnet add package Microsoft.Azure.AppConfiguration.Functions.Worker

cd ../..
```
