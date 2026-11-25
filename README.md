# QBuild BOM Viewer

## Overview

QBuild BOM Viewer is a web application for exploring a Bill of Materials (BOM). It loads BOM and part information from CSV files, builds a hierarchical component tree, and displays the direct components of the selected part.

## Features

- Load BOM and PART data from CSV files.
- Build and display a hierarchical BOM tree.
- Display direct components of a selected part.
- Match BOM records with PART information.
- Handle components with missing PART records.
- Detect and skip circular references.
- Validate BOM structure and hierarchy.
- Keep loaded data in memory for reuse.

## Tech Stack

**Backend:** C#, .NET 10, ASP.NET Core Web API, CsvHelper

**Frontend:** React, JavaScript, Vite, CSS

**Testing:** xUnit

**Infrastructure:** Docker, Docker Compose

## Project Structure

```text
QBuildChallenge/
├── Backend/
│   ├── QBuild.API/
│   │   ├── Controllers/
│   │   ├── Data/
│   │   │   ├── bom.csv
│   │   │   └── part.csv
│   │   ├── Properties/
│   │   ├── DataDirectoryResolver.cs
│   │   ├── DockerFile
│   │   └── Program.cs
│   ├── QBuild.Business/
│   ├── QBuild.DataAccess/
│   └── QBuild.Models/
├── Frontend/
├── QBuild.Tests/
└── docker-compose.yml
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js and npm
- Docker Desktop (for Docker deployment)

### Run Backend

From the solution root:

```bash
dotnet run --project Backend/QBuild.API
```

### Run Frontend

```bash
cd Frontend
npm install
npm run dev
```

The frontend runs on port `3000` and proxies API requests to `http://localhost:8080`.

## Run with Docker

From the solution root:

```bash
docker compose up --build -d
```

The Docker Compose configuration starts the backend API on port `8080`.

To stop the application:

```bash
docker compose down
```

## Run Tests

From the solution root:

```bash
dotnet test
```

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/bom/tree` | Retrieves the BOM hierarchy |
| GET | `/api/bom/components/{parentName}` | Retrieves direct components of a selected part |

## Notes

- CSV data is loaded into memory during application initialization.
- Entity Framework and a database are not used.
- The frontend and backend can be run independently.