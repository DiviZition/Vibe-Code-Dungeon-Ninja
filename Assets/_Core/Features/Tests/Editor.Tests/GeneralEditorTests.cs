using Dungeon;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

public class GeneralEditorTests
{
}

public static class NumberPredicates
{
    public static bool IsEven(int number) => number % 2 == 0;
    public static bool IsOdd(int number) => number % 2 != 0;
}
