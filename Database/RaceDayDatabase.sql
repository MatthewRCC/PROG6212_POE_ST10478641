/*
    RaceDay Part 2 database
    SQL Server / SQL Server Management Studio

    This script matches the entity names and properties in the uploaded
    RaceDay.Api starter project. Run it once on a new/local SQL Server database.
*/
IF DB_ID(N'RaceDayPart2') IS NULL
    CREATE DATABASE RaceDayPart2;
GO

USE RaceDayPart2;
GO

IF OBJECT_ID(N'dbo.WeatherForecasts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WeatherForecasts
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WeatherForecasts PRIMARY KEY,
        EventId INT NOT NULL,
        ForecastFor DATETIME2 NOT NULL,
        TemperatureC DECIMAL(8,2) NULL,
        Summary NVARCHAR(MAX) NULL,
        CONSTRAINT FK_WeatherForecasts_Events
            FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.Routes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Routes
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Routes PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        DistanceKm DECIMAL(10,2) NOT NULL,
        EventId INT NOT NULL,
        CONSTRAINT FK_Routes_Events
            FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE,
        CONSTRAINT CK_Routes_Distance CHECK (DistanceKm > 0)
    );
END;
GO

/* Add the event type seed data only when the table is empty. */
IF OBJECT_ID(N'dbo.EventTypes', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.EventTypes)
BEGIN
    INSERT INTO dbo.EventTypes (Name, Description)
    VALUES
      (N'Road Race', N'Running event held on paved roads.'),
      (N'Trail Run', N'Running event held on natural trails.'),
      (N'Cycling', N'Organised cycling event.'),
      (N'Marathon', N'Long-distance running event.');
END;
GO

SELECT DB_NAME() AS CurrentDatabase;
SELECT * FROM dbo.EventTypes;
GO
