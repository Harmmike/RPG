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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Engine.Models;
using Engine.ViewModels;
using Engine.EventArgs;

namespace WPFUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly GameSession _gameSession = new GameSession(); //instantiates - this gets put into memory.
        private readonly Dictionary<Key, Action> _userInputActions = new Dictionary<Key, Action>(); //The key is the keypressed, and it will have an action as the value.

        public MainWindow()
        {
            InitializeComponent();

            InitializeUserInputActions();

            _gameSession.OnMessageRaised += OnGameMessageRaised; //this subscribes to the event handler. When _gameSession raises an OnMessageRaised event, run OnGameMessageRaised function.
            DataContext = _gameSession; //sets the datacontext for xaml file
        }

        //OnClick Methods
        private void OnClick_MoveNorth(object sender, RoutedEventArgs e)
        {
            _gameSession.MoveNorth();
        }
        private void OnClick_MoveWest(object sender, RoutedEventArgs e)
        {
            _gameSession.MoveWest();
        }
        private void OnClick_MoveEast(object sender, RoutedEventArgs e)
        {
            _gameSession.MoveEast();
        }
        private void OnClick_MoveSouth(object sender, RoutedEventArgs e)
        {
            _gameSession.MoveSouth();
        }
        private void OnClick_AttackMonster(object sender, RoutedEventArgs e)
        {
            _gameSession.AttackCurrentMonster();
        }
        private void OnClick_DisplayTradeScreen(object sender, RoutedEventArgs e)
        {
            if (_gameSession.CurrentTrader != null) //gaurd clause to ensure the player is at a trader.
            {
                TradeScreen tradeScreen = new TradeScreen(); //instantiates a new trade screen.
                tradeScreen.Owner = this; //set the owner to the main window because we want to set the trade screen to the entire screen.
                tradeScreen.DataContext = _gameSession; //sets the object that the trade screen will be working with to our _gameSession. - this is just a reference to the object.
                tradeScreen.ShowDialog(); //this prevents the user from being able to click on the buttons on the main window.
            }
        }
        private void OnClick_UseCurrentConsumable(object sender, RoutedEventArgs e)
        {
            _gameSession.UseCurrentConsumable();
        }
        private void OnClick_Craft(object sender, RoutedEventArgs e)
        {
            Recipe recipe = ((FrameworkElement)sender).DataContext as Recipe;
            _gameSession.CraftItemUsing(recipe);
        }

        private void InitializeUserInputActions() //we initial this in the constructor.
        {
            //These assign the various keys and their actions.
            _userInputActions.Add(Key.W, () => _gameSession.MoveNorth()); // the () => is a pointer function - this is for delegates and assigns the action for the dictionary value.
            _userInputActions.Add(Key.A, () => _gameSession.MoveWest()); // the () can be used to define paramters, however our methods don't require any.
            _userInputActions.Add(Key.S, () => _gameSession.MoveSouth());
            _userInputActions.Add(Key.D, () => _gameSession.MoveEast());
            _userInputActions.Add(Key.Z, () => _gameSession.AttackCurrentMonster());
            _userInputActions.Add(Key.C, () => _gameSession.UseCurrentConsumable());
            _userInputActions.Add(Key.I, () => SetTabFocusTo("InventoryTabItem"));
            _userInputActions.Add(Key.Q, () => SetTabFocusTo("QuestTabItem"));
            _userInputActions.Add(Key.R, () => SetTabFocusTo("RecipesTabItem"));
            _userInputActions.Add(Key.T, () => OnClick_DisplayTradeScreen(this, new RoutedEventArgs()));
        }

        private void SetTabFocusTo(string tabName)
        {
            foreach (object item in PlayerDataTabControl.Items) //iterates through the tab item called PlayerDataTabControl, and finds each tab item.
            {
                if (item is TabItem tabItem) //if the item is a tab item within the PlayerDataTabControl
                {
                    if (tabItem.Name == tabName) //if the names match.
                    {
                        tabItem.IsSelected = true; //selects the focus of that tab.
                        return;
                    }
                }
            }
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e) //this looks at the event args parameter "e" and sees which key was pressed.
        {
            //checks if the dictionary has a key that matches the key pressed
            if (_userInputActions.ContainsKey(e.Key))
            {
                //If the dictionary does have the key, it invokes the necessary method.
                _userInputActions[e.Key].Invoke();
            }
        }

        private void OnGameMessageRaised(object sender, GameMessageEventArgs e) //datatype is GameMessageEventArgs because we defined that in the GameSession.
        {
            //When this function gets called it will take the GameMessageEventArgs object,
            //get the value of it's .Message property, format it (adding a "run" of text)
            //then add it to the rich text box document in the .xaml.
            //Then it will scroll to the bottom of the text box so the user can always see the latest message.
            GameMessages.Document.Blocks.Add(new Paragraph(new Run(e.Message)));
            GameMessages.ScrollToEnd();
        }
    }
}
