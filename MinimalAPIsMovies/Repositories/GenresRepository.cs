using Dapper;
using Microsoft.Data.SqlClient;
using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Repositories;

public class GenresRepository : IGenresRepository
{
    private readonly string _connectionString;

    public GenresRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<int> Create(Genre genre)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = @"
                insert into genres (Name)
                values(@Name)
                
                select scope_identity();

            ";
            var id = await connection.QuerySingleAsync<int>(query, genre);
            genre.Id = id;
            return id;
        }
    }

    public async Task<List<Genre>> GetAll()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = @" select Id,Name from genres";
            var genres = await connection.QueryAsync<Genre>(query);
            return genres.ToList();
        }
    }

    public async Task<Genre?> GetById(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = @" select Id,Name from genres where Id = @Id";
            var genre = await connection.QueryFirstOrDefaultAsync<Genre>(query, new { id });
            return genre;
        }
    }

    public async Task<bool> Exists(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = @" if exists(select 1 from Genres where Id = @Id) select 1; else select 0;";
            var genreExists = await connection.QuerySingleAsync<bool>( query, new { id });
            return genreExists;
        }
    }

    public async Task Update(Genre genre)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = @" update Genres set Name = @Name where Id = @Id";
            var genreExists = await connection.ExecuteAsync( query, genre);
        }
    }

    public async Task Delete(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = @" delete from Genres where Id = @Id";
            await connection.ExecuteAsync( query, new{id});
        }
    }
}