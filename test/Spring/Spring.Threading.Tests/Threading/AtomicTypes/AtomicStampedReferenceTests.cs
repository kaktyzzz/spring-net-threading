#region License

/*
 * Copyright 2002-2008 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.CommonFixtures;
using NUnit.Framework;

namespace Spring.Threading.AtomicTypes
{
    [TestFixture(typeof(string))]
    [TestFixture(typeof(object))]
	public class AtomicStampedReferenceTests<T> : ThreadingTestFixture<T>
        where T : class 
	{
	    [Test]
		public void Constructor()
		{
            AtomicStampedReference<T> ai = new AtomicStampedReference<T>(one, 0);
			Assert.AreEqual(one, ai.Value);
			Assert.AreEqual(0, ai.Stamp);
            AtomicStampedReference<T> a2 = new AtomicStampedReference<T>(null, 1);
			Assert.IsNull(a2.Value);
			Assert.AreEqual(1, a2.Stamp);
		}

		[Test]
		public void GetSet()
		{
			int mark;
            AtomicStampedReference<T> ai = new AtomicStampedReference<T>(one, 0);
			Assert.AreEqual(one, ai.Value);
			Assert.AreEqual(0, ai.Stamp);
			Assert.AreEqual(one, ai.GetValue(out mark));
			Assert.AreEqual(0, mark);
			ai.SetNewAtomicValue(two, 0);
			Assert.AreEqual(two, ai.Value);
			Assert.AreEqual(0, ai.Stamp);
            Assert.AreEqual(two, ai.GetValue(out mark));
			Assert.AreEqual(0, mark);
			ai.SetNewAtomicValue(one, 1);
			Assert.AreEqual(one, ai.Value);
			Assert.AreEqual(1, ai.Stamp);
            Assert.AreEqual(one, ai.GetValue(out mark));
			Assert.AreEqual(1, mark);

			ai.SetNewAtomicValue(one, 1);
			Assert.AreEqual(one, ai.Value);
			Assert.AreEqual(1, ai.Stamp);
            Assert.AreEqual(one, ai.GetValue(out mark));
			Assert.AreEqual(1, mark);
		}

		[Test]
		public void AttemptStamp()
		{
			int mark;
            AtomicStampedReference<T> ai = new AtomicStampedReference<T>(one, 0);
			Assert.AreEqual(0, ai.Stamp);
			Assert.IsTrue(ai.AttemptStamp(one, 1));
			Assert.AreEqual(1, ai.Stamp);
			Assert.AreEqual(one, ai.GetValue(out mark));
			Assert.AreEqual(1, mark);
		}

		[Test]
		public void CompareAndSet()
		{
			int mark;
            AtomicStampedReference<T> ai = new AtomicStampedReference<T>(one, 0);
			Assert.AreEqual(one, ai.GetValue(out mark));
			Assert.AreEqual(0, ai.Stamp);
			Assert.AreEqual(0, mark);

			Assert.IsTrue(ai.CompareAndSet(one, two, 0, 0));
			Assert.AreEqual(two, ai.GetValue(out mark));
			Assert.AreEqual(0, mark);

			Assert.IsTrue(ai.CompareAndSet(two, m3, 0, 1));
			Assert.AreEqual(m3, ai.GetValue(out mark));
			Assert.AreEqual(1, mark);

			Assert.IsFalse(ai.CompareAndSet(two, m3, 1, 1));
			Assert.AreEqual(m3, ai.GetValue(out mark));
			Assert.AreEqual(1, mark);
		}

		[Test]
		public void CompareAndSetInMultipleThreads()
		{
            AtomicStampedReference<T> ai = new AtomicStampedReference<T>(one, 0);
            Thread t = new Thread(delegate()
            {
                while (!ai.CompareAndSet(two, three, 0, 0))
                    Thread.Sleep(0);
            });
			t.Start();
			Assert.IsTrue(ai.CompareAndSet(one, two, 0, 0));
			t.Join(Delays.Long);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(ai.Value, three);
			Assert.AreEqual(ai.Stamp, 0);
		}

		[Test]
		public void CompareAndSetInMultipleThreads2()
		{
            AtomicStampedReference<T> ai = new AtomicStampedReference<T>(one, 0);
            Thread t = new Thread(delegate()
            {
                while (!ai.CompareAndSet(one, one, 1, 2))
                    Thread.Sleep(0);
            });
			t.Start();
			Assert.IsTrue(ai.CompareAndSet(one, one, 0, 1));
			t.Join(Delays.Long);
			Assert.IsFalse(t.IsAlive);
			Assert.AreEqual(ai.Value, one);
			Assert.AreEqual(ai.Stamp, 2);
		}

		[Test]
		public void WeakCompareAndSet()
		{
			int mark;
            AtomicStampedReference<T> ai = new AtomicStampedReference<T>(one, 0);
			Assert.AreEqual(one, ai.GetValue(out mark));
			Assert.AreEqual(0, ai.Stamp);
			Assert.AreEqual(0, mark);

			while (!ai.WeakCompareAndSet(one, two, 0, 0)) {}
		    
			Assert.AreEqual(two, ai.GetValue(out mark));
			Assert.AreEqual(0, mark);

			while (!ai.WeakCompareAndSet(two, m3, 0, 1)) {}

		    Assert.AreEqual(m3, ai.GetValue(out mark));
			Assert.AreEqual(1, mark);
		}
		[Test]
		public void SerializeAndDeseralize()
		{
            AtomicStampedReference<T> atomicStampedReference = new AtomicStampedReference<T>(one, 0987654321);	
			MemoryStream bout = new MemoryStream(10000);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(bout, atomicStampedReference);

			MemoryStream bin = new MemoryStream(bout.ToArray());
			BinaryFormatter formatter2 = new BinaryFormatter();
            AtomicStampedReference<T> atomicStampedReferenceReference2 = (AtomicStampedReference<T>)formatter2.Deserialize(bin);

			Assert.AreEqual(atomicStampedReference.Value, atomicStampedReferenceReference2.Value);
			Assert.AreEqual( atomicStampedReference.Stamp, atomicStampedReferenceReference2.Stamp);
		}
	}
}