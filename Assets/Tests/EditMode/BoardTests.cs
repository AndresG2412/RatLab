using System;
using System.Linq;
using NUnit.Framework;
using RatLab.Core;

namespace RatLab.Core.Tests
{
    public sealed class BoardTests
    {
        [Test]
        public void BoardStartsAsAnEmptyThreeByThreeBoard()
        {
            Board board = new Board();

            Assert.AreEqual(3, Board.Size);
            Assert.AreEqual(9, Board.CellCount);
            Assert.AreEqual(0, board.OccupiedCount);
            Assert.IsFalse(board.IsFull);
            Assert.AreEqual(9, board.GetEmptyCellIndices().Count());

            for (int index = 0; index < Board.CellCount; index++)
            {
                Assert.IsTrue(board.IsEmpty(index));
                Assert.IsNull(board.GetCard(index));
                Assert.AreEqual(Owner.None, board.GetOwner(index));
            }
        }

        [Test]
        public void BoardRejectsCellIndicesOutsideTheThreeByThreeBoard()
        {
            Board board = new Board();

            Assert.IsFalse(board.IsValidCellIndex(-1));
            Assert.IsFalse(board.IsValidCellIndex(Board.CellCount));
            Assert.Throws<ArgumentOutOfRangeException>(() => board.IsEmpty(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => board.GetCard(Board.CellCount));
            Assert.Throws<ArgumentOutOfRangeException>(() => board.GetOwner(Board.CellCount));
        }

        [Test]
        public void CloneCopiesBoardStateWithoutSharingBoardStorage()
        {
            Board board = new Board();
            Card card = new Card(1, 2, 3, 4);
            BoardLogic.PlaceCard(board, 0, card, Owner.Player);

            Board clone = board.Clone();
            BoardLogic.PlaceCard(clone, 1, new Card(4, 3, 2, 1), Owner.Enemy);

            Assert.AreSame(card, board.GetCard(0));
            Assert.AreEqual(Owner.Player, board.GetOwner(0));
            Assert.IsTrue(board.IsEmpty(1));
            Assert.IsFalse(clone.IsEmpty(0));
            Assert.IsFalse(clone.IsEmpty(1));
        }

        [Test]
        public void CountOwnedByReportsPlayerEnemyAndEmptyCells()
        {
            Board board = new Board();
            BoardLogic.PlaceCard(board, 0, new Card(1, 1, 1, 1), Owner.Player);
            BoardLogic.PlaceCard(board, 1, new Card(2, 2, 2, 2), Owner.Enemy);

            Assert.AreEqual(1, board.CountOwnedBy(Owner.Player));
            Assert.AreEqual(1, board.CountOwnedBy(Owner.Enemy));
            Assert.AreEqual(7, board.CountOwnedBy(Owner.None));
        }
    }
}
