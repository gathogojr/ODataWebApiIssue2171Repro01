using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ODataWebApiIssue2171Repro01.Data;
using ODataWebApiIssue2171Repro01.Models;
using System;
using System.Collections.Generic;
using System.Linq;


namespace ODataWebApiIssue2171Repro01
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(
                options => options.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddDbContext<MoviesDbContext>(options => options.UseInMemoryDatabase(databaseName: "Repro01MoviesDb"));
            services.AddOData();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // NOTE: UseODataBatching should appear before UseMvc
            app.UseODataBatching();

            app.UseMvc(routeBuilder =>
            {
                routeBuilder.Select().Filter().Expand().Count().OrderBy().SkipToken().MaxTop(100);
                routeBuilder.MapODataServiceRoute(
                    routeName: "odata",
                    routePrefix: "odata",
                    configureAction: containerBuilder => containerBuilder
                        .AddDefaultODataServices()
                        .AddService(Microsoft.OData.ServiceLifetime.Singleton, typeof(IEdmModel),
                            serviceProvider => GetEdmModel())
                        .AddServicePrototype(new ODataMessageReaderSettings
                        {
                            MessageQuotas = new ODataMessageQuotas
                            {
                                MaxPartsPerBatch = 256
                            }
                        })
                        .AddServicePrototype(new ODataMessageWriterSettings
                        {
                            MessageQuotas = new ODataMessageQuotas
                            {
                                MaxPartsPerBatch = 256
                            }
                        })
                        .AddService(Microsoft.OData.ServiceLifetime.Scoped, typeof(ODataBatchHandler),
                            (IServiceProvider serviceProvider) => new DefaultODataBatchHandler())
                        .AddService(Microsoft.OData.ServiceLifetime.Singleton, typeof(IEnumerable<IODataRoutingConvention>),
                            serviceProvider => ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", routeBuilder))
                );
                routeBuilder.EnableDependencyInjection();
            });

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetRequiredService<MoviesDbContext>();
                if (!db.Movies.Any())
                {
                    db.Movies.Add(new Movie { Id = 1, Name = "Movie 1 " });
                    db.SaveChanges();
                }
            }   
        }

        private IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Movie>("Movies");
            
            return builder.GetEdmModel();
        }
    }
}
