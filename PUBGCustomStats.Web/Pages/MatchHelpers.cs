using System;

namespace PUBGCustomStats.Web.Pages
{
    public static class MatchHelpers
    {
        public static string GetCarePackageDescription(string? itemId)
        {
            switch (itemId)
            {
                case "Carepackage_SmallPackage_NoParachute_Bluechip_C":
                    return "Blue Chip";
                case "Carapackage_RedBox_C":
                    return "Red Box";
                case "Carapackage_SmallPackage_C":
                    return "Small";
                case "Carapackage_SmallPackage_NoParachute_C":
                    return "Support Flare";
                case null:
                    return "";
                default:
                    Console.WriteLine("Unknown Care Package: " + itemId);
                    return itemId;
            }
        }
    }
}
