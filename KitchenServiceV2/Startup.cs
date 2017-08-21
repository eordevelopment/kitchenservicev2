using System.Linq;
using AutoMapper;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Repository;
using KitchenServiceV2.Db.Mongo.Schema;
using KitchenServiceV2.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;

namespace KitchenServiceV2
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AuthorizationOptions>(options =>
            {
                options.AddPolicy("HasToken", policy => policy.Requirements.Add(new UserTokenPolicy()));
            });

            var conn = Configuration.GetSection("mongoDbConnection").Value;
            var db = Configuration.GetSection("mongoDb").Value;

            services.AddScoped<IDbContext, DbContext>(ctx => new DbContext(conn, db));
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IRecipeTypeRepository, RecipeTypeRepository>();
            services.AddScoped<IRecipeRepository, RecipeRepository>();

            // Auto mapper
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Category, CategoryDto>();
                cfg.CreateMap<Item, ItemDto>();


                cfg.CreateMap<string, ObjectId>().ConvertUsing(s => new ObjectId(s));
                cfg.CreateMap<ObjectId, string>().ConvertUsing(id => id.ToString());
            });

            services.AddMvc();
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Shows UseCors with CorsPolicyBuilder.
            var origins = Configuration
                .GetSection("CorsOrigin")
                .GetChildren()
                .Select(x => x.Value)
                .ToArray();
            app.UseCors(builder => builder.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader());

            //loggerFactory.AddConsole(LogLevel.Debug).AddDebug(LogLevel.Debug);

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseMvc();
        }
    }
}
