using System;
using System.Linq;
using AutoMapper;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;

namespace KitchenServiceV2
{
    using System.Collections.Generic;

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

                cfg.CreateMap<ShoppingList, ShoppingListDto>().AfterMap((src, dest) =>
                {
                    dest.CreatedOn = DateTimeOffset.FromUnixTimeSeconds(src.CreatedOnUnixSeconds);
                });
                cfg.CreateMap<ShoppingListDto, ShoppingList>().AfterMap((src, dest) =>
                {
                    dest.CreatedOnUnixSeconds = src.CreatedOn.ToUnixTimeSeconds();
                    dest.IsDone = src.IsDone || src.Items.All(itm => itm.IsDone);
                });

                cfg.CreateMap<ShoppingListItem, ShoppingListItemDto>().AfterMap((src, dest) =>
                {
                    dest.Item = new ItemDto{Id = src.ItemId.ToString()};
                    dest.Recipes = src.RecipeIds?.Select(x => new RecipeDto {Id = x.ToString()}) ?? new List<RecipeDto>();
                });
                cfg.CreateMap<ShoppingListItemDto, ShoppingListItem>().AfterMap((src, dest) =>
                {
                    dest.ItemId = Mapper.Map<ObjectId>(src.Item?.Id);
                    dest.RecipeIds = new HashSet<ObjectId>();
                    if (src.Recipes == null || !src.Recipes.Any()) return;
                    foreach (var arg1Recipe in src.Recipes)
                    {
                        dest.RecipeIds.Add(Mapper.Map<ObjectId>(arg1Recipe.Id));
                    }
                });

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
