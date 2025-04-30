# [Softwarekueche](https://softwarekueche.de)

Projekt mit einem einfach Blog für die Homepage von `Softwareküche - Für jedes Vorhaben das richtige Rezept`.

Die Seite enthält einen kleine Blog mit einer einfachen Bilderverwaltung.

## Technik

Das Project basiert auf einer klassischen ASP.NET Razor Seite mit einem Entity Framework Datenbanklayer.

## Docker Support

The blog can be run in a Docker container. When you map the "/data" folder to a local folder, the database will be stored there. The database is created automatically when the container starts.

    docker run -p 8080:8080 -v .\blogengine\data:/data thomasley/onkelmatoblogengineweb

## Developer Settings

    export ASPNETCORE_ENVIRONMENT=Development

## DB Support

Migrations for each RDBMS

    dotnet ef migrations add InitialCreate --context SqlServerBlogEngineContext --output-dir Migrations/SqlServerMigrations

    dotnet ef migrations add InitialCreate --context SqliteBlogEngineContext --output-dir Migrations/SqliteMigrations
    

