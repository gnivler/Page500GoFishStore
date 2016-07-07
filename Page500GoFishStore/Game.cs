using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Page500GoFishStore
{
    class Game : INotifyPropertyChanged
    {
        private List<Player> players;
        private Dictionary<Values, Player> books;
        private Deck stock;
        private int round = 1;

        public bool GameInProgress { get; private set; }
        public bool GameNotStarted => !GameInProgress;
        public string PlayerName { get; set; }
        public ObservableCollection<string> Hand { get; private set; }
        public string Books => DescribeBooks();
        public string GameProgress { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Game()
        {
            PlayerName = "Mason";
            Hand = new ObservableCollection<string>();
            ResetGame();
        }

        public void StartGame()
        {
            ClearProgress();
            GameInProgress = true;
            OnPropertyChanged("GameInProgress");
            OnPropertyChanged("GameNotStarted");
            Random random = new Random();
            players = new List<Player>();
            players.Add(new Player(PlayerName, random, this));
            players.Add(new Player("Andrea", random, this));
            players.Add(new Player("Matt", random, this));
            Deal();
            players[0].SortHand();
            UpdateHand();
        }

        private void UpdateHand()
        {
            Hand.Clear();
            foreach (string cardName in GetPlayerCardNames())
            {
                Hand.Add(cardName);
            }
            if (!GameInProgress)
            {
                AddProgress(DescribePlayerHands());
            }
            OnPropertyChanged("BookList");
            OnPropertyChanged("Cards");
        }

        private void ResetGame()
        {
            GameInProgress = false;
            OnPropertyChanged("GameInProgress");
            OnPropertyChanged("GameNotStarted");
            books = new Dictionary<Values, Player>();
            stock = new Deck();
            Hand.Clear();
        }

        public void AddProgress(string line)
        {
            /*  this puts the first log entries first but the scrolling doesn't work automagically so TODO
             *  if (string.IsNullOrEmpty(GameProgress))
            {
                GameProgress = line;
            }
            else
            {
                GameProgress = GameProgress + Environment.NewLine + line;
            }*/

            GameProgress = line + Environment.NewLine + GameProgress;
            OnPropertyChanged("GameProgress");           
        }

        public void ClearProgress()
        {
            GameProgress = string.Empty;
            OnPropertyChanged("GameProgress");
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Deal()
        {
            stock.Shuffle();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < players.Count; j++)
                {
                    players[j].TakeCard(stock.Deal());
                }
            }
        }

        public void PlayOneRound(int selectedPlayerCard)
        {
            // go through all of the players and call
            // each one's AskForACard() methods, starting with the human player (who's
            // at index zero in the Players list—make sure he asks for the selected
            // card's value). Then call PullOutBooks()—if it returns true, then the
            // player ran out of cards and needs to draw a new hand. After all the players
            // have gone, sort the human player's hand (so it looks nice in the form).
            // Then check the stock to see if it's out of cards. If it is, reset the
            // TextBox on the form to say, "The stock is out of cards. Game over!" and return
            // true. Otherwise, the game isn't over yet, so return false.

            Card selectedCard = players[0].Peek(selectedPlayerCard);
            Values selectedValue = selectedCard.Value;
            int drawCount = 0;

            for (int i = 0; i < players.Count; i++)
            {
                // edge case, if the player lost their last card to another player they will have to draw to play
                if (players[i].CardCount == 0)
                {
                    players[i].TakeCard(stock.Deal());
                    AddProgress($"{players[i].Name} draws a card to keep playing.");
                }

                // 0 is the human player so the method is called with their selected card, otherwise the AI method is called which is the same but generates a random value
                if (i == 0)
                {
                    players[i].AskForACard(players, i, stock, selectedValue);
                }
                else
                {
                    players[i].AskForACard(players, i, stock);
                }

                // TODO should we end the for loop here to avoid extra processing?
                //

                // the card exchange is done so now we have to pull out books and draw cards
                //
                // don't check for books if there aren't at least 4 cards in hand
                if (players[i].CardCount > 3)
                {
                    // executes method, which returns true if the player is out of cards as a result of pulling out books
                    if (PullOutBooks(players[i]))
                    {
                        for (int j = 0; j < 5 && stock.Count > 0; j++)
                        {
                            players[i].TakeCard(stock.Deal());
                            drawCount++;
                        }
                        if (drawCount == 1)
                        {
                            AddProgress($"{players[i].Name} draws the last card.");
                        }
                        else
                        {
                            AddProgress($"{players[i].Name} draws {drawCount} cards.");
                        }
                    }
                    // books may or may not have been pulled out by here and the player should have cards
                }

                players[0].SortHand();
                if (stock.Count == 0)
                {
                    AddProgress($"The stock is out of cards, game over!");
                    //return true;
                }
            }

            UpdateHand();
            AddProgress($"--------------- End of round {round} ---------------");
            round++;
            //return false;
        }

        public bool PullOutBooks(Player player)
        {
            // Pull out a player's books. Return true if the player ran out of cards, otherwise
            // return false. Each book is added to the Books dictionary. A player runs out of
            // cards when he’'s used all of his cards to make books—and he wins the game

            IEnumerable<Values> pulledBook = player.PullOutBooks();

            // is a foreach loop for a Dictionary of one a target for improvement?  would have to extract the value otherwise 
            foreach (Values value in pulledBook)                 
            {
                books.Add(value, player);
                AddProgress($"{player.Name} scored a book of {Card.Plural(value)}");
            }

            if (player.CardCount == 0)
            {
                return true;
            }
            return false;
        }

        public string DescribeBooks()
        {
            // Return a long string that describes everyone's books by looking at the Books
            // dictionary: "Joe has a book of sixes. (line break) Ed has a book of Aces."
            string bookString = "";
            foreach (Values value in books.Keys)
            {
                bookString += $"{books[value].Name} has a book of {Card.Plural(value)}.";
            }
            return bookString;
        }

        public string GetWinnerName()
        {
            // This method is called at the end of the game. It uses its own dictionary
            // (Dictionary<string, int> winners) to keep track of how many books each player
            // ended up with in the books dictionary. First it uses a foreach loop
            // on books.Keys—foreach (Values value in books.Keys)—to populate
            // its winners dictionary with the number of books each player ended up with.
            // Then it loops through that dictionary to find the largest number of books
            // any winner has. And finally it makes one last pass through winners to come
            // up with a list of winners in a string ("Joe and Ed"). If there's one winner,
            // it returns a string like this: "Ed with 3 books". Otherwise, it returns a
            // string like this: "A tie between Joe and Bob with 2 books."

            Dictionary<string, int> winners = new Dictionary<string, int>();
            int theMostBooks = 0;
            string winnerString = "";

            foreach (Values value in books.Keys)
            {
                // initialize the key because it's throwing exceptions otherwise!
                if (!winners.ContainsKey(books[value].Name))
                {
                    winners.Add(books[value].Name, 0);
                }
                // read Name property from player value in the key
                // assign existing value on winner key to int count
                // reassign the value after incrementing it
                string name = books[value].Name;
                winners[name]++;                   
            }

            foreach (string name in winners.Keys)
            {
                if (winners[name] > theMostBooks)
                {
                    theMostBooks = winners[name];
                }
            }

            foreach (string name in winners.Keys)
            {
                if (winners[name] == theMostBooks)
                {
                    if (string.IsNullOrEmpty(winnerString))
                    {
                        winnerString = name;
                    }
                    else
                    {
                        winnerString += $" and {name}";
                    }
                }
            }
            return $"{winnerString} with {theMostBooks} books.";
        }

        public IEnumerable<string> GetPlayerCardNames()
        {
            return players[0].GetCardNames();
        }

        internal string DescribePlayerHands()
        {
            string description = "";
            for (int i = 0; i < players.Count; i++) {
                description += $"{players[i].Name} has {players[i].CardCount}";
                if (players[i].CardCount == 1)
                {
                    description += $" card.{Environment.NewLine}";
                }
                else
                {
                    description += $" cards.{Environment.NewLine}";
                }
            }
            description += $"The stock has {stock.Count} cards left.";
            return description;
        }
    }
}