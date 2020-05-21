using ODataWebApiIssue2171Repro01.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace ODataWebApiIssue2171Repro01.Data
{
    public class MoviesDbContext : DbContext
    {
        public MoviesDbContext(DbContextOptions<MoviesDbContext> options) : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
    }
}
