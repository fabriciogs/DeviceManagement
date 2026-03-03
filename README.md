# Device Management Api

This is a REST API for managing Devices, built with .NET Core 10, Clean Architecture, Dapper, and SQL Server.

## Creator
Fabricio Gabrielli da Silva

## Setup
1. Install .NET Core 10 SDK (https://dotnet.microsoft.com/en-us/download/dotnet/10.0).
2. Restore packages: `dotnet restore`.
3. Run migrations: Execute `Migrations/001_CreateDevicesTable.sql` on your SQL Server DB.
4. Update `appsettings.json` connection string.
5. Run: `dotnet run --project DeviceManagement.Api`.

## Docker
- Build and run: `docker-compose up`.

## API Documentation
- Swagger: `/swagger` when running.

## Jwt Authentication
- To generate a valid Jwt token, access endpoint `/api/auth/login` with username = `test` and password = `password01!`.

## Tests
- Run: `dotnet test`.
- Coverage: `dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover`.

## Future Improvements
- Finish authentication/authorization (validate the user credentials against a database or an identity provider).
- Handle concurrency with optimistic locking.
- Better code coverage for Notification Pattern.
- Improve Api documentation.

If issues, contact for clarification.