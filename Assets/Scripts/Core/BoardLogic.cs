using System;
using System.Collections.Generic;

namespace RatLab.Core
{
    public static class BoardLogic
    {
        public static IReadOnlyList<int> PlaceCard(Board board, int cellIndex, Card card, Owner owner)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            if (owner == Owner.None)
            {
                throw new ArgumentException("A placed card must have an owner.", nameof(owner));
            }

            if (!board.IsEmpty(cellIndex))
            {
                throw new InvalidOperationException("A card can only be placed in an empty cell.");
            }

            board.SetCell(cellIndex, card, owner);

            List<int> flippedCellIndices = new List<int>(4);
            int row = cellIndex / Board.Size;
            int column = cellIndex % Board.Size;

            if (row > 0)
            {
                TryFlipNeighbor(
                    board,
                    cellIndex - Board.Size,
                    owner,
                    card.North,
                    card => card.South,
                    flippedCellIndices);
            }

            if (column < Board.Size - 1)
            {
                TryFlipNeighbor(
                    board,
                    cellIndex + 1,
                    owner,
                    card.East,
                    card => card.West,
                    flippedCellIndices);
            }

            if (row < Board.Size - 1)
            {
                TryFlipNeighbor(
                    board,
                    cellIndex + Board.Size,
                    owner,
                    card.South,
                    card => card.North,
                    flippedCellIndices);
            }

            if (column > 0)
            {
                TryFlipNeighbor(
                    board,
                    cellIndex - 1,
                    owner,
                    card.West,
                    card => card.East,
                    flippedCellIndices);
            }

            return flippedCellIndices;
        }

        public static EnemyMove ChooseBestMove(
            Board board,
            IReadOnlyList<Card> availableCards,
            Owner owner,
            Random random)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (availableCards == null)
            {
                throw new ArgumentNullException(nameof(availableCards));
            }

            if (owner == Owner.None)
            {
                throw new ArgumentException("A move must have a non-empty owner.", nameof(owner));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            List<EnemyMove> bestMoves = new List<EnemyMove>();
            int bestFlippedCount = -1;

            foreach (Card card in availableCards)
            {
                if (card == null)
                {
                    throw new ArgumentException("Available cards cannot be null.", nameof(availableCards));
                }

                foreach (int cellIndex in board.GetEmptyCellIndices())
                {
                    Board simulation = board.Clone();
                    int flippedCount = PlaceCard(simulation, cellIndex, card, owner).Count;
                    EnemyMove move = new EnemyMove(card, cellIndex, flippedCount);

                    if (flippedCount > bestFlippedCount)
                    {
                        bestFlippedCount = flippedCount;
                        bestMoves.Clear();
                        bestMoves.Add(move);
                    }
                    else if (flippedCount == bestFlippedCount)
                    {
                        bestMoves.Add(move);
                    }
                }
            }

            if (bestMoves.Count == 0)
            {
                return null;
            }

            return bestMoves[random.Next(bestMoves.Count)];
        }

        public static Owner GetWinner(Board board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (!board.IsFull)
            {
                throw new InvalidOperationException("The winner can only be determined when the board is full.");
            }

            int playerCount = board.CountOwnedBy(Owner.Player);
            int enemyCount = board.CountOwnedBy(Owner.Enemy);

            if (playerCount == enemyCount)
            {
                return Owner.None;
            }

            return playerCount > enemyCount ? Owner.Player : Owner.Enemy;
        }

        private static void TryFlipNeighbor(
            Board board,
            int neighborIndex,
            Owner placingOwner,
            int attackingValue,
            Func<Card, int> defendingValueSelector,
            ICollection<int> flippedCellIndices)
        {
            Card neighborCard = board.GetCard(neighborIndex);
            Owner neighborOwner = board.GetOwner(neighborIndex);

            if (neighborCard == null || !IsOpponent(placingOwner, neighborOwner))
            {
                return;
            }

            if (attackingValue > defendingValueSelector(neighborCard))
            {
                board.ChangeOwner(neighborIndex, placingOwner);
                flippedCellIndices.Add(neighborIndex);
            }
        }

        private static bool IsOpponent(Owner placingOwner, Owner neighborOwner)
        {
            return placingOwner != Owner.None
                && neighborOwner != Owner.None
                && placingOwner != neighborOwner;
        }
    }
}
