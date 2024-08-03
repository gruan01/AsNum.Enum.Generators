using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.Properties;

namespace AAA;

public class Test
{
    public enum TestEnum
    {
        A,
        B,
        C
    }

    public enum Includes
    {
        E,
        F,
        G,
        H
    }
}



/// <summary>
/// 
/// </summary>
[Flags]
public enum Includes
{
    /// <summary>
    /// 
    /// </summary>
    [Description("standard")]
    Standard = 1 << 0,

    /// <summary>
    /// Include the metadata, coordinates and full hierarchy of the region.
    /// </summary>
    [Description("details")]
    Details = 1 << 1,

    /// <summary>
    /// Include the list of property IDs within the bounding polygon of the region.
    /// </summary>
    [Description("property_ids")]
    PropertyIDs = 1 << 2,

    /// <summary>
    /// Include the list of property IDs within the bounding polygon of the region and property IDs from the surrounding area if minimal properties are within the region.
    /// </summary>
    [Description("property_ids_expanded")]
    PropertyIDsExpanded = 1 << 3,
}


[Flags]
public enum FlagsEnum
{
    [Display(Name = "aa", ResourceType = typeof(Resources))]
    A = 1 << 0,
    [Display(Name = "bb")]
    B = 1 << 1,

    [Display(Name = "cc")]
    C = 1 << 2,


    [Display(Name = "AB")]
    AB = A | B,

    [Display(Name = "AC")]
    AC = A | C,
    //[Display(Name = "BC")]
    //BC = B | C,
    //[Display(Name = "ABC")]
    //ABC = A | B |C
}