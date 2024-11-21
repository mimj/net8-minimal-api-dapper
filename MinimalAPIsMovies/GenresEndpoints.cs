using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Repositories;

namespace MinimalAPIsMovies;

public static class GenresEndpoints
{
    public static RouteGroupBuilder MapGenres(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetGenres).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("genres-get"));
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", CreateGenre);
        group.MapPut("/{id:int}", UpdateGenre);
        group.MapDelete("/{id:int}", DeleteGenre);
        return group;
    }


    static async Task<Ok<List<GenreDto>>> GetGenres(IGenresRepository repository, IMapper mapper)
    {
        var genres = await repository.GetAll();
        var genresDto = mapper.Map<List<GenreDto>>(genres);
        // var genresDtos = genres.Select(x => new GenreDto { Id = x.Id, Name = x.Name }).ToList();
        return TypedResults.Ok(genresDto);
    }


    static async Task<Results<Ok<GenreDto>, NotFound>> GetById(int id, IGenresRepository repository)
    {
        var genre = await repository.GetById(id: id);
        if (genre is null)
        {
            return TypedResults.NotFound();
        }

        var genreDto = new GenreDto()
        {
            Id = genre.Id,
            Name = genre.Name
        };

        return TypedResults.Ok(genreDto);
    }

    static async Task<Created<GenreDto>> CreateGenre(CreateGenreDto createGenreDto, IGenresRepository genresRepository,
        IOutputCacheStore outputCacheStore, IMapper mapper)
    {
        var genre = mapper.Map<Genre>(createGenreDto);
        // var genre = new Genre()
        // {
        //     Name = genreDto.Name
        // };
        var id = await genresRepository.Create(genre);
        await outputCacheStore.EvictByTagAsync("genres-get", default);

        var genreDto = mapper.Map<GenreDto>(genre);
        return TypedResults.Created($"/genres/{id}", genreDto);
    }

    static async Task<Results<NotFound, NoContent>> UpdateGenre(int id, CreateGenreDto createGenreDto,
        IGenresRepository repository,
        IOutputCacheStore cacheStore,
        IMapper mapper)
    {
        var exists = await repository.Exists(id);
        if (!exists)
        {
            return TypedResults.NotFound();
        }

        var genre = mapper.Map<Genre>(createGenreDto);
        genre.Id = id;

        await repository.Update(genre);
        await cacheStore.EvictByTagAsync("genres-get", default);
        return TypedResults.NoContent();
    }

    static async Task<IResult> DeleteGenre(int id, IGenresRepository repository, IOutputCacheStore cacheStore)
    {
        var exists = await repository.Exists(id);
        if (!exists)
        {
            return Results.NotFound();
        }

        await repository.Delete(id);
        await cacheStore.EvictByTagAsync("genres-get", default);
        return Results.NoContent();
    }
}