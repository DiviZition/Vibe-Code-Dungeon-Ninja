using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestingScript
{
    [Test]
    public void TestingScriptSimplePasses()
    {
        //1st level is/has/does/contains
        //2nd level all/not/some/exactly
        //or/and/not/
        //Is.Unique / Is.Ordered
        //Assert.IsTrue

        string username = "User123";
        Assert.That(username, Does.StartWith("U"));
        Assert.That(username, Does.EndWith("3"));

        var testList = new List<int>{1, 2, 3, 4, 5};
        Assert.That(testList, Contains.Item(3));
        Assert.That(testList, Is.All.Positive);
        Assert.That(testList, Has.Exactly(2).LessThan(3));
        Assert.That(testList, Is.Ordered);
        Assert.That(testList, Is.Unique);
        Assert.That(testList, Has.Exactly(3).Matches<int>(NumberPredicates.IsOdd));
    }
}

public static class NumberPredicates
{
    public static bool IsEven(int number) => number % 2 == 0;
    public static bool IsOdd(int number) => number % 2 != 0;
}
