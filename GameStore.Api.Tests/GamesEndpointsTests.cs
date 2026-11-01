using System.Net;
using System.Net.Http.Json;
using GameStore.Api.Dtos;

namespace GameStore.Api.Tests;

public class GamesEndpointsTests
{
    [Fact]
    public async Task GetGame_GameDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync("/games/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetGame_GameExists_ReturnsOkWithGame()
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync("/games/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var game = await response.Content.ReadFromJsonAsync<GameDetailsDto>();

        Assert.NotNull(game);
        Assert.Equal(1, game.Id);
        Assert.Equal("Street Fighter 2", game.Name);
        Assert.Equal(19.99m, game.Price);
    }

    [Fact]
    public async Task CreateGame_ValidGame_ReturnsCreatedAndSavesGame()
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var newGame = new CreateGameDto(
            Name: "Tekken 8",
            GenreId: 1,
            Price: 59.99m,
            ReleaseDate: new DateOnly(2024, 1, 26)
        );

        // Act
        using var response = await client.PostAsJsonAsync("/games", newGame);

        // Assert: check the creation response
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdGame = await response.Content.ReadFromJsonAsync<GameDetailsDto>();

        Assert.NotNull(createdGame);
        Assert.True(createdGame.Id > 0);
        Assert.Equal(newGame.Name, createdGame.Name);
        Assert.Equal(newGame.GenreId, createdGame.GenreId);
        Assert.Equal(newGame.Price, createdGame.Price);
        Assert.Equal(newGame.ReleaseDate, createdGame.ReleaseDate);

        // Retrieve the game using the Location header
        var location = response.Headers.Location;
        Assert.NotNull(location);

        using var getResponse = await client.GetAsync(location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var savedGame = await getResponse.Content.ReadFromJsonAsync<GameDetailsDto>();

        Assert.NotNull(savedGame);
        Assert.Equal(createdGame, savedGame);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("0.99")]
    [InlineData("100.01")]
    [InlineData("101")]
    public async Task CreateGame_PriceOutsideAllowedRange_ReturnsBadRequest(string invalidPrice)
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var newGame = new CreateGameDto(
            Name: "Tekken 8",
            GenreId: 1,
            Price: decimal.Parse(invalidPrice, System.Globalization.CultureInfo.InvariantCulture),
            ReleaseDate: new DateOnly(2024, 1, 26)
        );

        // Act
        using var response = await client.PostAsJsonAsync("/games", newGame);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGame_GameExists_ReturnsNoContentAndSavesChanges()
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var updatedGame = new UpdateGameDto(
            Name: "Updated Game",
            GenreId: 2,
            Price: 49.99m,
            ReleaseDate: new DateOnly(2025, 1, 1)
        );

        // Act
        using var response = await client.PutAsJsonAsync("/games/1", updatedGame);

        // Assert: check the update response
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Retrieve the game to verify the saved changes
        using var getResponse = await client.GetAsync("/games/1");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var savedGame = await getResponse.Content.ReadFromJsonAsync<GameDetailsDto>();

        var expectedGame = new GameDetailsDto(
            Id: 1,
            Name: updatedGame.Name,
            GenreId: updatedGame.GenreId,
            Price: updatedGame.Price,
            ReleaseDate: updatedGame.ReleaseDate
        );

        Assert.NotNull(savedGame);
        Assert.Equal(expectedGame, savedGame);
    }

    [Fact]
    public async Task UpdateGame_GameDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var updatedGame = new UpdateGameDto(
            Name: "Updated Game",
            GenreId: 2,
            Price: 49.99m,
            ReleaseDate: new DateOnly(2025, 1, 1)
        );

        // Act
        using var response = await client.PutAsJsonAsync("/games/999999", updatedGame);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteGame_GameExists_ReturnsNoContentAndRemovesGame()
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        // Act
        using var response = await client.DeleteAsync("/games/1");

        // Assert: check the deletion response
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the game can no longer be retrieved
        using var getResponse = await client.GetAsync("/games/1");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteGame_GameDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        // Act
        using var response = await client.DeleteAsync("/games/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetGames_SeededGamesExist_ReturnsOkWithGameSummaries()
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync("/games");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var games = await response.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal(3, games.Count);

        Assert.Contains(games, game => game.Name == "Street Fighter 2" && game.Genre == "Fighting");

        Assert.Contains(games, game => game.Name == "Elden Ring" && game.Genre == "RPG");

        Assert.Contains(games, game => game.Name == "Forza Horizon 4" && game.Genre == "Racing");
    }

    [Fact]
    public async Task CreateGame_GenreDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var newGame = new CreateGameDto(
            Name: "Tekken 8",
            GenreId: 50,
            Price: 59.99m,
            ReleaseDate: new DateOnly(2024, 1, 26)
        );

        // Act
        using var response = await client.PostAsJsonAsync("/games", newGame);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGame_GenreDoesNotExist_ReturnsBadRequestAndKeepsGameUnchanged()
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var originalGame = await client.GetFromJsonAsync<GameDetailsDto>("/games/1");
        Assert.NotNull(originalGame);

        var updatedGame = new UpdateGameDto(
            Name: "Should Not Be Saved",
            GenreId: 50,
            Price: 49.99m,
            ReleaseDate: new DateOnly(2025, 1, 1)
        );

        // Act
        using var response = await client.PutAsJsonAsync("/games/1", updatedGame);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var savedGame = await client.GetFromJsonAsync<GameDetailsDto>("/games/1");

        Assert.NotNull(savedGame);
        Assert.Equal(originalGame, savedGame);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task CreateGame_PriceAtAllowedBoundary_ReturnsCreated(int price)
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var newGame = new CreateGameDto(
            Name: "Boundary Price Game",
            GenreId: 1,
            Price: price,
            ReleaseDate: new DateOnly(2025, 1, 1)
        );

        // Act
        using var response = await client.PostAsJsonAsync("/games", newGame);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdGame = await response.Content.ReadFromJsonAsync<GameDetailsDto>();

        Assert.NotNull(createdGame);
        Assert.Equal((decimal)price, createdGame.Price);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateGame_NameIsBlank_ReturnsBadRequest(string name)
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var newGame = new CreateGameDto(
            Name: name,
            GenreId: 1,
            Price: 29.99m,
            ReleaseDate: new DateOnly(2025, 1, 1)
        );

        // Act
        using var response = await client.PostAsJsonAsync("/games", newGame);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
