using System.Linq;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Repository;
using KitchenServiceV2.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                options.AddPolicy("HasToken", policy => policy.Requirements.Add(new UserTokenPolicyRequirement()));
            });

            var conn = Configuration.GetSection("mongoDbConnection").Value;
            var db = Configuration.GetSection("mongoDb").Value;

            services.AddScoped<IAuthorizationHandler, UserTokenPolicy>();
            services.AddScoped<IDbContext, DbContext>(ctx => new DbContext(conn, db));

            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IRecipeTypeRepository, RecipeTypeRepository>();
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<IPlanRepository, PlanRepository>();
            services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
            services.AddSingleton<IShoppingListModel, ShoppingListModel>();

            // Auto mapper
            AutoMapperConfig.InitializeMapper();

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

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseMvc();
        }
    }
}
