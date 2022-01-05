using Couchbase.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Wavelength.Core.Models;
using Wavelength.Server.WebAPI.Middleware;
using Wavelength.Server.WebAPI.Providers;
using Wavelength.Server.WebAPI.Repositories;
using Wavelength.Server.WebAPI.Services;

namespace Wavelength.Server.WebAPI
{
    public class Startup
	{
		public IConfiguration Configuration { get; }
		private ICouchbaseConfigService _config;
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			_config = new CouchbaseConfigService(Configuration);	
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{

			services.AddControllers();
			//Register the Swagger generator, defining 1 or more Swagger documents
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc(
					"v1",
					new OpenApiInfo
					{
						Title = "Wavelength Demo",
						Version = "v1"
					});
				c.CustomSchemaIds((type) => type.FullName);
				c.DescribeAllParametersInCamelCase();
			});

			//setup the couchbase config
			_config.InitConfig();
			services.AddSingleton<ICouchbaseConfigService>(_config);
				
			if (_config.Config.Mode == CouchbaseConfig.ModeServer) 
			{
				services.AddCouchbase(options => { 
					options.ConnectionString = _config.Config.ConnectionString;
					options.UserName = _config.Config.Username;
					options.Password = _config.Config.Password;
				});
				services.AddCouchbaseBucket<IWavelengthBucketProvider>("wavelength");
				services.AddSingleton<IAuctionRepository, AuctionRepository>();
				services.AddSingleton<IBidRepository, BidRepository>();
			}
			else 
			{
				services.AddSingleton<CouchbaseLiteService>();
				services.AddSingleton<IAuctionRepository, AuctionCBLiteRepository>();
			}

			//adding mediatr for cqrs support
			services.AddMediatR(typeof(Program));

			//https://timdows.com/projects/use-mediatr-with-fluentvalidation-in-the-asp-net-core-pipeline/
			//register validators with dependcy injection
			AssemblyScanner
				.FindValidatorsInAssembly(typeof(Startup).Assembly)
				.ForEach(result => services.AddTransient(result.InterfaceType, result.ValidatorType));


			//pipeline for validation
			services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(
			IApplicationBuilder app, 
			IWebHostEnvironment env,
			IHostApplicationLifetime appLifetime)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();
			app.UseRouting();
			app.UseCookiePolicy();

			//get the database service for configuring Couchbase
			var configService = app.ApplicationServices.GetService<ICouchbaseConfigService>();
			var dbService = app.ApplicationServices.GetService<CouchbaseLiteService>();

			if (_config.Config.Mode == CouchbaseConfig.ModeCBLite)
			{
				appLifetime.ApplicationStarted.Register(() =>
				{
					var dbService = app.ApplicationServices.GetService<CouchbaseLiteService>();
					var fp = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "StaticFiles");
					if (!System.IO.Directory.Exists(fp))
					{
						System.IO.Directory.CreateDirectory(fp);
					}
					app.UseStaticFiles(new StaticFileOptions
					{
						FileProvider = new PhysicalFileProvider(fp),
						RequestPath = "/StaticFiles"
					});

					dbService?.InitDatabase(fp);
				});
			}

			//remove couchbase from memory when ASP.NET closes
			appLifetime.ApplicationStopped.Register(() => 
			{
				if (_config.Config.Mode == CouchbaseConfig.ModeCBLite)
				{
					//stop replication
					dbService?.StopReplication();
					dbService?.Dispose();
				}
				else
				{
					app.ApplicationServices
				   .GetRequiredService<ICouchbaseLifetimeService>()
				   .Close();
				}
			});

			// Enable middleware to serve generated Swagger as a JSON endpoint.
			app.UseSwagger();

			//enable middleware to serve generated swagger as JSON enpoint.
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wavelength Demo");
				c.RoutePrefix = string.Empty;
			});

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
