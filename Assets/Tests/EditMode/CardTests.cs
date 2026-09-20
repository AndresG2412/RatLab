using System;
using NUnit.Framework;
using RatLab.Core;

namespace RatLab.Core.Tests
{
    public sealed class CardTests
    {
        [Test]
        public void CardStoresNorthEastSouthAndWestValues()
        {
            Card card = new Card(1, 5, 9, 3);

            Assert.AreEqual(1, card.North);
            Assert.AreEqual(5, card.East);
            Assert.AreEqual(9, card.South);
            Assert.AreEqual(3, card.West);
        }

        [TestCase(0, 5, 5, 5)]
        [TestCase(5, 0, 5, 5)]
        [TestCase(5, 5, 0, 5)]
        [TestCase(5, 5, 5, 0)]
        [TestCase(10, 5, 5, 5)]
        [TestCase(5, 10, 5, 5)]
        [TestCase(5, 5, 10, 5)]
        [TestCase(5, 5, 5, 10)]
        public void CardRejectsValuesOutsideOneToNine(int north, int east, int south, int west)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Card(north, east, south, west));
        }

        [Test]
        public void CreateRandomGeneratesValuesBetweenOneAndNine()
        {
            Random random = new Random(12345);

            for (int index = 0; index < 100; index++)
            {
                Card card = Card.CreateRandom(random);

                Assert.That(card.North, Is.InRange(1, 9));
                Assert.That(card.East, Is.InRange(1, 9));
                Assert.That(card.South, Is.InRange(1, 9));
                Assert.That(card.West, Is.InRange(1, 9));
            }
        }

        [Test]
        public void CreateRandomRejectsNullRandomSource()
        {
            Assert.Throws<ArgumentNullException>(() => Card.CreateRandom(null));
        }
    }
}
