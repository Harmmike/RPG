using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Engine.Models
{
    public abstract class LivingEntity : BaseNotificationClass //cannot instantiate abstract classes.
    {
        private string _name;
        private int _currentHitPoints;
        private int _maximumHitPoints;
        private int _gold;
        private int _level;
        private GameItem _currentWeapon;
        private GameItem _currentConsumable;

        public string Name
        {
            get { return _name; }
            private set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        public int CurrentHitPoints
        {
            get { return _currentHitPoints; }
            private set
            {
                _currentHitPoints = value;
                OnPropertyChanged();
            }
        }
        public int MaximumHitPoints
        {
            get { return _maximumHitPoints; }
            protected set
            {
                _maximumHitPoints = value;
                OnPropertyChanged();
            }
        } //protected allows the child to change its own
        public int Gold
        {
            get { return _gold; }
            private set
            {
                _gold = value;
                OnPropertyChanged();
            }
        }
        public int Level
        {
            get { return _level; }
            protected set
            {
                _level = value;
                OnPropertyChanged();
            }
        }
        public GameItem CurrentWeapon
        {
            get { return _currentWeapon; }
            set
            {
                if(_currentWeapon != null)
                {
                    _currentWeapon.Action.OnActionPerformed -= RaiseActionPerformedEvent; //unscubscribe
                }

                _currentWeapon = value;

                if(_currentWeapon != null)
                {
                    _currentWeapon.Action.OnActionPerformed += RaiseActionPerformedEvent; //subscribe
                }
                OnPropertyChanged();
            }
        }
        public GameItem CurrentConsumable
        {
            get { return _currentConsumable; }
            set
            {
                if(_currentConsumable != null)
                {
                    _currentConsumable.Action.OnActionPerformed -= RaiseActionPerformedEvent; //unsubscribe
                }

                _currentConsumable = value;

                if(_currentConsumable != null)
                {
                    _currentConsumable.Action.OnActionPerformed += RaiseActionPerformedEvent; //subscribe;
                }
                OnPropertyChanged();
            }
        }

        public bool HasConsumable => Consumables.Any(); //checks the list of consumables for any entries. Used to give a boolean to display in UI.
        public bool IsDead => CurrentHitPoints <= 0;

        public event EventHandler OnKilled;
        public event EventHandler<string> OnActionPerformed;

        public ObservableCollection<GameItem> Inventory { get; }
        public ObservableCollection<GroupedInventoryItem> GroupedInventory { get; }

        public List<GameItem> Weapons => Inventory.Where(i => i.Category is GameItem.ItemCategory.Weapon).ToList(); //Checks the inventory for items that have a category of Weapon, then adds them to the list.
        public List<GameItem> Consumables => Inventory.Where(i => i.Category is GameItem.ItemCategory.Consumable).ToList(); //Checks inventory for consumables and adds them to this list.

        protected LivingEntity(string name, int maximumHitPoints, int currentHitPoints, int gold, int level = 1)
        {
            Name = name;
            MaximumHitPoints = maximumHitPoints;
            CurrentHitPoints = currentHitPoints;
            Gold = gold;
            Level = level;

            Inventory = new ObservableCollection<GameItem>(); //initializing inventory upon creation.
            GroupedInventory = new ObservableCollection<GroupedInventoryItem>(); //initializes the grouped inventory.
        }

        public void UseCurrentWeaponOn(LivingEntity target)
        {
            CurrentWeapon.PerformAction(this, target);
        }

        public void UseCurrentConsumable()
        {
            CurrentConsumable.PerformAction(this, this); //calls the function to use the consumable. In the future, items could be used on other objects, for now just the player.
            RemoveItemFromInventory(CurrentConsumable); //removes the item from the inventory.
        }

        public void TakeDamage(int hitPointsOfDamage)
        {
            CurrentHitPoints -= hitPointsOfDamage;
            if(IsDead)
            {
                CurrentHitPoints = 0;
                RaiseOnKilledEvent();
            }
        }

        public void Heal(int hitPointsToHeal)
        {
            CurrentHitPoints += hitPointsToHeal;
            if(CurrentHitPoints > MaximumHitPoints)
            {
                CurrentHitPoints = MaximumHitPoints;
            }
        }

        public void FullHeal()
        {
            CurrentHitPoints = MaximumHitPoints;
        }

        public void ReceiveGold(int amount)
        {
            Gold += amount;
        }

        public void SpendGold(int amount)
        {
            if(amount > Gold)
            {
                throw new ArgumentOutOfRangeException($"{Name} only has {Gold} gold, and cannot afford this item.");
            }
            Gold -= amount;
        }

        public void AddItemToInventory(GameItem item)
        {
            Inventory.Add(item);

            if(item.IsUnique) //checks if the item is unique.
            {
                GroupedInventory.Add(new GroupedInventoryItem(item, 1)); //if the item is unique, it creates a new item and adds it to the inventory.
            }
            else
            {
                if(!GroupedInventory.Any(gi => gi.Item.ItemTypeID == item.ItemTypeID)) //if the item is not unique, checks to see if it's the first type of this item, or if the quantity just needs to be increased.
                {
                    GroupedInventory.Add(new GroupedInventoryItem(item, 0)); //if there are no other items of that match this item id, it this creates a new grouped inventory for that item id.
                }

                GroupedInventory.First(gi => gi.Item.ItemTypeID == item.ItemTypeID).Quantity++; //if the player already has the item id, this increases the quantity.
            }

            OnPropertyChanged(nameof(Weapons)); //Just in case the item is a weapon, since List<T> does not raise events.
            OnPropertyChanged(nameof(Consumables)); //Derived lists will not automatically raise events.
            OnPropertyChanged(nameof(HasConsumable)); //This is for notifying the UI of the change.
        }

        public void RemoveItemFromInventory(GameItem item)
        {
            Inventory.Remove(item);

            GroupedInventoryItem groupedInventoryItemToRemove = GroupedInventory.FirstOrDefault(gi => gi.Item == item); //gets the first item from the grouped inventory that matches the item id.

            if(groupedInventoryItemToRemove != null)
            {
                if (groupedInventoryItemToRemove.Quantity == 1)
                {
                    GroupedInventory.Remove(groupedInventoryItemToRemove); //this completely removes the grouped inventory item if we only have 1.
                }
                else
                {
                    groupedInventoryItemToRemove.Quantity--; //if we have more than 1 of the grouped inventory item, this removes 1.
                }
            }

            OnPropertyChanged(nameof(Weapons)); //Just in case the item is a weapon, since List<T> does not raise events.
            OnPropertyChanged(nameof(Consumables));
            OnPropertyChanged(nameof(HasConsumable));
        }

        private void RaiseOnKilledEvent()
        {
            OnKilled?.Invoke(this, new System.EventArgs());
        }

        private void RaiseActionPerformedEvent(object sender, string result)
        {
            OnActionPerformed?.Invoke(this, result);
        }

        public void RemoveItemsFromInventory(List<ItemQuantity> itemQuantities)
        {
            foreach (ItemQuantity itemQuantity in itemQuantities)
            {
                for (int i = 0; i < itemQuantity.Quantity; i++)
                {
                    RemoveItemFromInventory(Inventory.First(item => item.ItemTypeID == itemQuantity.ItemID));
                }
            }
        }

        public bool HasAllTheseItems(List<ItemQuantity> items)
        {
            foreach (ItemQuantity item in items)
            {
                if (Inventory.Count(i => i.ItemTypeID == item.ItemID) < item.Quantity)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
