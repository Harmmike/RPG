using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Models;
using Engine.Factories;
using Engine.EventArgs;
using System.ComponentModel;

namespace Engine.ViewModels
{
    public class GameSession : BaseNotificationClass
    {
        public event EventHandler<GameMessageEventArgs> OnMessageRaised;

        private Player _currentPlayer;
        private Location _currentLocation;
        private Monster _currentMonster;
        private Trader _currentTrader;

        public Player CurrentPlayer
        {
            get { return _currentPlayer; }
            set
            {
                if(_currentPlayer != null)
                {
                    _currentPlayer.OnActionPerformed -= OnCurrentPlayerPerformedAction;
                    _currentPlayer.OnKilled -= OnCurrentPlayerKilled; // if the game session already has a current player, we will unsubscribe from the event. (this is in case a bug creates a new player)
                    _currentPlayer.OnLeveledUp -= OnCurrentPlayerLeveledUp;
                }

                //When you choose an object.event and -= or += it unsubs or subs to that event.

                _currentPlayer = value;

                if(_currentPlayer != null)
                {
                    _currentPlayer.OnActionPerformed += OnCurrentPlayerPerformedAction;
                    _currentPlayer.OnKilled += OnCurrentPlayerKilled; // if the current player is not null, we will subscribe to the event.
                    _currentPlayer.OnLeveledUp += OnCurrentPlayerLeveledUp;
                }
            }
        }
        public World CurrentWorld { get; }
        public Location CurrentLocation
        {
            get { return _currentLocation; }
            set
            {
                _currentLocation = value; //when current location changes, it raises these events...

                OnPropertyChanged(); //we're using nameof because it's safer.. if we were to change method names later this would change with it...
                OnPropertyChanged(nameof(HasLocationToNorth));
                OnPropertyChanged(nameof(HasLocationToWest));
                OnPropertyChanged(nameof(HasLocationToEast));
                OnPropertyChanged(nameof(HasLocationToSouth));

                CompleteQuestsAtLocation();
                GivePlayerQuestsAtLocation();
                GetMonsterAtLocation();

                CurrentTrader = CurrentLocation.TraderHere;
            }
        }
        public Monster CurrentMonster
        {
            get { return _currentMonster; }
            set
            {
                if(_currentMonster != null) //if the old current monster was not null, we unsubscribe from its onkilled event.
                {
                    _currentMonster.OnActionPerformed -= OnCurrentMonsterPerformedAction;
                    _currentMonster.OnKilled -= OnCurrentMonsterKilled; //Unsub
                }

                _currentMonster = value; // set variable to new value.

                if(_currentMonster != null) //if there is a current monster (not null).
                {
                    //subscribe to event
                    _currentMonster.OnActionPerformed += OnCurrentMonsterPerformedAction;
                    _currentMonster.OnKilled += OnCurrentMonsterKilled;

                    //raise an empty string to put a blank line.
                    //raise a message that shows what monster you found.
                    RaiseMessage("");
                    RaiseMessage($"You see a {CurrentMonster.Name} here!");
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMonster));
            }
        }
        public Trader CurrentTrader
        {
            get { return _currentTrader; }
            set
            {
                _currentTrader = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasTrader));
            }
        }

        public bool HasLocationToNorth =>
            CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate + 1) != null;//checks the location with a y coord + 1 of the current y coord.

        public bool HasLocationToWest =>
            CurrentWorld.LocationAt(CurrentLocation.XCoordinate - 1, CurrentLocation.YCoordinate) != null;

        public bool HasLocationToEast =>
            CurrentWorld.LocationAt(CurrentLocation.XCoordinate + 1, CurrentLocation.YCoordinate) != null;

        public bool HasLocationToSouth =>
            CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate - 1) != null;

        public bool HasMonster => CurrentMonster != null;  //a bool that shows false if the CurrentMonster property is null.

        public bool HasTrader => CurrentTrader != null; //Checks to see if we have a trader at the location.

        public GameSession()
        {
            CurrentPlayer = new Player("Michael", "Warrior", 0, 10, 10, 100);

            if(!CurrentPlayer.Weapons.Any())
            {
                CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(1001));
            }

            CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(2001)); //Gives the player a Granola Bar to heal themselves with.

            CurrentPlayer.LearnRecipe(RecipeFactory.RecipeByID(1)); //Gives the player the granola bar recipe.

            CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(3001)); //Gives the player some items to test crafting.
            CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(3001)); //Gives the player some items to test crafting.
            CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(3002)); //Gives the player some items to test crafting.
            CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(3002)); //Gives the player some items to test crafting.
            CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(3003)); //Gives the player some items to test crafting.
            CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(3003)); //Gives the player some items to test crafting.

            CurrentWorld = WorldFactory.CreateWorld(); //uses the static factory class to create a world and sets the current world to the newly created world.

            CurrentLocation = CurrentWorld.LocationAt(0, 0);
        }

        public void MoveNorth()
        {
            if(HasLocationToNorth) //example of a guard clause.
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate + 1); // finds the current location and adds to the corresponding coordinate.
            }
            if(CurrentLocation == CurrentWorld.LocationAt(0, -1))
            {
                CurrentPlayer.FullHeal();
                RaiseMessage($"You were healed at home.");
            }
            if(HasTrader)
            {
                RaiseMessage($"You can trade with {CurrentLocation.TraderHere.Name} here.");
            }
        }
        public void MoveWest()
        {
            if(HasLocationToWest)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate - 1, CurrentLocation.YCoordinate);
            }
            if (CurrentLocation == CurrentWorld.LocationAt(0, -1))
            {
                CurrentPlayer.FullHeal();
                RaiseMessage($"You were healed at home.");
            }
            if (HasTrader)
            {
                RaiseMessage($"You can trade with {CurrentLocation.TraderHere.Name} here.");
            }
        }
        public void MoveEast()
        {
            if(HasLocationToEast)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate + 1, CurrentLocation.YCoordinate);
            }
            if (CurrentLocation == CurrentWorld.LocationAt(0, -1))
            {
                CurrentPlayer.FullHeal();
                RaiseMessage($"You were healed at home.");
            }
            if (HasTrader)
            {
                RaiseMessage($"You can trade with {CurrentLocation.TraderHere.Name} here.");
            }
        }
        public void MoveSouth()
        {
            if(HasLocationToSouth)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate - 1);
            }
            if (CurrentLocation == CurrentWorld.LocationAt(0, -1))
            {
                CurrentPlayer.FullHeal();
                RaiseMessage($"You were healed at home.");
            }
            if (HasTrader)
            {
                RaiseMessage($"You can trade with {CurrentLocation.TraderHere.Name} here.");
            }
        }

        private void CompleteQuestsAtLocation()
        {
            foreach (Quest quest in CurrentLocation.QuestsAvailableHere)
            {
                QuestStatus questToComplete =
                    CurrentPlayer.Quests.FirstOrDefault(q => q.PlayerQuest.ID == quest.ID &&
                                                             !q.IsCompleted);

                if (questToComplete != null)
                {
                    if (CurrentPlayer.HasAllTheseItems(quest.ItemsToComplete))
                    {
                        // Remove the quest completion items from the player's inventory
                        foreach (ItemQuantity itemQuantity in quest.ItemsToComplete)
                        {
                            for (int i = 0; i < itemQuantity.Quantity; i++)
                            {
                                CurrentPlayer.RemoveItemFromInventory(CurrentPlayer.Inventory.First(item => item.ItemTypeID == itemQuantity.ItemID));
                            }
                        }

                        RaiseMessage("");
                        RaiseMessage($"You completed the '{quest.Name}' quest");

                        // Give the player the quest rewards
                        CurrentPlayer.AddExperience(quest.RewardExperience);
                        RaiseMessage($"You receive {quest.RewardExperience} experience points");

                        CurrentPlayer.ReceiveGold(quest.RewardGold);
                        RaiseMessage($"You receive {quest.RewardGold} gold");

                        foreach (ItemQuantity itemQuantity in quest.RewardItems)
                        {
                            GameItem rewardItem = ItemFactory.CreateGameItem(itemQuantity.ItemID);

                            CurrentPlayer.AddItemToInventory(rewardItem);
                            RaiseMessage($"You receive a {rewardItem.Name}");
                        }

                        // Mark the Quest as completed
                        questToComplete.IsCompleted = true;
                    }
                }
            }
        }
        private void GivePlayerQuestsAtLocation()
        {
            foreach (Quest quest in CurrentLocation.QuestsAvailableHere)
            {
                if(!CurrentPlayer.Quests.Any(q => q.PlayerQuest.ID == quest.ID)) //this checks if the quests at the current location match any quests in the player's questlist.
                {
                    CurrentPlayer.Quests.Add(new QuestStatus(quest)); //if none of the available quests match, this will add the quest.

                    RaiseMessage("");
                    RaiseMessage($"You receive the '{quest.Name}' quest.");
                    RaiseMessage(quest.Description);

                    RaiseMessage("Return with: ");
                    foreach (ItemQuantity itemQuantity in quest.ItemsToComplete)
                    {
                        RaiseMessage($"{itemQuantity.Quantity} {ItemFactory.CreateGameItem(itemQuantity.ItemID).Name}");
                    }

                    RaiseMessage("And you will receive:");
                    RaiseMessage($" {quest.RewardExperience} experience points,");
                    RaiseMessage($" {quest.RewardGold} gold, and");
                    foreach (ItemQuantity itemQuantity in quest.RewardItems)
                    {
                        RaiseMessage($" {itemQuantity.Quantity} {ItemFactory.CreateGameItem(itemQuantity.ItemID).Name}");
                    }
                }
            }
        }
        private void GetMonsterAtLocation()
        {
            CurrentMonster = CurrentLocation.GetMonster();
        }

        public void RaiseMessage(string message) //when the view model wants to raise an event it will call this function and send a message.
        {
            //this code checks if OnMessageRaised has any suscribers(will be null if no subs).
            //if there are subs, it will invoke the event
            //passing a new GameMessageEventArgs object with the message.
            OnMessageRaised?.Invoke(this, new GameMessageEventArgs(message)); 
        }

        public void AttackCurrentMonster()
        {
            if(CurrentMonster == null)
            {
                return;
            }

            if(CurrentPlayer.CurrentWeapon == null) //bail early...
            {
                RaiseMessage("You must select a weapon to attack.");
                return;
            }

            CurrentPlayer.UseCurrentWeaponOn(CurrentMonster);

            if(CurrentMonster.IsDead)
            {
                //Get next monster to fight...
                GetMonsterAtLocation();
            }
            else
            {
                CurrentMonster.UseCurrentWeaponOn(CurrentPlayer);
            }
        }

        public void UseCurrentConsumable()
        {
            if (CurrentPlayer.CurrentConsumable != null)
            {
                CurrentPlayer.UseCurrentConsumable();
            }
        }

        public void CraftItemUsing(Recipe recipe)
        {
            if (CurrentPlayer.HasAllTheseItems(recipe.Ingredients))
            {
                CurrentPlayer.RemoveItemsFromInventory(recipe.Ingredients);

                foreach (ItemQuantity itemQuantity in recipe.OutputItems)
                {
                    for (int i = 0; i < itemQuantity.Quantity; i++)
                    {
                        GameItem outputItem = ItemFactory.CreateGameItem(itemQuantity.ItemID);
                        CurrentPlayer.AddItemToInventory(outputItem);
                        RaiseMessage($"You craft 1 {outputItem.Name}");
                    }
                }
            }
            else
            {
                RaiseMessage("You do not have the required ingredients:");
                foreach (ItemQuantity itemQuantity in recipe.Ingredients)
                {
                    RaiseMessage($"  {itemQuantity.Quantity} {ItemFactory.ItemName(itemQuantity.ItemID)}");
                }
            }
        }

        private void OnCurrentPlayerKilled(object sender, System.EventArgs eventArgs)
        {
            RaiseMessage("");
            //RaiseMessage($"The {CurrentMonster.Name} has killed you.");
            RaiseMessage("You have died...");

            CurrentLocation = CurrentWorld.LocationAt(0, -1);
            CurrentPlayer.FullHeal();
        }

        private void OnCurrentMonsterKilled(object sender, System.EventArgs eventArgs)
        {
            RaiseMessage("");
            RaiseMessage($"You have killed the {CurrentMonster.Name}!");

            RaiseMessage($"You receive {CurrentMonster.RewardExperiencePoints} experience points.");
            CurrentPlayer.AddExperience(CurrentMonster.RewardExperiencePoints);

            RaiseMessage($"You receive {CurrentMonster.Gold} gold.");
            CurrentPlayer.ReceiveGold(CurrentMonster.Gold);

            foreach (GameItem gameItem in CurrentMonster.Inventory)
            {
                RaiseMessage($"You receive one {gameItem.Name}");
                CurrentPlayer.AddItemToInventory(gameItem);
            }
        }

        private void OnCurrentPlayerLeveledUp(object sender, System.EventArgs eventArgs)
        {
            RaiseMessage($"You are now level {CurrentPlayer.Level}!");
        }

        private void OnCurrentPlayerPerformedAction(object sender, string result)
        {
            RaiseMessage(result);
        }

        private void OnCurrentMonsterPerformedAction(object sender, string result)
        {
            RaiseMessage(result);
        }
    }
}
