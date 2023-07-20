// A wrapper to keep the references to the chunk files
using System.Collections;

namespace FruitSort;

class ChunksCollection : IEnumerable<StreamReader>, IDisposable
{
    private readonly List<StreamReader> store = new();

    public void Dispose()
    {
        foreach (var sr in store)
        {
            sr.Dispose();
        }
    }

    public void Add(StreamReader chunk)
    {
        store.Add(chunk);
    }

    public int Count => store.Count;

    public IEnumerator<StreamReader> GetEnumerator()
    {
        return store.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}