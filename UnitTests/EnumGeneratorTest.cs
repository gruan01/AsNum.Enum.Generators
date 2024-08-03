using AAA;
using AAA.BBB;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Security.AccessControl;
using UnitTests.Properties;

namespace UnitTests;

[TestClass]
public class EnumGeneratorTest
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Resources.ResourceManager> resManagers = new();


    private static string Get(string key, Type resourceType, System.Globalization.CultureInfo culture = null)
    {
        return resManagers.GetOrAdd(key, (k) => new System.Resources.ResourceManager(resourceType)).GetString(key, culture);
    }

    [TestMethod]
    public void TestEnumDefined()
    {
        var enUs = CultureInfo.GetCultureInfo("en-US");
        //CultureInfo.CurrentCulture = enUs;
        var res = Resources.ResourceManager.GetString("aa", enUs);
        var aaaa = FlagsEnum.A.ToDisplayFast();
        var aaaaa = FlagsEnum.A.ToDisplayFast(enUs);

        //var a = Includes.PropertyIDsExpanded.ToDisplayFast();
        //var b = Test.Includes.F.ToDisplayFast();
        //var c = AAA.Includes.Standard.ToDisplayFast();
        //var d = AAA.Test.Includes.F.ToDisplayFast();

        //resManagers.GetOrAdd("", (k)=> new System.Resources.ResourceManager())

        var fs = Enum.GetValues<FlagsEnum>();
        var ns = Enum.GetNames<FlagsEnum>();
        var aaa = Enum.GetValuesAsUnderlyingType<FlagsEnum>();

        var tmps = new List<FlagsEnum>();

        for (var i = 1; i<=fs.Length; i++)
        {
            var t = fs.Combinations(i);

            //var tt = t.Select(s => (FlagsEnum)s.Sum(ss => (int)ss));
            foreach (var tt in t)
            {
                FlagsEnum tmp = 0;
                foreach (var ttt in tt)
                {
                    tmp |=ttt;
                }

                tmps.Add(tmp);
            }
        }

        //var fs2 = tmps.Distinct().Select(s => (FlagsEnum)s);
        tmps = tmps.Distinct().ToList();

        var TTT = Enum.IsDefined<FlagsEnum>(FlagsEnum.C | FlagsEnum.B);

        var ee = FlagsEnum.A | FlagsEnum.B;

        var eeeee = ee.ToStringFast();

        var a = ee.HasFlag(FlagsEnum.A | FlagsEnum.C);
        var b = ee.HasFlag(FlagsEnum.A);
        //var c = ee.HasFlag(FlagsEnum.AB);
        //var d = ee.HasFlag(FlagsEnum.AC);
        //var e = ee.HasFlag(FlagsEnum.BC);
        //var f = ee.HasFlag(FlagsEnum.BC);
        //var g = ee.HasFlag(FlagsEnum.ABC);
    }


}


public static class Extensions
{
    public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int index)
    {
        return index == 0
            ? new[] { Array.Empty<T>() }
            : elements.SelectMany((element, indexer) =>
                elements.Skip(indexer + 1).Combinations(index - 1)
                    .Select(combo => (new[] { element })
                        .Concat(combo)));
    }
}