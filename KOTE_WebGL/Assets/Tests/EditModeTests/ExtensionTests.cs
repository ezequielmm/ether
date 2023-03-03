using KOTE.UI.Armory;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExtensionTests : MonoBehaviour
{
    [Test]
    public void LinqPartitionCorrectCount()
    {
        List<int> list = new List<int>() { 0,1,2,3,4,5,6,7,8,9 };
        var partition = list.Partition(5);

        Assert.AreEqual(2, partition.Count());
    }
    [Test]
    public void LinqPartitionCorrectCountAdnormalSize()
    {
        List<int> list = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var partition = list.Partition(3);

        Assert.AreEqual(4, partition.Count());
    }
}
