using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class ParseStringTests : MonoBehaviour
{
    [Test]
    public void DoesParseStringIdentifyValidEmail()
    {
        Assert.False(ParseString.IsEmail("test"));
        Assert.True(ParseString.IsEmail("test@gmail.com"));
    }

    [Test]
    public void DoesParseStringIdentifyValidPassword()
    {
        Assert.False(ParseString.IsPassword("test"));
        Assert.False(ParseString.IsPassword("1264564456"));
        Assert.False(ParseString.IsPassword("fkjsdflkjsfdljkdsflkj"));
        Assert.False(ParseString.IsPassword("***********"));
        Assert.False(ParseString.IsPassword(""));
        Assert.True(ParseString.IsPassword("Testing123!"));
    }
}
