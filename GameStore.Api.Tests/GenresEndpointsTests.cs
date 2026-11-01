using System.Net;
using System.Net.Http.Json;
using GameStore.Api.Dtos;

namespace GameStore.Api.Tests;

public class GenresEndpointsTests
{
    [Fact]
    public async Task GetGenres_SeededGenresExist_ReturnsOkWithGenres()
    {
        // Arrange
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync("/genres");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var genres = await response.Content.ReadFromJsonAsync<List<GenreDto>>();

        Assert.NotNull(genres);
        Assert.Equal(4, genres.Count);

        Assert.Contains(genres, genre => genre.Name == "Fighting");
        Assert.Contains(genres, genre => genre.Name == "RPG");
        Assert.Contains(genres, genre => genre.Name == "Racing");
        Assert.Contains(genres, genre => genre.Name == "Sport");

        Assert.All(genres, genre => Assert.True(genre.Id > 0));
    }
}
