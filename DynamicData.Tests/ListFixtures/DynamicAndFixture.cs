using System;
using System.Linq;
using NUnit.Framework;

namespace DynamicData.Tests.ListFixtures
{
    [TestFixture]
    public class DynamicAndFixture
    {
        private ISourceList<int> _source1;
        private ISourceList<int> _source2;
        private ISourceList<int> _source3;
        private ISourceList<IObservable<IChangeSet<int>>> _source;

        private ChangeSetAggregator<int> _results;

        [SetUp]
        public void Initialise()
        {
            _source1 = new SourceList<int>();
            _source2 = new SourceList<int>();
            _source3 = new SourceList<int>();
            _source = new SourceList<IObservable<IChangeSet<int>>>();
            _results = _source.And().AsAggregator();
        }

        [TearDown]
        public void Cleanup()
        {
            _source1.Dispose();
            _source2.Dispose();
            _source3.Dispose();
            _source.Dispose();
            _results.Dispose();
        }

        [Test]
        public void ExcludedWhenItemIsInOneSource()
        {
            _source.Add(_source1.Connect());
            _source.Add(_source2.Connect());
            _source1.Add(1);
            Assert.AreEqual(0, _results.Data.Count);
        }

        [Test]
        public void IncludedWhenItemIsInTwoSources()
        {
            _source.Add(_source1.Connect());
            _source.Add(_source2.Connect());
            _source1.Add(1);
            _source2.Add(1);
            Assert.AreEqual(1, _results.Data.Count);
        }

        [Test]
        public void RemovedWhenNoLongerInBoth()
        {
            _source.Add(_source1.Connect());
            _source.Add(_source2.Connect());
            _source1.Add(1);
            _source2.Add(1);
            _source1.Remove(1);
            Assert.AreEqual(0, _results.Data.Count);
        }

        [Test]
        public void CombineRange()
        {
            _source.Add(_source1.Connect());
            _source.Add(_source2.Connect());
            _source1.AddRange(Enumerable.Range(1, 10));
            _source2.AddRange(Enumerable.Range(6, 10));
            Assert.AreEqual(5, _results.Data.Count);
            CollectionAssert.AreEquivalent(Enumerable.Range(6, 5), _results.Data.Items);
        }

        [Test]
        public void ClearOneClearsResult()
        {
            _source.Add(_source1.Connect());
            _source.Add(_source2.Connect());
            _source1.AddRange(Enumerable.Range(1, 5));
            _source2.AddRange(Enumerable.Range(1, 5));
            _source1.Clear();
            Assert.AreEqual(0, _results.Data.Count);
        }

        [Test]
        public void AddAndRemoveLists()
        {
            _source1.AddRange(Enumerable.Range(1, 5));
            _source3.AddRange(Enumerable.Range(1, 5));

            _source.Add(_source1.Connect());
            _source.Add(_source3.Connect());

            var result = Enumerable.Range(1, 5).ToArray();

            Assert.AreEqual(5, _results.Data.Count);
            CollectionAssert.AreEquivalent(result, _results.Data.Items);

            _source2.AddRange(Enumerable.Range(6, 5));
            Assert.AreEqual(5, _results.Data.Count);

            _source.Add(_source2.Connect());
            Assert.AreEqual(0, _results.Data.Count);

            _source.RemoveAt(2);
            Assert.AreEqual(5, _results.Data.Count);
            CollectionAssert.AreEquivalent(result, _results.Data.Items);
        }
    }
}
