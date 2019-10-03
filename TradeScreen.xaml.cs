using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Engine.Models;
using Engine.ViewModels;

namespace WPFUI
{
    /// <summary>
    /// Interaction logic for TradeScreen.xaml
    /// </summary>
    public partial class TradeScreen : Window
    {
        public GameSession Session => DataContext as GameSession;

        public TradeScreen()
        {
            InitializeComponent();
        }

        public void OnClick_Sell(object sender, RoutedEventArgs e)
        {
            GroupedInventoryItem groupedInventoryItem = ((FrameworkElement)sender).DataContext as GroupedInventoryItem; // learns what row the button is on.

            if(groupedInventoryItem != null) // if we have an item when we click on the button, perform necessary actions...
            {
                //These items already have the logic needed to refresh...
                Session.CurrentPlayer.ReceiveGold(groupedInventoryItem.Item.Price) ;
                Session.CurrentTrader.AddItemToInventory(groupedInventoryItem.Item);
                Session.CurrentPlayer.RemoveItemFromInventory(groupedInventoryItem.Item);
            }
        }

        public void OnClick_Buy(object sender, RoutedEventArgs e)
        {
            GroupedInventoryItem groupedInventoryItem = ((FrameworkElement)sender).DataContext as GroupedInventoryItem; //learns what row the button is on. *Magic*??

            if(groupedInventoryItem != null) //if the trader has the item perform necessary actions...
            {
                if(Session.CurrentPlayer.Gold >= groupedInventoryItem.Item.Price) //verify player has enough gold to buy the item.
                {
                    Session.CurrentPlayer.SpendGold(groupedInventoryItem.Item.Price);
                    Session.CurrentTrader.RemoveItemFromInventory(groupedInventoryItem.Item);
                    Session.CurrentPlayer.AddItemToInventory(groupedInventoryItem.Item);
                }
                else
                {
                    MessageBox.Show($"You do not have enough gold for {groupedInventoryItem.Item.Name}");
                }
            }    
        }

        public void OnClick_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
