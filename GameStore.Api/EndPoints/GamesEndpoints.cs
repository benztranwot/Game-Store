using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.EndPoints;

public static class GamesEndpoints
{
    const string GetGameEndpointName = "GetGame";

    public static void MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games");

        // GET /games
        group.MapGet(
            "/",
            async (GameStoreContext dbContext) =>
                await dbContext
                    .Games.Include(game => game.Genre)
                    .Select(game => new GameSummaryDto(
                        game.Id,
                        game.Name,
                        game.Genre!.Name,
                        game.Price,
                        game.ReleaseDate
                    ))
                    .AsNoTracking()
                    .ToListAsync()
        );

        // GET /games/id
        group
            .MapGet(
                "/{id}",
                async (int id, GameStoreContext dbContext) =>
                {
                    var game = await dbContext.Games.FindAsync(id);

                    return game is null
                        ? Results.NotFound()
                        : Results.Ok(
                            new GameDetailsDto(
                                game.Id,
                                game.Name,
                                game.GenreID,
                                game.Price,
                                game.ReleaseDate
                            )
                        );
                }
            )
            .WithName(GetGameEndpointName);

        // POST /games
        group.MapPost(
            "/",
            async (CreateGameDto newGame, GameStoreContext dbContext) =>
            {
                var genreExists = await dbContext.Genres.AnyAsync(genre =>
                    genre.Id == newGame.GenreId
                );

                if (!genreExists)
                {
                    return Results.ValidationProblem(
                        new Dictionary<string, string[]>
                        {
                            ["GenreId"] = ["The selected genre does not exist."],
                        }
                    );
                }

                Game game = new()
                {
                    Name = newGame.Name,
                    GenreID = newGame.GenreId,
                    Price = newGame.Price,
                    ReleaseDate = newGame.ReleaseDate,
                };

                dbContext.Games.Add(game);
                await dbContext.SaveChangesAsync();

                GameDetailsDto gameDetailsDto = new(
                    game.Id,
                    game.Name,
                    game.GenreID,
                    game.Price,
                    game.ReleaseDate
                );

                return Results.CreatedAtRoute(
                    GetGameEndpointName,
                    new { id = gameDetailsDto.Id },
                    gameDetailsDto
                );
            }
        );

        // PUT /games/id
        group.MapPut(
            "/{id}",
            async (int id, UpdateGameDto updatedGame, GameStoreContext dbContext) =>
            {
                var existingGame = await dbContext.Games.FindAsync(id);

                if (existingGame is null)
                    return Results.NotFound();

                var genreExists = await dbContext.Genres.AnyAsync(genre =>
                    genre.Id == updatedGame.GenreId
                );

                if (!genreExists)
                {
                    return Results.ValidationProblem(
                        new Dictionary<string, string[]>
                        {
                            ["GenreId"] = ["The selected genre does not exist."],
                        }
                    );
                }

                existingGame.Name = updatedGame.Name;
                existingGame.GenreID = updatedGame.GenreId;
                existingGame.Price = updatedGame.Price;
                existingGame.ReleaseDate = updatedGame.ReleaseDate;

                await dbContext.SaveChangesAsync();

                return Results.NoContent();
            }
        );

        // DELETE /games/id
        group.MapDelete(
            "/{id}",
            async (int id, GameStoreContext dbContext) =>
            {
                var deletedCount = await dbContext
                    .Games.Where(game => game.Id == id)
                    .ExecuteDeleteAsync();

                return deletedCount == 0 ? Results.NotFound() : Results.NoContent();
            }
        );
    }
}
