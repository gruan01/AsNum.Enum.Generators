using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAA.BBB;

////[EnumGenerator]
//public enum Class1
//{
//    [Display(Name = "AAA")]
//    A,

//    [Display(Name = "BBB")]
//    B,

//    /// <summary>
//    /// 
//    /// </summary>
//    [Display(Name = "C")]
//    C
//}

public class Test
{
    public enum TestEnum
    {
        [Display(Name = "aaaa")]
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
    [Display(Name = "standard")]
    Standard = 1 << 0,

    /// <summary>
    /// Include the metadata, coordinates and full hierarchy of the region.
    /// </summary>
    [Display(Name = "details")]
    Details = 1 << 1,

    /// <summary>
    /// Include the list of property IDs within the bounding polygon of the region.
    /// </summary>
    [Display(Name = "property_ids")]
    PropertyIDs = 1 << 2,

    /// <summary>
    /// Include the list of property IDs within the bounding polygon of the region and property IDs from the surrounding area if minimal properties are within the region.
    /// </summary>
    [Display(Name = "property_ids_expanded")]
    PropertyIDsExpanded = 1 << 3,
}
