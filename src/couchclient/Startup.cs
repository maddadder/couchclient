using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using couchclient.Models;
using couchclient.Services;

namespace couchclient
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

         /// <summary>
        /// dev origins used to fix CORS for local dev/qa debugging of site
        /// </summary>
        private readonly string _devSpecificOriginsName = "_devAllowSpecificOrigins";

        public Startup(
            IConfiguration configuration, 
            IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //05/25/2021
            //fix for debugging with DEV and QA environments in GitPod
            //DO NOT APPLY to UAT and PROD environments!!!
            //https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-3.1
            //
            services.AddCors(options =>
            {
                options.AddPolicy(name: _devSpecificOriginsName,
                                  builder =>
                                  {
                                      builder.WithOrigins(
                                                          "http://localhost:5000",
                                                          "https://localhost:5001")
                                                          .AllowAnyHeader()
                                                          .AllowAnyMethod()
                                                          .AllowCredentials();
                                  });
            });

	        //read in configuration to connect to the database
	        services.Configure<CouchbaseConfig>(Configuration.GetSection("Couchbase"));

	        //register the configuration 
	        services.AddCouchbase(Configuration.GetSection("Couchbase"));
	        services.AddHttpClient();

	        //register the service to handle bucket, collection, scope, and index creation
            services.AddTransient<DatabaseService>();

            services.AddControllers();

	        //customize Swagger UI
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
		            Title = "Couchbase Quickstart API", 
		            Version = "v1" 
		        });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env, 
            IHostApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseForwardedHeaders();
                app.UseDeveloperExceptionPage();

                //add cors policy
                 app.UseCors(_devSpecificOriginsName);

		        //setup swagger for debugging and testing APIs
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint(
		                "/swagger/v1/swagger.json", 
		                "Couchbase Quickstart API v1"); 
                    c.RoutePrefix = string.Empty;
                    });
            }
            else
            {
                app.UseForwardedHeaders();

                // remove this when go live begin
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint(
		                "/swagger/v1/swagger.json", 
		                "Couchbase Quickstart API v1"); 
                    c.RoutePrefix = string.Empty;
                    });
                // remove this when go live end
            }

	        if (_env.EnvironmentName == "Testing")
            {
                //add cors policy
                 app.UseCors(_devSpecificOriginsName);

	            //setup the database once everything is setup and running integration tests need to make sure database is fully working before running,hence running Synchronously
	            appLifetime.ApplicationStarted.Register(() => {
		            var db = app.ApplicationServices.GetService<DatabaseService>();
		            db.SetupDatabase().RunSynchronously();
	            });
		    } else {
	            //setup the database once everything is setup and running
	            appLifetime.ApplicationStarted.Register(async () => {
		            var db = app.ApplicationServices.GetService<DatabaseService>();
		            await db.SetupDatabase();
	            });
		    }

            //remove couchbase from memory when ASP.NET closes
            appLifetime.ApplicationStopped.Register(() => {
                app.ApplicationServices
                   .GetRequiredService<ICouchbaseLifetimeService>()
                   .Close();
            });
            // caused the reply_url to start with https
            app.Use((context, next) =>
            {
                context.Request.Scheme = "https";
                return next();
            });
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
