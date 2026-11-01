# GameStore

A full-stack game management application built while learning C# and ASP.NET Core. The backend provides a REST API backed by SQLite, and the React frontend lets users browse, create, edit, and delete games.

This is my first ASP.NET Core project, built by following Julio Casal’s ASP.NET Core tutorial.

## Features

- View games with their genre, price, and release date.
- View individual game details.
- Create, update, and delete games.
- Validate incoming create and update requests.
- Store data in SQLite using Entity Framework Core.
- Apply pending database migrations at startup and seed initial genres and games when their respective tables are empty.

## Tech Stack

| Layer                      | Technologies                                   |
| -------------------------- | ---------------------------------------------- |
| Backend                    | C#, .NET 10, ASP.NET Core Minimal APIs         |
| Database                   | SQLite, Entity Framework Core 10               |
| Frontend                   | React 19, TypeScript, Vite 7                   |
| UI and navigation          | Bootstrap 5, React Router                      |
| Automated API testing      | xUnit, WebApplicationFactory, SQLite in memory |
| Manual API testing         | Postman                                        |
| Containers and web serving | Docker, Docker Compose, NGINX                  |

## Project Structure

```text
GameStore/
├── GameStore.slnx
├── compose.yaml
├── GameStore.Api/
│   ├── Data/
│   │   ├── Migrations/
│   │   ├── DataExtensions.cs
│   │   └── GameStoreContext.cs
│   ├── Dtos/
│   ├── EndPoints/
│   ├── Models/
│   ├── Properties/
│   ├── .dockerignore
│   ├── Dockerfile
│   ├── GameStore.Api.csproj
│   ├── Program.cs
│   └── dotnet-tools.json
├── GameStore.Api.Tests/
│   ├── GameStore.Api.Tests.csproj
│   ├── GameStoreApiFactory.cs
│   ├── GamesEndpointsTests.cs
│   └── GenresEndpointsTests.cs
├── GameStore.React/
│   ├── src/
│   │   ├── clients/
│   │   ├── components/
│   │   ├── models/
│   │   └── pages/
│   ├── .dockerignore
│   ├── Dockerfile
│   ├── nginx.conf
│   ├── package.json
│   ├── package-lock.json
│   └── vite.config.ts
└── README.md
```

## Run with Docker

### Prerequisites

- Docker Desktop running with Linux containers enabled.
- Git, if cloning the repository.

The Docker build installs the required .NET and Node.js dependencies inside
build containers. You do not need to install those tools locally for this option.

### First-Time Setup

Clone the repository and enter its root folder:

```bash
git clone https://github.com/benztranwot/Game-Store.git
cd Game-Store
```

Create the external volume used to store the SQLite database:

```bash
docker volume create gamestore-data
```

Build the images and start both services:

```bash
docker compose up -d --build
```

If you already have the repository locally, run these Docker commands from
the folder containing compose.yaml.

### Open the Application

| Address                         | Purpose                        |
| ------------------------------- | ------------------------------ |
| http://localhost:8080           | React frontend served by NGINX |
| http://localhost:8080/api/games | Games API through NGINX        |
| http://localhost:5205/games     | Direct access to the games API |

NGINX serves the built frontend and forwards `/api/` requests to the API
container, removing the `/api` prefix.

The API applies pending database migrations at startup. Initial genres and
games are inserted when their respective tables are empty.

### Database Persistence

SQLite is stored at `/data/GameStore.db` inside the API container.
That directory is mounted from the external Docker volume `gamestore-data`.

The Docker database is separate from the database used when running the API
directly on your computer.

Stopping or replacing containers preserves the volume and its data.
Deleting the volume deletes that database.

### Useful Commands

Check service status:

```bash
docker compose ps
```

Follow application logs:

```bash
docker compose logs -f
```

Press Ctrl+C to leave the log viewer. Background containers keep running.

After changing application code, rebuild and update the containers:

```bash
docker compose up -d --build
```

Stop and remove the containers and Compose network:

```bash
docker compose down
```

The external database volume remains.

This setup serves production builds and does not provide automatic hot reload.
Use the local development instructions below when you want the development servers.

## Local Development Without Docker

If the Docker version is running, stop it before starting the local backend,
because both use port 5205:

```bash
docker compose down
```

Run this command from the repository root. The database volume is preserved.

### Prerequisites

- .NET 10 SDK
- Node.js compatible with Vite 7, such as Node.js 24
- npm

Run the backend and frontend in separate terminals.

### Run the Backend

From the repository root:

```bash
cd GameStore.Api
dotnet tool restore
dotnet restore
dotnet watch run
```

The HTTP development profile serves the API at:

```text
http://localhost:5205
```

At startup, the application applies pending EF Core migrations and runs the configured database seeding. The local database is excluded from Git.

If a code change requires an application restart, press **Ctrl+R** in the terminal running `dotnet watch`.

### Run the Frontend

In another terminal, starting from the repository root:

```bash
cd GameStore.React
npm ci
npm run dev
```

Open the local URL printed by Vite, usually:

```text
http://localhost:5173
```

The Vite development proxy forwards `/api` requests to `http://localhost:5205` and removes the `/api` prefix.

For example:

```text
Frontend request: /api/games
Backend endpoint: /games
```

## API Endpoints

| Method | Endpoint      | Description                               |
| ------ | ------------- | ----------------------------------------- |
| GET    | `/games`      | Get game summaries, including genre names |
| GET    | `/games/{id}` | Get an individual game's details          |
| POST   | `/games`      | Create a game                             |
| PUT    | `/games/{id}` | Update a game                             |
| DELETE | `/games/{id}` | Delete a game                             |
| GET    | `/genres`     | Get available genres                      |

### Create or Update a Game

For POST and PUT requests, use `Content-Type: application/json` and a body such as:

```json
{
  "name": "Example Game",
  "genreId": 2,
  "price": 29.99,
  "releaseDate": "2026-10-28"
}
```

Use an existing genre ID from `GET /genres`.

Input validation requires:

- A name containing non-whitespace characters, with a maximum length of 50.
- A genre ID between 1 and 50 that references an existing genre.
- A decimal price between 1 and 100, inclusive.
- A release date that can be parsed as a date.

POST and PUT requests with a nonexistent genre return **400 Bad Request**
with a field-specific validation error.

A successful POST returns **201 Created**, the created game, and a
`Location` header. Successful PUT and DELETE requests return
**204 No Content**.

GET, PUT, and DELETE requests targeting a nonexistent game return
**404 Not Found**. GET `/games` returns **200 OK** with an empty array
when no games exist.

## Automated Tests

The API integration tests use xUnit and WebApplicationFactory to exercise
the ASP.NET Core request pipeline against SQLite databases held in memory.

### Run the Tests

Install the .NET 10 SDK, then run this command from the repository root:

```bash
dotnet test GameStore.slnx --configuration Release
```

The command restores dependencies, builds the solution, and runs the tests.

Docker, Node.js, and a separately running API are not required. Each test
creates a fresh database, applies migrations, and runs the application's
seed logic. The tests do not modify the local or Docker database.

### Coverage

- Retrieving game lists, individual games, and genres.
- Creating, updating, and deleting games.
- Returning appropriate responses for missing games.
- Rejecting blank names and out-of-range prices.
- Accepting prices at the allowed boundaries.
- Rejecting nonexistent genres.
- Preserving existing game data after a rejected update.
- Retrieving created and updated games to verify persistence.

## What I Learned

- Registering and grouping Minimal API endpoints.
- Separating request and response DTOs from database entities.
- Using dependency injection to supply a database context.
- Defining entity relationships with EF Core.
- Creating and applying database migrations.
- Seeding initial database data.
- Performing asynchronous database operations.
- Connecting a React frontend to an ASP.NET Core API.
- Testing HTTP requests with Postman.
- Building backend and frontend images with multi-stage Dockerfiles.
- Running the application with Docker Compose.
- Preserving SQLite data using a named Docker volume.
- Serving React files and proxying API requests with NGINX.
- Writing API integration tests with xUnit and WebApplicationFactory.
- Structuring tests with Arrange, Act, and Assert.
- Using theories to test multiple input cases.
- Isolating tests with fresh SQLite databases.
- Detecting regressions and improving validation through failing tests.

## Acknowledgments

- [Julio Casal’s ASP.NET Core tutorial](https://www.youtube.com/watch?v=YbRe4iIVYJk) for the backend walkthrough.
- [Julio Casal’s C# unit testing tutorial](https://www.youtube.com/watch?v=MpeMJ0XqgYg) for test naming, Arrange–Act–Assert, and regression-testing concepts.
- This repository documents my implementation and learning from the course.
