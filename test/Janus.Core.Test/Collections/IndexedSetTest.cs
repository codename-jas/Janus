using System;
using System.Collections.Generic;
using Janus.Core.Collections;
using Xunit;
using System.Linq;
namespace Janus.Core.Test
{
    internal class FinalizeDisposableIndexedSet<T> : IndexedSet<T>
    {
        private readonly Action _onExplicitDispose;
        private readonly Action _onImplicitDispose;

        public FinalizeDisposableIndexedSet(Action onExplicitDispose, Action onImplicitDispose)
        {
            this._onExplicitDispose = onExplicitDispose;
            this._onImplicitDispose = onImplicitDispose;
        }

        protected override void DisposeExplicit()
        {
            this._onExplicitDispose?.DynamicInvoke();
            base.DisposeExplicit();
        }

        protected override void DisposeImplicit()
        {
            this._onImplicitDispose?.DynamicInvoke();
            base.DisposeImplicit();
        }
    }

    public class IndexedSetTest
    {

        #region [ Positive ]

        #region [ Construction/Disposing ]

        [Fact]
        public void Default_CtorTest()
        {
            var sut = new IndexedSet<object>();
            Assert.NotNull(sut);
            Assert.Empty(sut);
            Assert.False(sut.IsReadOnly);
            Assert.Null(sut[0]);
        }

        [Fact]
        public void WithInitialValues_CtorTest()
        {
            var sut = new IndexedSet<string>(new string[] { "test 1", "test 2", "test 3" });
            Assert.NotNull(sut);
            Assert.NotEmpty(sut);
            Assert.Contains("test 1", sut);
            Assert.Contains("test 2", sut);
            Assert.Contains("test 3", sut);
            Assert.Equal(3, sut.Count);
            for (var i = 0; i < 3; i++)
            {
                Assert.Equal($"test {i + 1}", sut[i]);
            }
        }

        [Fact]
        public void DisposableTest()
        {
            IndexedSet<string> sut;
            using (sut = new IndexedSet<string>())
            {
                Assert.NotNull(sut);
            }
            Assert.True(sut.Disposed);
        }

        [Fact]
        public void Dispose_CallsExplicitAndImplicitDisposal()
        {
            // Arrange
            var @explicit = false;
            var @implicit = false;
            var disposable = new FinalizeDisposableIndexedSet<string>(
                onExplicitDispose: () => @explicit = true,
                onImplicitDispose: () => @implicit = true);

            // Act
            disposable.Dispose();

            // Assert
            Assert.True(@explicit);
            Assert.True(@implicit);
        }

        [Fact]
        public void FinalizerTest()
        {
            // Arrange
            var @explicit = false;
            var @implicit = false;
            WeakReference<FinalizeDisposableIndexedSet<string>> weak = null;
            Action dispose = () =>
            {
                // This will go out of scope after dispose() is executed
                var disposable = new FinalizeDisposableIndexedSet<string>(
                    onExplicitDispose: () => @explicit = true,
                    onImplicitDispose: () => @implicit = true);
                weak = new WeakReference<FinalizeDisposableIndexedSet<string>>(disposable, true);
            };

            // Act
            dispose();
            GC.Collect(0, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();

            // Assert
            Assert.False(@explicit); // Not called through finalizer
            Assert.True(@implicit);
        }

        #endregion

        #endregion

        #region [ Negative ]

        #region [ Construction/Disposing ]

        [Fact]
        public void NullInitialValues_CtorTest()
        {
            IndexedSet<string> sut = null;
            Action ctor = () => { sut = new IndexedSet<string>(null); };
            Assert.Throws<ArgumentNullException>(ctor);
            Assert.Null(sut);
        }

        [Fact]
        public void EmptyInitialValues_CtorTest()
        {
            IndexedSet<string> sut = null;
            Action ctor = () => { sut = new IndexedSet<string>(new string[] { }); };
            ctor();
            Assert.NotNull(sut);
            Assert.Empty(sut);
        }

        public static IEnumerable<object[]> DuplicateInitialValues =>
        new List<object[]>
        {
            new object[] {new string[] {"Test 1", "Test 2", "Test 3", "Test 1"}},
            new object[] {new string[] {"Test 1", "Test 1", "Test 3", "Test 4"}},
            new object[] {new string[] {"Test 1", "Test 2", "Test 2", "Test 1"}}
        };

        [Theory]
        [MemberData(nameof(DuplicateInitialValues))]
        public void DuplicateInitialValues_CtorTest(string[] values)
        {
            var sut = new IndexedSet<string>(values);
            var uniqueValues = values.Distinct().ToList();
            Assert.NotNull(sut);
            Assert.Equal(uniqueValues.Count, sut.Count);
            uniqueValues.ForEach(x => Assert.Contains(x, sut));
            //asert ordering
            for (var i = 0; i < uniqueValues.Count; i++)
            {
                Assert.Equal(uniqueValues[i], sut[i]);
            }
        }

        #endregion

        #endregion

    }
}