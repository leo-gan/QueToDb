using NUnit.Framework;
using QueToDb.Queues.Redis;

namespace QueToDb.Tests.Functional.Queues
{
    [TestFixture]
    public class Redis
    {
        #region Setup/Teardown

        [SetUp]
        public void Init()
        {
            _w.Initialize();
            _r.Initialize();
        }

        [TearDown]
        public void Dispose()
        {
            _w.Dispose();
            _r.Dispose();
        }

        #endregion

        private const string Transport = "Redis";
        private readonly Writer _w = new Writer();
        private readonly Reader _r = new Reader();


        [Test]
        [TestCase(100, '*')]
        public void ReadWrite1Msg(int msgBodySizeChars, char msgBodyFiller)
        {
            WriteAndRead.ReadWrite1Msg(_w, _r, Transport, msgBodySizeChars, msgBodyFiller);
        }

        [Test]
        [TestCase(100, '*', 10000)]
        [TestCase(100, '*', 30000)]
        [TestCase(1000, '*', 1000)]
        [TestCase(1000, '*', 3000)]
        public void ReadWriteManyMsgInBatch(int msgBodySizeChars, char msgBodyFiller, int msgNumber)
        {
            WriteAndRead.ReadWriteManyMsgInBatch(_w, _r, Transport, msgBodySizeChars, msgBodyFiller, msgNumber);
        }

        [Test]
        [TestCase(100, '*', 10000)]
        [TestCase(100, '*', 30000)]
        [TestCase(1000, '*', 1000)]
        [TestCase(1000, '*', 3000)]
        public void ReadWriteManyMsgInSequence(int msgBodySizeChars, char msgBodyFiller, int msgNumber)
        {
            WriteAndRead.ReadWriteManyMsgInSequence(_w, _r, Transport, msgBodySizeChars, msgBodyFiller, msgNumber);
        }
    }
}