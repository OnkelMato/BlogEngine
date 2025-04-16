# [Softwarekueche](https://softwarekueche.de)

Projekt mit einem einfach Blog für die Homepage von `Softwareküche - Für jedes Vorhaben das richtige Rezept`.

Die Seite enthält einen kleine Blog mit einer einfachen Bilderverwaltung.

## Technik

Das Project basiert auf einer klassischen ASP.NET Razor Seite mit einem Entity Framework Datenbanklayer.

## Developer Settings

    export ASPNETCORE_ENVIRONMENT=Development

## DB Support

Migrations for each RDBMS

    dotnet ef migrations add InitialCreate --context SqlServerBlogEngineContext --output-dir Migrations/SqlServerMigrations

    dotnet ef migrations add InitialCreate --context SqliteBlogEngineContext --output-dir Migrations/SqliteMigrations
    

