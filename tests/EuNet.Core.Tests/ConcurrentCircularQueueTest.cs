﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EuNet.Core.Tests
{
    public class ConcurrentCircularQueueTest
    {
        public class DataClass
        {
            public int Value;
        }

        private List<DataClass> _testList;

        [SetUp]
        public void Setup()
        {
            _testList = new List<DataClass>();
            for(int i=0; i<5; i++)
            {
                _testList.Add(new DataClass() { Value = i });
            }
        }

        private void CheckList(List<DataClass> list, int count)
        {
            list.Sort((x, y) =>
            {
                return x.Value.CompareTo(y.Value);
            });

            for (int i = 0; i < count; ++i)
            {
                Assert.AreSame(_testList[i], list[i]);
            }
        }

        [Test]
        public void BasicTest()
        {
            var queue = new ConcurrentCircularQueue<DataClass>(_testList.Count);

            foreach (var item in _testList)
                queue.Enqueue(item);

            Assert.AreEqual(_testList.Count, queue.Length);

            List<DataClass> _dequeueList = new List<DataClass>(_testList.Count);
            foreach (var item in _testList)
                _dequeueList.Add(queue.Dequeue());

            Assert.AreEqual(0, queue.Length);

            CheckList(_dequeueList, _dequeueList.Count);

            for(int i=0; i<_testList.Count * 3; ++i)
            {
                var item = _testList[i % _testList.Count];
                queue.Enqueue(item);
                var result = queue.Dequeue();

                Assert.AreSame(item, result);
            }
        }

        [Test]
        public void ArrayTest()
        {
            var queue = new ConcurrentCircularQueue<DataClass>(_testList.Count);

            queue.Enqueue(new DataClass());
            queue.Dequeue();

            queue.Enqueue(_testList);

            Assert.AreEqual(_testList.Count, queue.Length);
            
            foreach(var item in _testList)
            {
                var result = queue.Dequeue();
                Assert.AreSame(item, result);
            }

            queue.Enqueue(new DataClass());
            queue.Dequeue();

            queue.Enqueue(_testList);
            DataClass[] copyList = new DataClass[_testList.Count];
            queue.CopyTo(copyList, 0);

            for(int i=0; i<_testList.Count; ++i)
            {
                Assert.AreSame(_testList[i], copyList[i]);
            }
        }

        [Test]
        public void OverflowTest()
        {
            int allocCount = _testList.Count / 2;
            var queue = new ConcurrentCircularQueue<DataClass>(allocCount);

            for(int i=0 ; i<_testList.Count; ++i )
            {
                bool result = queue.Enqueue(_testList[i]);
                if (i < allocCount)
                    Assert.AreEqual(true, result);
                else Assert.AreEqual(false, result);
            }

            Assert.AreEqual(allocCount, queue.Length);

            List<DataClass> _dequeueList = new List<DataClass>(_testList.Count);
            for (int i=0; i< allocCount; ++i)
                _dequeueList.Add(queue.Dequeue());

            Assert.AreEqual(0, queue.Length);

            CheckList(_dequeueList, _dequeueList.Count);
        }
    }
}