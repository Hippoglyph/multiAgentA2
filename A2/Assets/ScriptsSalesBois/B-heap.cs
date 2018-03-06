using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bheap {

    public static List<List<T>> Permute<T>(this List<T> v)
    {
        List<List<T>> result = new List<List<T>>();

        Permute(v, v.Count, result);

        return result;
    }

    private static void Permute<T>(List<T> v, int n, ICollection<List<T>> result)
    {
        if (n == 1)
        {
            result.Add(new List<T>(v));
        }
        else
        {
            for (var i = 0; i < n; i++)
            {
                Permute(v, n - 1, result);
                Swap(v, n % 2 == 1 ? 0 : i, n - 1);
            }
        }
    }

    private static void Swap<T>(IList<T> v, int i, int j)
    {
        var t = v[i];
        v[i] = v[j];
        v[j] = t;
    }
}
