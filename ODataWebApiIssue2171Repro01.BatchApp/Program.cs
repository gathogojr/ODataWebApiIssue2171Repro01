using Default;
using ODataWebApiIssue2171Repro01.Models;
using Microsoft.OData.Client;
using System;
using System.Linq;

namespace ODataWebApiIssue2171Repro01.BatchApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceUri = new Uri("http://localhost:7443/odata");
            var context = new Container(serviceUri);

            var query = context.CreateQuery<Movie>("Movies")
                .AddQueryOption("$orderby", "Id desc")
                .AddQueryOption("$top", 1);

            var asyncResult = query.BeginExecute(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            var queryResult = query.EndExecute(asyncResult);

            var nthMovie = queryResult.FirstOrDefault();

            var n = nthMovie != null ? nthMovie.Id : 0;
            var batchSize = 101;

            for (var i = 1; i <= batchSize; i++)
            {
                var movieId = n + i;
                context.AddToMovies(new Movie { Id = movieId, Name = "Movie " + movieId });
            }

            context.SaveChangesAsync(SaveChangesOptions.BatchWithIndependentOperations).Wait();
        }
    }
}
