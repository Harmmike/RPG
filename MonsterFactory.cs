﻿using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Models;
using Engine.Shared;

namespace Engine.Factories
{
    class MonsterFactory
    {
        private const string GAME_DATA_FILENAME = ".\\GameData\\Monsters.xml";

        private static readonly List<Monster> _baseMonsters = new List<Monster>(); //This will be used to store the various monsters upon load.

        static MonsterFactory()
        {
            if (File.Exists(GAME_DATA_FILENAME))
            {
                XmlDocument data = new XmlDocument();
                data.LoadXml(File.ReadAllText(GAME_DATA_FILENAME));

                string rootImagePath = data.SelectSingleNode("/Monsters").AttributeAsString("RootImagePath");

                LoadMonstersFromNodes(data.SelectNodes("/Monsters/Monster"), rootImagePath);
            }
            else
            {
                throw new FileNotFoundException($"Missing data file: {GAME_DATA_FILENAME}");
            }
        }

        private static void LoadMonstersFromNodes(XmlNodeList nodes, string rootImagePath)
        {
            if (nodes == null)
            {
                return;
            }

            foreach (XmlNode node in nodes)
            {
                Monster monster = new Monster(node.AttributeAsInt("ID"), 
                    node.AttributeAsString("Name"), 
                    $".{rootImagePath}{node.AttributeAsString("ImageName")}", 
                    node.AttributeAsInt("MaximumHitPoints"), 
                    ItemFactory.CreateGameItem(node.AttributeAsInt("WeaponID")), 
                    node.AttributeAsInt("RewardXP"), 
                    node.AttributeAsInt("Gold"));

                XmlNodeList lootItemNodes = node.SelectNodes("./LootItems/LootItem");
                if (lootItemNodes != null)
                {
                    foreach (XmlNode lootItemNode in lootItemNodes)
                    {
                        monster.AddItemToLootTable(lootItemNode.AttributeAsInt("ID"),
                                                   lootItemNode.AttributeAsInt("Percentage"));
                    }
                }

                _baseMonsters.Add(monster);
            }
        }

        public static Monster GetMonster(int id)
        {
            return _baseMonsters.FirstOrDefault(m => m.ID == id)?.GetNewInstance();
        }

        private static void AddLootItem(Monster monster, int itemID, int percentage)
        {
            if(RandomNumberGenerator.SimpleNumberBetween(1, 100) <= percentage) //checks if the rng is greater than percentage chance, then adds the item if it is greater.
            {
                monster.AddItemToInventory(ItemFactory.CreateGameItem(itemID));
            }
        }
    }
}
