using NUnit.Framework;
using RatLab.Core;

namespace RatLab.Core.Tests
{
    public sealed class OwnerTests
    {
        [Test]
        public void OwnerDefinesEmptyPlayerAndEnemyStates()
        {
            Assert.AreEqual(Owner.None, (Owner)0);
            Assert.AreEqual(Owner.Player, (Owner)1);
            Assert.AreEqual(Owner.Enemy, (Owner)2);
        }
    }
}
