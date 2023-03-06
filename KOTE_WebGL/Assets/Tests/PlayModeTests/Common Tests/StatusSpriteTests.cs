using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.VFX;

public class StatusSpriteTests : MonoBehaviour
{
    NamedSpriteList statusIconList;

    [SetUp]
    public void Setup()
    {
        statusIconList =
            AssetDatabase.LoadAssetAtPath<NamedSpriteList>("Assets/Prefabs/Common/SpriteLists/StatusIcons.asset");
    }

    [Test]
    public void AllStatusIconsAreMapped()
    {
        bool allValuesPass = true;
        foreach (STATUS status in Enum.GetValues(typeof(STATUS)))
        {
            string generatedString = status.ToString();
            NamedSpriteList.NamedSprite relatedValue = statusIconList.SpriteList.Find(x => x.name == generatedString);
            if (relatedValue == null)
            {
                allValuesPass = false;
                Debug.LogAssertion($"{generatedString} was not found in StausIcons.asset");
                break;
            }
        }
        Assert.IsTrue(allValuesPass);
    }

    [Test]
    public void AllStatusIconsHaveImages()
    {
        bool allValuesPass = true;
        foreach (NamedSpriteList.NamedSprite namedSprite in statusIconList.SpriteList)
        {
            if (namedSprite.image == null)
            {
                allValuesPass = false;
                Debug.LogAssertion($"{namedSprite.name} in StausIcons.asset did not have an Image.");
                break;
            }
        }
        Assert.IsTrue(allValuesPass);
    }
}
