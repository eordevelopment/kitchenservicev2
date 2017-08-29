using System;
using System.Linq;
using AutoMapper;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;

namespace KitchenServiceV2
{
    public class AutoMapperConfig
    {
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

                cfg.CreateMap<Plan, PlanDto>().AfterMap((src, dest) =>
                {
                    dest.DateTime = DateTimeOffset.FromUnixTimeSeconds(src.DateTimeUnixSeconds);
                    dest.Items = src.PlanItems?.Select(Mapper.Map<PlanItemDto>).ToList();
                });
                cfg.CreateMap<PlanDto, Plan>().AfterMap((src, dest) =>
                {
                    dest.PlanItems = src.Items?.Select(Mapper.Map<PlanItem>).ToList();
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
