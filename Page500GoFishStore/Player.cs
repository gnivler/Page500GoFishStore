﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Page500GoFishStore
{
    class Player
    {
        private string name;
        public string Name => name;
        private Random random;
        private Deck cards;
        private Game game;
        public int CardCount => cards.Count;

        public void TakeCard(Card card) => cards.Add(card);
        public IEnumerable<string> GetCardNames() => cards.GetCardNames();
        public Card Peek(int cardNumber) => cards.Peek(cardNumber);
        public void SortHand() => cards.SortByValue();

        public Player(string name, Random random, Game game)
        {
            this.name = name;
            this.random = random;
            this.game = game;
            cards = new Deck(new Card[] { });
            game.AddProgress($"{this.name} has joined the game.");
        }

        public IEnumerable<Values> PullOutBooks()
        {
            List<Values> books = new List<Values>();
            for (int i = 1; i <= 13; i++)
            {
                Values value = (Values)i;
                int howMany = 0;
                for (int card = 0; card < cards.Count; card++)
                {
                    if (cards.Peek(card).Value == value)
                    {
                        {
                            howMany++;
                        }
                    }
                    if (howMany == 4)
                    {
                        books.Add(value);
                        cards.PullOutValues(value);
                    }
                }
            }
            return books;
        }

        public Values GetRandomValue()
        {
            Card card = cards.Peek(random.Next(0, cards.Count));
            return card.Value;
        }

        public Deck DoYouHaveAny(Values value)
        {
            // This is where an opponent asks if I have any cards of a certain value
            // Use Deck.PullOutValues() to pull out the values. Add a line to the TextBox
            // that says, "Joe has 3 sixes"—use the new Card.Plural() static method
            Deck resultingDeck = cards.PullOutValues(value);
            string valueString;
            if (resultingDeck.Count != 1)
            {
                valueString = Card.Plural(value);
            }
            else
            {
                valueString = value.ToString();
            }
            game.AddProgress($"{Name} has {resultingDeck.Count} {valueString}");
            return resultingDeck;
        }

        public void AskForACard(List<Player> players, int myIndex, Deck stock)
        {
            // Here's an overloaded version of AskForACard()—choose a random value
            // from the deck using GetRandomValue() and ask for it using AskForACard()
            AskForACard(players, myIndex, stock, GetRandomValue());
        }

        public void AskForACard(List<Player> players, int myIndex, Deck stock, Values value)
        {
            // Ask the other players for a value. First add a line to the TextBox: "Joe asks
            // if anyone has a Queen". Then go through the list of players that was passed in
            // as a parameter and ask each player if he has any of the value (using his
            // DoYouHaveAny() method). He'll pass you a deck of cards—add them to my deck.
            // Keep track of how many cards were added. If there weren't any, you'll need
            // to deal yourself a card from the stock (which was also passed as a parameter),
            // and you'll have to add a line to the TextBox: "Joe had to draw from the stock"

            Deck resultingDeck;
            int numCards = 0;
            string pluralName = Card.Plural(value);
            bool goFish = true;

            game.AddProgress($"{Name} asks if anyone has a {value}.");
            for (int i = 0 ; i < players.Count; i++)
            {
                if (players[i].Name != Name)
                {
                    resultingDeck = players[i].DoYouHaveAny(value);
                    numCards = resultingDeck.Count;
                    if (numCards > 0)
                    {
                        goFish = false;
                        for (int j = 0; j < resultingDeck.Count; j++)
                        {
                            cards.Add(resultingDeck.Deal());
                        }
                    }
                }
            }

            if (goFish)
            {
                cards.Add(stock.Deal());
                game.AddProgress($"{Name} had to Go Fish.");
            }
        }
    }
}