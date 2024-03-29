﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;using Engine.Factories;

namespace Engine.Models
{
    public class Monster :  LivingEntity
    {
        private readonly List<ItemPercentage> _lootTable = new List<ItemPercentage>();

        public string ImageName { get; }
        public int ID { get; set; }
        public int RewardExperiencePoints { get; }

        public Monster(int id, string name, string imageName, int maximumHitPoints, GameItem currentWeapon, int rewardExperiencePoints, int gold)
            : base(name, maximumHitPoints, maximumHitPoints, gold)
        {
            ID = id;
            ImageName = imageName;
            CurrentWeapon = currentWeapon;
            RewardExperiencePoints = rewardExperiencePoints;
        }

        public void AddItemToLootTable(int id, int percentage) //this populates the loot table when a monster is created.
        {
            // Remove the entry from the loot table,
            // if it already contains an entry with this ID
            _lootTable.RemoveAll(ip => ip.ID == id);

            _lootTable.Add(new ItemPercentage(id, percentage));
        }

        public Monster GetNewInstance()
        {
            // "Clone" this monster to a new Monster object
            Monster newMonster =  new Monster(ID, Name, ImageName, MaximumHitPoints, CurrentWeapon, RewardExperiencePoints, Gold);

            foreach (ItemPercentage itemPercentage in _lootTable)
            {
                // Clone the loot table - even though we probably won't need it
                newMonster.AddItemToLootTable(itemPercentage.ID, itemPercentage.Percentage);

                // Populate the new monster's inventory, using the loot table
                if (RandomNumberGenerator.NumberBetween(1, 100) <= itemPercentage.Percentage)
                {
                    newMonster.AddItemToInventory(ItemFactory.CreateGameItem(itemPercentage.ID));
                }
            }

            return newMonster;
        }
    }
}
