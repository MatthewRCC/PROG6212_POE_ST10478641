# RaceDay Part 2 — C# ASP.NET Core Web API

## Included
- ASP.NET Core Web API (.NET 8), EF Core Code First and SQL Server
- Cookie-based authentication; role claims for `Participant` and `Organiser`
- Password hashing with ASP.NET Core `PasswordHasher`
- Swagger endpoint documentation
- Event, category, enrolment and result endpoints
- xUnit password-hashing test and GitHub Actions build/test workflow

## Run in Visual Studio
1. Open `RaceDay.Api/RaceDay.Api.csproj` in Visual Studio 2022.
2. Confirm the connection string in `appsettings.json` suits your SQL Server/LocalDB installation.
3. In Package Manager Console run `Add-Migration InitialCreate` and `Update-Database` (select `RaceDay.Api` as the startup project), or use `dotnet ef migrations add InitialCreate --project RaceDay.Api` then `dotnet ef database update --project RaceDay.Api`.
4. Run the API and open `/swagger`.
5. Run tests using Test Explorer or `dotnet test`.

## Roles
- **Participant:** browse public events/categories, update own profile, enrol, view own results.
- **Organiser:** create/update/delete own events, manage categories for own events, view enrolments for own events, record results.

## Important
The API uses the Part 1-style entities `User`, `EventType`, `Event`, `Category`, `Enrollment`, `Result`, `Route`, and `WeatherForecast` (the C# class is `RaceRoute` to avoid conflict with ASP.NET routing). Verify fields and names against your approved Part 1 ERD before submitting. Add your own meaningful incremental commits, capture your own CI screenshot, and add the unlisted demo video URL requested in the brief. The included tests are a starter; extend them to cover the required registration/login, organiser CRUD, role denial, enrolment and result-recording scenarios.
