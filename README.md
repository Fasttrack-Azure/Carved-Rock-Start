# Carved Rock Start - Docker 
This repo contains some simple code that illustrates .NET applications targeting 
Docker containers.

## Initial Creation Notes
The project was created by using the ASP.NET Web API (.NET 5 - C#) template and including Docker Support.

It also added the `.vscode` folder to support running and debugging the project within VS Code.

No other initial changes were made for the initial commit of the repo.

## Inject ILogger to ProductsController
```
private readonly ILogger _logger;

        public ProductsController(ILogger<ProductsController> logger, IProductLogic productLogic)
        {
            _productLogic = productLogic;
            _logger = logger;
        }


        public IEnumerable<Product> GetProducts(string category = "all")
        {
            _logger.LogInformation("Starting controller action GetProducts for {category}", category);
            return _productLogic.GetProductsForCategory(category);
        }
```


## DO the same for QuickOrderController

## Validate logger injection in Domain classes
* ProductLogic.cs
* QuickOrderLogic.cd

## Validate startup.cs for dependency injection
```
services.AddScoped<IProductLogic, ProductLogic>();
services.AddScoped<IQuickOrderLogic, QuickOrderLogic>();
```
* Add debug at GetProducts method and test VS Code debugging
* Verify if the logs are visible in the debug console

## Exception Handeling - Notice there is no try catch block in the controller logic
* Navigate to the startup.cs file to validate the call to middleware for exception handling 
* Validate the ProductLogic.cs code for the validation logic that throws Application exception or a technical exception

## Add support for Serilog
* https://github.com/serilog/serilog-aspnetcore 
```
cd .\CarvedRock.Api\
dotnet add package Serilog.AspNetCore --version 5.0.0
dotnet add package Serilog.Enrichers.Environment
```

## Update the main method
```
public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting web host");
            CreateHostBuilder(args).Build().Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
```

## Code Cleanup
* Add .UseSerilog()
```
 public static IHostBuilder CreateHostBuilder(string[] args) =>
		    // http://bit.ly/aspnet-builder-defaults
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
```

* Remove Logging information from appsettings 
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

## Add log enrichment and custom property
```
 var name = typeof(Program).Assembly.GetName().Name;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Assembly", name)
                .WriteTo.Console()
                .CreateLogger();
```

## Remove dependency injection from ProductsController
```
//_logger.LogInformation("Starting controller action GetProducts for {category}");
Log.Information("Starting controller action GetProducts for {category}");
```

* Debug using VS Code to verify logs using Serilogs

## Add support for Seq
```
dotnet add package Serilog.Sinks.Seq

.WriteTo.Seq(serverUrl: "http://host.docker.internal:5341")

//Optionally, Avoid including category in the log message text for easier log search
Log.ForContext("Category", category)
    .Information("Starting controller action GetProducts");
```

## Spin up Seq container
```
docker pull datalust/seq
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
```
## Update startup.cs file to exclude health check logs and summarize HTTP requests and add diagnostic context to the log entries
```
app.UseCustomRequestLogging();
```
* Navigate to the above method to exclude health checks
* Update Program.sc to use Seq service name

## Add Docker compose.yml to the project
```
version: '3.4'

services:
  carvedrock.api:    
    build:
      context: .
      dockerfile: CarvedRock.Api/Dockerfile
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - SimpleProperty="hello-from-code-compose"
    depends_on: 
      - seq_in_dc


  seq_in_dc:    
    image: datalust/seq    
    ports:
      - '8005:80'      
    environment:
      - ACCEPT_EULA=Y 
```

## Add the UI to the carved rock project
* Validate the FootwareController
* Validate the program.cs
* Validate the startup.cs
* Validate the integrations - CarvedRockApiClient
* Validate the appsettings.json to have the compose api service name
```
{
  "CarvedRockApiUrl": "http://carvedrock.api",

  "AllowedHosts": "*"
}
```

## Update the docker-compose.yml
```
  carvedrock.app:
    build:
      context: .
      dockerfile: CarvedRock.App/Dockerfile
    ports:
      - "8081:80"
    depends_on: 
      - seq_in_dc
```
## Run compose up and test the API app with UI application and Seq