namespace Stl.Tests.Collections;

public class RadixHeapSetTest(ITestOutputHelper @out) : TestBase(@out)
{
    [Fact]
    public void EmptyHeapTest()
    {
        var heap = new RadixHeapSet<int>();
        heap.Count.Should().Be(0);
        heap.MinPriority.Should().Be(0);
        heap.ExtractMin().IsNone().Should().BeTrue();
        Assert.Empty(heap);
    }

    [Fact]
    public void RandomHeapTest()
    {
        var rnd = new Random(10);
        for (var count = 1; count < 150; count++) {
            for (var iteration = 0; iteration < 500; iteration++) {
                var items = Enumerable
                    .Range(0, count)
                    .Select(_ => rnd.Next(1000))
                    .Distinct()
                    .ToList();
                var heap = new RadixHeapSet<int>();
                foreach (var item in items)
                    heap.Add(item, item);

                var itemsToRemove = new HashSet<int>(
                    items.Where(_ => rnd.NextDouble() < 0.33));
                foreach (var item in itemsToRemove)
                    heap.Remove(item, out _);
                items = items
                    .Where(i => !itemsToRemove.Contains(i))
                    .ToList();

                var sortedItems = items.OrderBy(i => i).ToList();
                var heapPriorities = heap.Select(i => i.Priority).ToList();
                var heapValues = heap.Select(i => i.Value).ToList();

                heap.Count.Should().Be(sortedItems.Count);
                heapPriorities.Count.Should().Be(sortedItems.Count);
                heapValues.Count.Should().Be(sortedItems.Count);
                for (var i = 0; i < sortedItems.Count; i++) {
                    heapPriorities[i].Should().Be(sortedItems[i]);
                    heapValues[i].Should().Be(sortedItems[i]);
                }
                var min = heap.PeekMin();
                var minSet = heap.PeekMinSet();
                if (sortedItems.Count == 0) {
                    min.IsNone().Should().BeTrue();
                    minSet.Count.Should().Be(0);
                } else {
                    min.Value.Priority.Should().Be(sortedItems[0]);
                    min.Value.Value.Should().Be(sortedItems[0]);
                    minSet.Should().NotBeNullOrEmpty();
                    foreach (var (value, priority) in minSet) {
                        priority.Should().Be(sortedItems[0]);
                        value.Should().Be(sortedItems[0]);
                    }
                }
            }
        }
    }

    [Fact]
    public void AddOrUpdateTest()
    {
        var heap = new RadixHeapSet<string>();
        heap.AddOrUpdate(5, "a");
        heap.AddOrUpdate(1, "b");
        heap.Count.Should().Be(2);
        heap.PeekMin().Value.Value.Should().Be("b");
        heap.ExtractMin().Value.Value.Should().Be("b");
        heap.Count.Should().Be(1);

        heap.AddOrUpdate(3, "a");
        heap.Count.Should().Be(1);
        heap.PeekMin().Value.Value.Should().Be("a");
        heap.ExtractMin().Value.Value.Should().Be("a");
    }

    [Fact]
    public void UpdateMinPriorityTest()
    {
        var heap = new RadixHeapSet<int>();
        for (var i = 1; i < 100; i += 5) {
            heap.AddOrUpdate(i, i);
            heap.Count.Should().Be((i + 4) / 5);
            heap.PeekMin().Value.Value.Should().Be(1);
        }
        for (var i = 0; i < 95; i++) {
            var minSet = heap.ExtractMinSet(i);
            var isFit = (i - 1) % 5 == 0;
            minSet.Count.Should().Be(isFit ? 1 : 0);
            if (isFit)
                minSet.Single().Value.Should().Be(i);
        }
    }
}
