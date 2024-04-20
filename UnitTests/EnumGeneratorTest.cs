using AAA.BBB;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel.DataAnnotations;

namespace UnitTests;

[TestClass]
public class EnumGeneratorTest
{
    [TestMethod]
    public void TestEnumDefined()
    {
        var a = Includes.PropertyIDsExpanded.ToDisplayFast();
        var b = Test.Includes.F.ToDisplayFast();
        var c = AAA.Includes.Standard.ToDisplayFast();
        var d = AAA.Test.Includes.F.ToDisplayFast();
    }

}
