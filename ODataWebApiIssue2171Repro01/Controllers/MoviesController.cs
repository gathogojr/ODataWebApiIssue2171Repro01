using ODataWebApiIssue2171Repro01.Data;
using ODataWebApiIssue2171Repro01.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ODataWebApiIssue2171Repro01.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoviesController : ControllerBase
    {
        private MoviesDbContext _db;

        public MoviesController(MoviesDbContext db)
        {
            _db = db;
        }

        [EnableQuery]
        public IQueryable<Movie> Get()
        {
            return _db.Movies;
        }

        [EnableQuery]
        public SingleResult<Movie> Get([FromODataUri]int key)
        {
            return SingleResult.Create(_db.Movies.Where(d => d.Id.Equals(key)));
        }

        public async Task<IActionResult> Post([FromBody]Movie movie)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var movieEntity = await _db.Movies.FindAsync(movie.Id);

            if (movieEntity != null)
                return Conflict();

            _db.Movies.Add(movie);

            await _db.SaveChangesAsync();

            return Created(new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}/{movie.Id}", UriKind.Absolute), movie);
        }

        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody]Movie movie)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var movieEntity = await _db.Movies.FindAsync(key);

            if (movieEntity == null)
                return NotFound();

            movieEntity.Name = movie.Name;

            await _db.SaveChangesAsync();

            return Ok();
        }

        public async Task<IActionResult> Patch([FromODataUri] int key, [FromBody]Delta<Movie> delta)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var movieEntity = await _db.Movies.FindAsync(key);

            if (movieEntity == null)
                return NotFound();

            delta.Patch(movieEntity);

            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
