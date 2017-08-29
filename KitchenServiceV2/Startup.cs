using System;
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

            // Auto mapper
            InitializeMapper();

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

        public static void InitializeMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Category, CategoryDto>();
                cfg.CreateMap<CategoryDto, Category>().AfterMap((src, dest) => dest.Name = dest.Name.ToLower());

                cfg.CreateMap<Item, ItemDto>();
                cfg.CreateMap<ItemDto, Item>().AfterMap((src, dest) => dest.Name = dest.Name.ToLower());

                cfg.CreateMap<RecipeType, RecipeTypeDto>();
                cfg.CreateMap<RecipeTypeDto, RecipeType>().AfterMap((src, dest) => dest.Name = dest.Name.ToLower());

                cfg.CreateMap<Recipe, RecipeDto>();
                cfg.CreateMap<RecipeDto, Recipe>().AfterMap((src, dest) => dest.Name = dest.Name.ToLower());

                cfg.CreateMap<RecipeStep, RecipeStepDto>();
                cfg.CreateMap<RecipeStepDto, RecipeStep>();
                cfg.CreateMap<RecipeItem, RecipeItemDto>().AfterMap((src, dest) => dest.Item = new ItemDto
                {
                    Id = src.ItemId.ToString()
                });
                cfg.CreateMap<RecipeItemDto, RecipeItem>();

                cfg.CreateMap<Plan, PlanDto>().AfterMap((src, dest) => dest.DateTime = DateTimeOffset.FromUnixTimeSeconds(src.DateTimeUnixSeconds));
                cfg.CreateMap<PlanDto, Plan>().AfterMap((src, dest) =>
                {
                    dest.DateTimeUnixSeconds = src.DateTime.ToUnixTimeSeconds();
                    dest.IsDone = src.Items != null && src.Items.All(x => x.IsDone);
                });

                cfg.CreateMap<PlanItem, PlanItemDto>();
                cfg.CreateMap<PlanItemDto, PlanItem>();

                cfg.CreateMap<string, ObjectId>().ConvertUsing(s =>
                {
                    if (string.IsNullOrWhiteSpace(s)) return ObjectId.Empty;
                    return ObjectId.TryParse(s, out ObjectId id) ? id : ObjectId.Empty;
                });
                cfg.CreateMap<ObjectId, string>().ConvertUsing(id => id.ToString());
            });
        }
    }
}
