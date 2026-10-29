using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data;

public static class DataExtensions
{
    public static void MigrateDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameStoreContext>();

        dbContext.Database.Migrate();
    }

    public static void AddGameStoreDb(this WebApplicationBuilder builder)
    {
        var connString = builder.Configuration.GetConnectionString("GameStore");
        builder.Services.AddScoped<GameStoreContext>();
        builder.Services.AddSqlite<GameStoreContext>(
            connString,
            optionsAction: options =>
                options.UseSeeding(
                    (context, _) =>
                    {
                        if (!context.Set<Genre>().Any())
                        {
                            context
                                .Set<Genre>()
                                .AddRange(
                                    new Genre { Name = "Fighting" },
                                    new Genre { Name = "RPG" },
                                    new Genre { Name = "Racing" },
                                    new Genre { Name = "Sport" }
                                );

                            context.SaveChanges();
                        }
                        if (!context.Set<Game>().Any())
                        {
                            var fightingGenre = context
                                .Set<Genre>()
                                .Single(genre => genre.Name == "Fighting");

                            var rpgGenre = context
                                .Set<Genre>()
                                .Single(genre => genre.Name == "RPG");

                            var racingGenre = context
                                .Set<Genre>()
                                .Single(genre => genre.Name == "Racing");

                            context
                                .Set<Game>()
                                .AddRange(
                                    new Game
                                    {
                                        Name = "Street Fighter 2",
                                        GenreID = fightingGenre.Id,
                                        Price = 19.99m,
                                        ReleaseDate = new DateOnly(1992, 7, 15),
                                    },
                                    new Game
                                    {
                                        Name = "Elden Ring",
                                        GenreID = rpgGenre.Id,
                                        Price = 39.99m,
                                        ReleaseDate = new DateOnly(2022, 2, 25),
                                    },
                                    new Game
                                    {
                                        Name = "Forza Horizon 4",
                                        GenreID = racingGenre.Id,
                                        Price = 29.99m,
                                        ReleaseDate = new DateOnly(2018, 10, 2),
                                    }
                                );

                            context.SaveChanges();
                        }
                    }
                )
        );
    }
}
