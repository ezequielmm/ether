using System.Collections.Generic;

public static class LinqExtension
{
    public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> sequence, int size)
    {
        List<T> partition = new List<T>(size);
        foreach (var item in sequence)
        {
            partition.Add(item);
            if (partition.Count == size)
            {
                yield return partition;
                partition = new List<T>(size);
            }
        }
        if (partition.Count > 0)
            yield return partition;
    }
}
