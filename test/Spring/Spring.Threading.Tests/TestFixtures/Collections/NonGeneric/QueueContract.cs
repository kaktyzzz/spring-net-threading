﻿using System;
using System.Collections;
using NUnit.CommonFixtures;
using NUnit.CommonFixtures.Collections;
using NUnit.CommonFixtures.Collections.NonGeneric;
using NUnit.Framework;
using Spring.Collections;

namespace Spring.TestFixtures.Collections.NonGeneric
{
#if PHASED
    using IQueue = ICollection;
#endif

    /// <summary>
    /// Basic functionality test cases for implementation of <see cref="IQueue"/>.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public abstract class QueueContract : CollectionContract
    {
        protected object[] _samples;

        protected int _sampleSize = 9;

        protected bool IsFifo
        {
            get { return Options.Has(CollectionContractOptions.Fifo); }
            set { Options = Options.Set(CollectionContractOptions.Fifo, value); }
        }
        protected bool IsUnbounded
        {
            get { return Options.Has(CollectionContractOptions.Unbounded); }
            set { Options = Options.Set(CollectionContractOptions.Unbounded, value); }
        }

        /// <summary>
        /// Only evaluates option <see cref="CollectionContractOptions.Unique"/>,
        /// <see cref="CollectionContractOptions.Fifo"/>.
        /// </summary>
        /// <param name="options"></param>
        protected QueueContract(CollectionContractOptions options) : base(options)
        {
        }

        protected override sealed ICollection NewCollection()
        {
            return NewQueueFilledWithSample();
        }

        protected virtual IQueue NewQueueFilledWithSample()
        {
            IQueue queue = NewQueue();
#if !PHASED
            foreach (object o in _samples)
            {
                queue.Offer(o);
            }
#endif
            return queue;
        }

        protected virtual object[] NewSamples()
        {
            return TestData<object>.MakeTestArray(_sampleSize);
        }

        /// <summary>
        /// Return a new empty queue.
        /// </summary>
        /// <returns></returns>
        protected abstract IQueue NewQueue();

        [SetUp] public virtual void SetUpSamples()
        {
            _samples = NewSamples();
        }

        protected virtual object MakeData(int i)
        {
            return new object();
        }

#if !PHASED
        [Test] public void AddChokesWhenQueueIsFull()
        {
            Options.SkipWhen(CollectionContractOptions.Unbounded);
            IQueue queue = NewQueueFilledWithSample();
            Assert.Throws<InvalidOperationException>(() => queue.Add(MakeData(0)));
        }

        [Test] public void AddAllSamplesSuccessfully()
        {
            IQueue queue = NewQueue();
            foreach (object o in _samples) queue.Add(o);
            Assert.That(queue.Count, Is.EqualTo(_samples.Length));
            CollectionAssert.AreEquivalent(_samples, queue);
        }

        [Test] public void AddReturnsFalseWhenQueueIsFull()
        {
            Options.SkipWhen(CollectionContractOptions.Unbounded);
            IQueue queue = NewQueueFilledWithSample();
            Assert.IsFalse(queue.Offer(MakeData(0)));
            Assert.That(queue.Count, Is.EqualTo(_sampleSize));
        }

        [Test] public void OfferAllSamplesSuccessfully()
        {
            IQueue queue = NewQueue();
            foreach (object o in _samples)
            {
                Assert.IsTrue(queue.Offer(o));
            }
            Assert.That(queue.Count, Is.EqualTo(_samples.Length));
            CollectionAssert.AreEquivalent(_samples, queue);
        }

        [Test] public void RemoveChokesWhenQueueIsEmpty()
        {
            IQueue queue = NewQueue();
            Assert.Throws<InvalidOperationException>(() => queue.Remove());
        }

        [Test] public void RemoveAllSamplesSucessfully()
        {
            IQueue queue = NewQueueFilledWithSample();

            for (int i = queue.Count - 1; i >= 0; i--)
            {
                object o = queue.Remove();
                Assert.IsNotNull(o);
                CollectionAssert.Contains(_samples, o);
            }
        }

        [Test] public void PollReturnsNullWhenQueueIsEmpty()
        {
            IQueue queue = NewQueue();
            Assert.IsNull(queue.Poll());
        }

        [Test] public void PollAllSamplesSucessfully()
        {
            IQueue queue = NewQueueFilledWithSample();
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                object o = queue.Poll();
                Assert.IsNotNull(o);
                CollectionAssert.Contains(_samples, o);
            }
        }

        [Test] public void ElementChokesWhenQueueIsEmpty()
        {
            IQueue queue = NewQueue();
            Assert.Throws<InvalidOperationException>(() => queue.Element());
        }

        [Test] public void ElementGetsAllSamplesSucessfully()
        {
            IQueue queue = NewQueueFilledWithSample();
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                CollectionAssert.Contains(_samples, queue.Element());
                queue.Remove();
            }
        }

        [Test] public void PeekReturnsNullWhenQueueIsEmpty()
        {
            IQueue queue = NewQueue();
            Assert.IsNull(queue.Peek());
        }

        [Test] public void PeekAllSamplesSucessfully()
        {
            IQueue queue = NewQueueFilledWithSample();
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                CollectionAssert.Contains(_samples, queue.Peek());
                queue.Remove();
            }
        }

        [Test] public void IsEmptyReturnsTrueWhenQueueIsEmptyElseFalse()
        {
            IQueue queue = NewQueue();
            Assert.IsTrue(queue.IsEmpty);
            if (_sampleSize ==0) Assert.Pass();
            queue.Add(_samples[0]);
            Assert.IsFalse(queue.IsEmpty);
        }

        [Test] public void AddRemoveInMultipeLoops()
        {
            IQueue queue = NewQueue();
            AddRemoveOneLoop(queue, _sampleSize / 2);
            AddRemoveOneLoop(queue, _sampleSize );
            AddRemoveOneLoop(queue, _sampleSize *2 / 3);
            AddRemoveOneLoop(queue, _sampleSize);
        }

        private void AddRemoveOneLoop(IQueue queue, int size)
        {
            for (int i = 0; i < size; i++)
            {
                queue.Add(_samples[i]);
            }
            for (int i = 0; i < size; i++)
            {
                object o = queue.Remove();
                if(IsFifo)
                {
                    Assert.That(o, Is.EqualTo(_samples[i]));
                }
                else
                {
                    CollectionAssert.Contains(_samples, o);
                }
            }
        }
#endif
    }
}