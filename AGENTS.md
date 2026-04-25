# AI Agent Instructions for MyMoney

MyMoney is a personal finance application built with WPF. It uses
the WPF-UI package for a WinUI 3 theme.

## Build, Test, and Lint Commands

- **Build**: `dotnet build` (from solution directory)
- **Run all tests**: `dotnet test` (from solution directory)
- **Run single test**: `dotnet test --filter "Fully.Qualified.Test.Name"`
- **Lint**: Uses built-in .editorconfig for C# formatting (no separate lint command)

## High-level Architecture

MyMoney is a WPF application using MVVM pattern with:

- **Core Layer** (`MyMoney.Core`): Domain models, database access, and business logic
  - Models: `Models/` directory contains data entities (Account, Transaction, Budget, etc.)
  - Database: `Database/` directory handles LiteDB operations via `DatabaseManager`
  - Reports: `Reports/` directory generates financial reports

- **Application Layer** (`MyMoney`): UI and presentation logic
  - ViewModels: `ViewModels/Pages/` contains page-specific view models
  - Views: WPF views in `Views/` directory
  - Services: Application services in `Services/` directory

- **Test Layer** (`MyMoney.Tests`): Unit tests for core functionality
  - Tests organized by component (DatabaseTests, ModelTests, etc.)
  - Uses MSTest framework with Moq for mocking

## Key Conventions

1. **Database Operations**: All database access goes through `DatabaseManager` interface
2. **MVVM Pattern**: ViewModels expose properties and commands for Views to bind to
5. **Testing**: Tests use in-memory database via `MemoryStream` for isolation
