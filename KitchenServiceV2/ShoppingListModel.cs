using System;
using System.Collections.Generic;
using System.Linq;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;

namespace KitchenServiceV2
{
    public interface IShoppingListModel
    {
        ShoppingList CreateShoppingList(string userToken, ICollection<Recipe> recipes, IReadOnlyDictionary<ObjectId, Item> itemsById, IReadOnlyCollection<ItemToBuy> mustByItems);
    }

    public class ShoppingListModel : IShoppingListModel
    {
        public ShoppingList CreateShoppingList(string userToken, ICollection<Recipe> recipes, IReadOnlyDictionary<ObjectId, Item> itemsById, IReadOnlyCollection<ItemToBuy> mustByItems)
        {
            var recipItemsById = recipes
                .SelectMany(x => x.RecipeItems)
                .ToLookup(x => x.ItemId);

            var recipesByItemId = GetRecipesByItemId(recipes);

            var now = DateTimeOffset.UtcNow;
            var shoppingList = new ShoppingList
            {
                CreatedOnUnixSeconds = now.ToUnixTimeSeconds(),
                Name = now.ToString("ddd, MMM-dd yyyy"),
                IsDone = false,
                UserToken = userToken,
                Items = new List<ShoppingListItem>(),
                OptionalItems = new List<ShoppingListItem>()
            };


            foreach (var itemCollection in recipItemsById)
            {
                if (!itemsById.ContainsKey(itemCollection.Key)) continue;

                float inStock = 0, needed = 0;
                var item = itemsById[itemCollection.Key];

                foreach (var recipeItem in itemCollection)
                {
                    inStock = item.Quantity;
                    needed += recipeItem.Amount;
                }

                var shoppingListItem = new ShoppingListItem
                {
                    ItemId = itemCollection.Key,
                    IsDone = false,
                    Amount = needed - inStock,
                    TotalAmount = needed,
                    RecipeIds = recipesByItemId.ContainsKey(itemCollection.Key) ? recipesByItemId[itemCollection.Key] : new HashSet<ObjectId>()
                };

                if (needed > inStock)
                {
                    shoppingList.Items.Add(shoppingListItem);
                }
                else
                {
                    shoppingListItem.Amount = shoppingListItem.TotalAmount;
                    shoppingList.OptionalItems.Add(shoppingListItem);
                }
            }

            foreach (var mustByItem in mustByItems)
            {
                var optionalItem = shoppingList.OptionalItems.FirstOrDefault(x => x.ItemId == mustByItem.ItemId);
                if (optionalItem != null)
                {
                    shoppingList.OptionalItems.Remove(optionalItem);
                    shoppingList.Items.Add(optionalItem);
                    continue;
                }

                if(shoppingList.Items.Any(x => x.ItemId == mustByItem.ItemId))
                    continue;

                shoppingList.Items.Add(new ShoppingListItem
                {
                    IsDone = false,
                    ItemId = mustByItem.ItemId,
                    Amount = 1,
                    TotalAmount = 1
                });
            }

            return shoppingList;
        }

        private static Dictionary<ObjectId, HashSet<ObjectId>> GetRecipesByItemId(ICollection<Recipe> recipes)
        {
            var recipesByItemId = new Dictionary<ObjectId, HashSet<ObjectId>>();
            foreach (var recipe in recipes)
            {
                foreach (var item in recipe.RecipeItems)
                {
                    if (recipesByItemId.ContainsKey(item.ItemId))
                    {
                        recipesByItemId[item.ItemId].Add(recipe.Id);
                    }
                    else
                    {
                        recipesByItemId.Add(item.ItemId, new HashSet<ObjectId> { recipe.Id });
                    }
                }
            }

            return recipesByItemId;
        }
    }
}
