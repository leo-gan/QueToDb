using NUnit.Framework;
using QueToDb.Queues.AzureRedis;

namespace QueToDb.Tests.Functional.Queues
{
    [TestFixture]
    public class AzureRedis
    {
        #region Setup/Teardown

        [SetUp]
        public void Init()
        {
            var isInitialized = _w.Initialize();
            Assert.IsTrue(isInitialized);
            isInitialized = _r.Initialize();
            Assert.IsTrue(isInitialized);
            _r.CleanUpAllMessages();
        }

        [TearDown]
        public void Dispose()
        {
            _w.Dispose();
            _r.Dispose();
        }

        #endregion

        private const string Transport = "AzureRedis";
        private readonly Writer _w = new Writer();
        private readonly Reader _r = new Reader();


        [Test]
        [TestCase(100, '*')]
        [TestCase(1000, '*')]
        public void ReadWrite1Msg(int msgBodySizeChars, char msgBodyFiller)
        {
            WriteAndRead.ReadWrite1Msg(_w, _r, Transport, msgBodySizeChars, msgBodyFiller);
        }

        [Test]
        [TestCase(100, '*', 10)]
        [TestCase(100, '*', 30)]
        [TestCase(1000, '*', 100)]
        [TestCase(1000, '*', 300)]
        public void ReadWriteManyMsgInBatch(int msgBodySizeChars, char msgBodyFiller, int msgNumber)
        {
            WriteAndRead.ReadWriteManyMsgInBatch(_w, _r, Transport, msgBodySizeChars, msgBodyFiller, msgNumber);
        }

        [Test]
        [TestCase(100, '*', 10)]
        [TestCase(100, '*', 30)]
        [TestCase(1000, '*', 100)]
        [TestCase(1000, '*', 300)]
        public void ReadWriteManyMsgInSequence(int msgBodySizeChars, char msgBodyFiller, int msgNumber)
        {
            WriteAndRead.ReadWriteManyMsgInSequence(_w, _r, Transport, msgBodySizeChars, msgBodyFiller, msgNumber);
        }
    }
}