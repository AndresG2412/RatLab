using System;

namespace RatLab.Core
{
    public sealed class Card
    {
        public const int MinimumValue = 1;
        public const int MaximumValue = 9;

        public Card(int north, int east, int south, int west)
        {
            North = ValidateValue(nameof(north), north);
            East = ValidateValue(nameof(east), east);
            South = ValidateValue(nameof(south), south);
            West = ValidateValue(nameof(west), west);
        }

        public int North { get; }
        public int East { get; }
        public int South { get; }
        public int West { get; }

        public static Card CreateRandom(Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            return new Card(
                random.Next(MinimumValue, MaximumValue + 1),
                random.Next(MinimumValue, MaximumValue + 1),
                random.Next(MinimumValue, MaximumValue + 1),
                random.Next(MinimumValue, MaximumValue + 1));
        }

        private static int ValidateValue(string parameterName, int value)
        {
            if (value < MinimumValue || value > MaximumValue)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Card values must be between 1 and 9.");
            }

            return value;
        }
    }
}
