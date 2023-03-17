using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class NftManagerTests : MonoBehaviour
{
    private NftManager nftManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        nftManager = NftManager.Instance;
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        nftManager.DestroyInstance();
        yield return null;
    }

    [Test]
    public void BadContractGetContractNftsNotNull()
    {
        List<Nft> nftList = nftManager.GetContractNfts(NftContract.None);
        Assert.NotNull(nftList);
    }
}
