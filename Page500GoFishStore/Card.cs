using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Page500GoFishStore
{
    class Card : IComparable<Card>
    {
        public Suits Suit { get; }
        public Values Value { get; }
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        public Card(Suits suit, Values value)
        {
            Suit = suit;
            Value = value;
            _name = $"{value} of {suit}";
        }

        public int CompareTo(Card other)
        {
            if (Value > other.Value)
            {
                return 1;
            }
            if (Value < other.Value)
            {
                return -1;
            }
            return 0;
        }

        public override string ToString()
        {
            return Name;
        }

        public static string Plural(Values value)
        {
            if (value == Values.Six)
            {
                return "Sixes";
            }
            return $"{value.ToString()}s";
        }
    }
}