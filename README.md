# GameStore

A full-stack game management application built while learning C# and ASP.NET Core. The backend provides a REST API backed by SQLite, and the React frontend lets users browse, create, edit, and delete games.

This is my first ASP.NET Core project, built by following [Julio Casal’s ASP.NET Core tutorial].

## Features

- View games with their genre, price, and release date.
- View individual game details.
- Create, update, and delete games.
- Validate incoming create and update requests.
- Store data in SQLite using Entity Framework Core.
- Apply database migrations and seed initial genres at application startup.

## Tech Stack

| Layer             | Technologies                           |
| ----------------- | -------------------------------------- |
| Backend           | C#, .NET 10, ASP.NET Core Minimal APIs |
| Database          | SQLite, Entity Framework Core 10       |
| Frontend          | React 19, TypeScript, Vite 7           |
| UI and navigation | Bootstrap 5, React Router              |
| API testing       | Postman                                |

## Project Structure

```text
GameStore/
├── GameStore.Api/
│   ├── Data/
│   │   ├── Migrations/
│   │   ├── DataExtensions.cs
│   │   └── GameStoreContext.cs
│   ├── Dtos/
│   ├── EndPoints/
│   ├── Models/
│   ├── Properties/
│   ├── GameStore.Api.csproj
│   ├── Program.cs
│   └── dotnet-tools.json
├── GameStore.React/
│   ├── src/
│   │   ├── clients/
│   │   ├── components/
│   │   ├── models/
│   │   └── pages/
│   ├── package.json
│   ├── package-lock.json
│   └── vite.config.ts
└── README.md
```

## Getting Started

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

- A nonempty name of at most 50 characters.
- A genre ID between 1 and 50.
- A price between 1 and 100.
- A release date that can be parsed as a date.

The genre ID range check does not verify that the genre exists; the database relationship also requires a matching genre.

A successful POST returns **201 Created**, the created game, and a `Location` header. Successful PUT and DELETE requests return **204 No Content**.

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

## Acknowledgments

- [Julio Casal’s ASP.NET Core tutorial](https://www.youtube.com/watch?v=YbRe4iIVYJk) for the backend walkthrough.
- This repository documents my implementation and learning from the course.
