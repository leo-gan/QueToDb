﻿using NUnit.Framework;
using QueToDb.Queues.NetMq;

namespace QueToDb.Tests.Functional.Queues
{
    [TestFixture]
    public class NetMq
    {
        #region Setup/Teardown

        [SetUp]
        public void Init()
        {
            var isInitialized = _w.Initialize();
            Assert.IsTrue(isInitialized);
            isInitialized = _r.Initialize();
            Assert.IsTrue(isInitialized);
        }

        [TearDown]
        public void Dispose()
        {
            //_r.Dispose();
            //_w.Dispose();
        }

        #endregion

        private const string Transport = "NetMq";
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