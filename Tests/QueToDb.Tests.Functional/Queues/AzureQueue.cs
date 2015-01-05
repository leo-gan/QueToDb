using NUnit.Framework;
using QueToDb.Queues.AzureQueue;

namespace QueToDb.Tests.Functional.Queues
{
    [TestFixture]
    public class AzureQueue

    {
        #region Setup/Teardown

        [SetUp]
        public void Init()
        {
            var isInitialized = _r.Initialize();
            Assert.IsTrue(isInitialized);
            _r.CleanUp(); // move to the end of the queue
            isInitialized = _w.Initialize();
            Assert.IsTrue(isInitialized);
         }

        [TearDown]
        public void Dispose()
        {
            _w.Dispose();
            _r.Dispose();
        }

        #endregion

        private const string Transport = "AzureQueue";
        private readonly Writer _w = new Writer();
        private readonly Reader _r = new Reader();


        [Test]
        [TestCase(100, '*')]
        public void ReadWrite1Msg(int msgBodySizeChars, char msgBodyFiller)
        {
            WriteAndRead.ReadWrite1Msg(_w, _r, Transport, msgBodySizeChars, msgBodyFiller);
        }

        [Test]
        [TestCase(10, '*', 10)]
        [TestCase(10, '*', 100)]
        [TestCase(10, '*', 300)]
        [TestCase(100, '*', 100)]
        [TestCase(100, '*', 300)]
        [TestCase(1000, '*', 100)]
        [TestCase(1000, '*', 300)]
        public void ReadWriteManyMsgInBatch(int msgBodySizeChars, char msgBodyFiller, int msgNumber)
        {
            WriteAndRead.ReadWriteManyMsgInBatch(_w, _r, Transport, msgBodySizeChars, msgBodyFiller, msgNumber);
        }

        [Test]
        [TestCase(10, '*', 10)]
        [TestCase(10, '*', 100)]
        [TestCase(10, '*', 300)]
        [TestCase(100, '*', 100)]
        [TestCase(100, '*', 300)]
        [TestCase(1000, '*', 100)]
        [TestCase(1000, '*', 300)]
        public void ReadWriteManyMsgInSequence(int msgBodySizeChars, char msgBodyFiller, int msgNumber)
        {
            WriteAndRead.ReadWriteManyMsgInSequence(_w, _r, Transport, msgBodySizeChars, msgBodyFiller, msgNumber);
        }
    }
}