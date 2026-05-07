# How to run the project
First, clone the repo to your computer

# Setup the local database
1. Make sure you have dotnet-ef installed (you can install it with "dotnet tool install --global dotnet-ef")
2. Make sure you change your connection string:
  - Go to PussyCats.Api/appsettings.json/appsettings.Development.json
  - Under "ConnectionStrings", on "PussyCatsDb", change only the "Server=Server_name" part
  - Replace Server_Name with your actual SqlServer connection
  - Example: 
    "ConnectionStrings": {
    "PussyCatsDb": "Server=EMANUEL\\SQLEXPRESS;Database=ISS-921-1;Trusted_Connection=True;TrustServerCertificate=True;"
}

2. In Visual Studio, go to Developer Powershell:
  - You should see a path ending in \UBB-SE-2026-921-1
  - Go to the PussyCats.Api folder: "cd PussyCats.Api"
  - Run the following command: "dotnet ef database update"
  - If you now check in SSMS you should have a new database called ISS-921-1

# Run the project
To run the project you need to do two things, first start the api, then start the app.
In order for the api to work, you may need to run this command the first time (not everytime): "dotnet dev-certs https --trust"

1. Start the api:
  - Make sure you're still in PussyCats.Api
  - Run this command: "dotnet run --launch-profile https"
  - If everything goes right, you should see something like this:
    Building...
    info: Microsoft.Hosting.Lifetime[14]
          Now listening on: https://localhost:7134
    info: Microsoft.Hosting.Lifetime[14]
          Now listening on: http://localhost:5041
    info: Microsoft.Hosting.Lifetime[0]
          Application started. Press Ctrl+C to shut down.
    info: Microsoft.Hosting.Lifetime[0]
          Hosting environment: Development

2. Start the app:
  - From where you normally run the app, make sure you have PussyCats.App selected and click on run

To test if your api works, go on the browser and type: https://localhost:7134/api/companies/1 and you should see a json with a company.