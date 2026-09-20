using System;
using System.Collections.Generic;

namespace RatLab.Core
{
    public sealed class Board
    {
        public const int Size = 3;
        public const int CellCount = Size * Size;

        private readonly Card[] cards = new Card[CellCount];
        private readonly Owner[] owners = new Owner[CellCount];

        public Board()
        {
            for (int index = 0; index < CellCount; index++)
            {
                owners[index] = Owner.None;
            }
        }

        public int OccupiedCount
        {
            get
            {
                int occupiedCount = 0;

                for (int index = 0; index < CellCount; index++)
                {
                    if (cards[index] != null)
                    {
                        occupiedCount++;
                    }
                }

                return occupiedCount;
            }
        }

        public bool IsFull => OccupiedCount == CellCount;

        public bool IsValidCellIndex(int cellIndex)
        {
            return cellIndex >= 0 && cellIndex < CellCount;
        }

        public bool IsEmpty(int cellIndex)
        {
            ValidateCellIndex(cellIndex);
            return cards[cellIndex] == null;
        }

        public Card GetCard(int cellIndex)
        {
            ValidateCellIndex(cellIndex);
            return cards[cellIndex];
        }

        public Owner GetOwner(int cellIndex)
        {
            ValidateCellIndex(cellIndex);
            return owners[cellIndex];
        }

        public IEnumerable<int> GetEmptyCellIndices()
        {
            for (int index = 0; index < CellCount; index++)
            {
                if (cards[index] == null)
                {
                    yield return index;
                }
            }
        }

        public int CountOwnedBy(Owner owner)
        {
            int count = 0;

            for (int index = 0; index < CellCount; index++)
            {
                if (owners[index] == owner)
                {
                    count++;
                }
            }

            return count;
        }

        public Board Clone()
        {
            Board clone = new Board();

            for (int index = 0; index < CellCount; index++)
            {
                if (cards[index] != null)
                {
                    clone.SetCell(index, cards[index], owners[index]);
                }
            }

            return clone;
        }

        internal void SetCell(int cellIndex, Card card, Owner owner)
        {
            ValidateCellIndex(cellIndex);

            if (card == null)
            {
                if (owner != Owner.None)
                {
                    throw new ArgumentException(
                        "An empty cell must have Owner.None.",
                        nameof(owner));
                }

                cards[cellIndex] = null;
                owners[cellIndex] = Owner.None;
                return;
            }

            if (owner == Owner.None)
            {
                throw new ArgumentException(
                    "A card must have a non-empty owner.",
                    nameof(owner));
            }

            cards[cellIndex] = card;
            owners[cellIndex] = owner;
        }

        internal void ChangeOwner(int cellIndex, Owner owner)
        {
            ValidateCellIndex(cellIndex);

            if (cards[cellIndex] == null)
            {
                throw new InvalidOperationException("An empty cell cannot change owner.");
            }

            if (owner == Owner.None)
            {
                throw new ArgumentException(
                    "An occupied cell must have a non-empty owner.",
                    nameof(owner));
            }

            owners[cellIndex] = owner;
        }

        private void ValidateCellIndex(int cellIndex)
        {
            if (!IsValidCellIndex(cellIndex))
            {
                throw new ArgumentOutOfRangeException(nameof(cellIndex), cellIndex, "Cell index must be between 0 and 8.");
            }
        }
    }
}
