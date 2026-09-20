using System;
using NUnit.Framework;
using RatLab.Core;

namespace RatLab.Core.Tests
{
    public sealed class BoardLogicTests
    {
        [Test]
        public void PlacingCardFlipsEachRivalNeighborWhenTheMatchingSideIsStrictlyGreater()
        {
            Board board = new Board();
            BoardLogic.PlaceCard(board, 1, new Card(1, 1, 4, 1), Owner.Enemy);
            BoardLogic.PlaceCard(board, 5, new Card(1, 1, 1, 5), Owner.Enemy);
            BoardLogic.PlaceCard(board, 7, new Card(6, 1, 1, 1), Owner.Enemy);
            BoardLogic.PlaceCard(board, 3, new Card(1, 7, 1, 1), Owner.Enemy);

            var flipped = BoardLogic.PlaceCard(
                board,
                4,
                new Card(5, 6, 7, 8),
                Owner.Player);

            CollectionAssert.AreEquivalent(new[] { 1, 5, 7, 3 }, flipped);
            Assert.AreEqual(Owner.Player, board.GetOwner(1));
            Assert.AreEqual(Owner.Player, board.GetOwner(5));
            Assert.AreEqual(Owner.Player, board.GetOwner(7));
            Assert.AreEqual(Owner.Player, board.GetOwner(3));
        }

        [Test]
        public void PlacingCardDoesNotFlipEqualLowerOrOwnNeighbors()
        {
            Board board = new Board();
            BoardLogic.PlaceCard(board, 1, new Card(1, 1, 5, 1), Owner.Enemy);
            BoardLogic.PlaceCard(board, 5, new Card(1, 1, 1, 7), Owner.Enemy);
            BoardLogic.PlaceCard(board, 7, new Card(7, 1, 1, 1), Owner.Enemy);
            BoardLogic.PlaceCard(board, 3, new Card(1, 1, 1, 1), Owner.Player);

            var flipped = BoardLogic.PlaceCard(
                board,
                4,
                new Card(5, 6, 8, 9),
                Owner.Player);

            CollectionAssert.AreEquivalent(new[] { 7 }, flipped);
            Assert.AreEqual(Owner.Enemy, board.GetOwner(1));
            Assert.AreEqual(Owner.Enemy, board.GetOwner(5));
            Assert.AreEqual(Owner.Player, board.GetOwner(3));
            Assert.AreEqual(Owner.Player, board.GetOwner(7));
        }

        [Test]
        public void EmptyNeighborsAreIgnored()
        {
            Board board = new Board();

            var flipped = BoardLogic.PlaceCard(
                board,
                0,
                new Card(9, 9, 9, 9),
                Owner.Player);

            Assert.IsEmpty(flipped);
            Assert.AreEqual(Owner.Player, board.GetOwner(0));
        }

        [Test]
        public void OnlyTheNewlyPlacedCardAttacks()
        {
            Board board = new Board();
            BoardLogic.PlaceCard(board, 0, new Card(1, 1, 1, 1), Owner.Enemy);
            BoardLogic.PlaceCard(board, 1, new Card(9, 1, 1, 1), Owner.Enemy);

            BoardLogic.PlaceCard(board, 4, new Card(2, 1, 1, 1), Owner.Player);

            Assert.AreEqual(Owner.Player, board.GetOwner(1));
            Assert.AreEqual(Owner.Enemy, board.GetOwner(0));
        }

        [Test]
        public void CannotPlaceCardOnAnOccupiedCell()
        {
            Board board = new Board();
            BoardLogic.PlaceCard(board, 0, new Card(1, 1, 1, 1), Owner.Player);

            Assert.Throws<InvalidOperationException>(
                () => BoardLogic.PlaceCard(board, 0, new Card(2, 2, 2, 2), Owner.Enemy));
        }

        [Test]
        public void CannotPlaceCardWithoutAnOwner()
        {
            Board board = new Board();

            Assert.Throws<ArgumentException>(
                () => BoardLogic.PlaceCard(board, 0, new Card(1, 1, 1, 1), Owner.None));
        }

        [Test]
        public void WinnerIsUnavailableBeforeTheBoardIsFull()
        {
            Assert.Throws<InvalidOperationException>(() => BoardLogic.GetWinner(new Board()));
        }

        [Test]
        public void PlayerWinsWithFiveCardsAgainstFourWhenTheBoardIsFull()
        {
            Board board = new Board();

            for (int index = 0; index < Board.CellCount; index++)
            {
                Owner owner = index % 2 == 0 ? Owner.Player : Owner.Enemy;
                BoardLogic.PlaceCard(board, index, new Card(1, 1, 1, 1), owner);
            }

            Assert.IsTrue(board.IsFull);
            Assert.AreEqual(5, board.CountOwnedBy(Owner.Player));
            Assert.AreEqual(4, board.CountOwnedBy(Owner.Enemy));
            Assert.AreEqual(Owner.Player, BoardLogic.GetWinner(board));
        }

        [Test]
        public void ChooseBestMoveSelectsTheCardAndCellThatFlipTheMostCards()
        {
            Board board = new Board();
            BoardLogic.PlaceCard(board, 1, new Card(1, 1, 1, 1), Owner.Player);

            Card winningCard = new Card(1, 9, 1, 1);
            Card otherCard = new Card(1, 1, 1, 1);
            EnemyMove move = BoardLogic.ChooseBestMove(
                board,
                new[] { winningCard, otherCard },
                Owner.Enemy,
                new Random(10));

            Assert.AreSame(winningCard, move.Card);
            Assert.AreEqual(0, move.CellIndex);
            Assert.AreEqual(1, move.FlippedCount);
        }

        [Test]
        public void ChooseBestMoveUsesTheRandomSourceToBreakTies()
        {
            Board board = new Board();
            Card firstCard = new Card(1, 1, 1, 1);
            Card secondCard = new Card(2, 2, 2, 2);

            EnemyMove firstChoice = BoardLogic.ChooseBestMove(
                board,
                new[] { firstCard, secondCard },
                Owner.Enemy,
                new Random(123));
            EnemyMove repeatedChoice = BoardLogic.ChooseBestMove(
                board,
                new[] { firstCard, secondCard },
                Owner.Enemy,
                new Random(123));

            Assert.AreSame(firstChoice.Card, repeatedChoice.Card);
            Assert.AreEqual(firstChoice.CellIndex, repeatedChoice.CellIndex);
            Assert.AreEqual(0, firstChoice.FlippedCount);
        }
    }
}
