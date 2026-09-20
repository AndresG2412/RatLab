namespace RatLab.Core
{
    public sealed class EnemyMove
    {
        public EnemyMove(Card card, int cellIndex, int flippedCount)
        {
            Card = card;
            CellIndex = cellIndex;
            FlippedCount = flippedCount;
        }

        public Card Card { get; }
        public int CellIndex { get; }
        public int FlippedCount { get; }
    }
}
