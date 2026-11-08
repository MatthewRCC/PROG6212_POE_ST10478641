# RaceDay additional code

These files are additions for the RaceDay starter ZIP you uploaded.

## Folders and files

- `Controllers/EventTypesController.cs` — public read-only event type endpoints.
- `Controllers/RoutesController.cs` — public route viewing and organiser-only route management.
- `Controllers/WeatherForecastsController.cs` — public forecast viewing and organiser-only forecast management.
- `Database/RaceDayDatabase.sql` — SQL Server script to add the Routes and WeatherForecasts tables and seed event types.

## Where to put the files

Copy the `Controllers` folder contents into:

`RaceDay.Api/Controllers/`

Copy the `Database` folder into the root of your solution, alongside `RaceDay.Api` and `RaceDay.Tests`.

The new controller files use the existing global model/database types from your uploaded `Program.cs`:
`RaceDayDb`, `UserRole`, `RaceEvent`, `RaceRoute`, and `WeatherForecast`.
Do not create duplicate copies of those classes.

## SQL instructions

The uploaded starter project did not include the full database creation script. The SQL file here is an ADD-ON script, not a complete replacement for the database schema. It expects the base tables `Events` and `EventTypes` to exist first.

If you are creating the database from scratch, use EF Core migrations from the `RaceDay.Api` project instead:

1. Open Visual Studio.
2. Select Tools > NuGet Package Manager > Package Manager Console.
3. Set the Default project to `RaceDay.Api`.
4. Run `Add-Migration InitialCreate`.
5. Run `Update-Database`.

Then run the add-on SQL script in SQL Server Management Studio if the Routes and WeatherForecasts tables were not created by the migration.

## New API endpoints

- `GET /api/event-types`
- `GET /api/event-types/{id}`
- `GET /api/events/{eventId}/routes`
- `POST /api/events/{eventId}/routes` (organiser)
- `PUT /api/events/{eventId}/routes/{routeId}` (owning organiser)
- `DELETE /api/events/{eventId}/routes/{routeId}` (owning organiser)
- `GET /api/events/{eventId}/weather`
- `POST /api/events/{eventId}/weather` (organiser)
- `PUT /api/events/{eventId}/weather/{forecastId}` (owning organiser)
- `DELETE /api/events/{eventId}/weather/{forecastId}` (owning organiser)

## Important

The SQL add-on assumes the starter database's table and column names match the entity conventions in the uploaded project. Check your actual database before running it. Back up any existing database first.

The GitHub repository link could not be read in this session, so these additions are based on the RaceDay starter ZIP you uploaded, not a verified comparison against the GitHub repository.
