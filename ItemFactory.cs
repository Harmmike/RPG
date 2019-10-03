using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Actions;
using Engine.Models;
using Engine.Shared;

namespace Engine.Factories
{
    public static class ItemFactory
    {
        //This string tells where to search for the data.
        //The single '.' tells to look in the current directory.
        //The \\ are a special character that tell C# to include just a single \.
        private const string GAME_DATA_FILENAME = ".\\GameData\\GameItems.xml";

        private static readonly List<GameItem> _standardGameItems = _standardGameItems = new List<GameItem>(); //creates an empty list of items upon the initial use of ItemFactory.

        static ItemFactory()
        {
            if (File.Exists(GAME_DATA_FILENAME)) //Checks if the file exists, and if it does...
            {
                XmlDocument data = new XmlDocument(); //Creates a new xml object.
                data.LoadXml(File.ReadAllText(GAME_DATA_FILENAME));

                LoadItemsFromNodes(data.SelectNodes("/GameItems/Weapons/Weapon"));
                LoadItemsFromNodes(data.SelectNodes("/GameItems/HealingItems/HealingItem"));
                LoadItemsFromNodes(data.SelectNodes("/GameItems/MiscellaneousItems/MiscellaneousItem"));
            }
            else
            {
                throw new FileNotFoundException($"Missing data file: {GAME_DATA_FILENAME}"); //if the data file doesn't exist, throws an error.
            }
        }

        public static GameItem CreateGameItem(int itemTypeID)
        {
            //returns standard game items (non weapon/misc items)
            return _standardGameItems.FirstOrDefault(item => item.ItemTypeID == itemTypeID)?.Clone();
        }

        public static void BuildMiscellaneousItem(int id, string name, int price)
        {
            //Creates a new GameItem of the Misc. category and assigns its ID, Name, and Price.
            //Because we assign the min/max damage in the constructor, we don't need to pass
            //values for them here, they'll be left at 0, and unused.
            _standardGameItems.Add(new GameItem(GameItem.ItemCategory.Miscellaneous, id, name, price));
        }

        public static void BuildHealingItem(int id, string name, int price, int hpToHeal)
        {
            //Composition Over Inheritance
            GameItem item = new GameItem(GameItem.ItemCategory.Consumable, id, name, price); //creates a new game item.
            item.Action = new Heal(item, 2); //gives the new game item an action of type Heal.
            _standardGameItems.Add(item); //adds the item.
        }

        public static void BuildWeapon(int id, string name, int price, int minDamage, int maxDamage)
        {
            //Creates a new GameItem of the Weapon category and assigns its values.
            GameItem weapon = new GameItem(GameItem.ItemCategory.Weapon, id, name, price, true);

            //We have to use 2 steps to set this up.  This is called composition over inheritance.
            weapon.Action = new AttackWithWeapon(weapon, minDamage, maxDamage);

            _standardGameItems.Add(weapon);
        }

        public static string ItemName(int itemTypeID)
        {
            //this takes in an item's id, finds the item, and returns its name.
            return _standardGameItems.FirstOrDefault(i => i.ItemTypeID == itemTypeID)?.Name ?? "";
        }

        private static void LoadItemsFromNodes(XmlNodeList nodes) //accepts a list of xml nodes.
        {
            if (nodes == null) //if there are no xml nodes, bail.
            {
                return;
            }

            foreach (XmlNode node in nodes) //iterates through all the nodes.
            {
                //Sets the item's category by calling the DetermineItemCategory method.
                GameItem.ItemCategory itemCategory = DetermineItemCategory(node.Name);

                //Creates a new game item with the appropriate category taken from the variable above.
                //Then calls the extension methods to get appropriate ints and strings to use for the appropriate constructors.
                GameItem gameItem =
                    new GameItem(itemCategory,
                                 node.AttributeAsInt("ID"),
                                 node.AttributeAsString("Name"),
                                 node.AttributeAsInt("Price"),
                                 itemCategory == GameItem.ItemCategory.Weapon);

                if (itemCategory == GameItem.ItemCategory.Weapon)
                {
                    gameItem.Action =
                        new AttackWithWeapon(gameItem,
                                             node.AttributeAsInt("MinimumDamage"),
                                             node.AttributeAsInt("MaximumDamage"));
                }
                else if (itemCategory == GameItem.ItemCategory.Consumable)
                {
                    gameItem.Action =
                        new Heal(gameItem,
                                 node.AttributeAsInt("HitPointsToHeal"));
                }

                //If the item is neither a weapon or consumable, returns the default game item (misc.).
                _standardGameItems.Add(gameItem);
            }
        }

        private static GameItem.ItemCategory DetermineItemCategory(string itemType)
        {
            switch (itemType) //checks each GameItem's category and returns its value.
            {
                case "Weapon":
                    return GameItem.ItemCategory.Weapon;
                case "HealingItem":
                    return GameItem.ItemCategory.Consumable;
                default:
                    return GameItem.ItemCategory.Miscellaneous;
            }
        }
    }
}
