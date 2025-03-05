using System;
using System.IO;
using UnityEngine;

namespace InnovateLabs.Utilities
{
    //Extension methods must be defined in a static class
    public static class StringExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static string ToExplorerPath(this string str)
        {
            return str.Replace('/', '\\');
        }
        public static string ToUnityPath(this string str)
        {
            return str.Replace('\\', '/');
        }
    }

    //public static class ApplicationExtensions
    //{
    //    public static string ConvertToRelativePath(this Application app, string absolutePath)
    //    {
    //        var baseURI = new Uri(Application.dataPath);
    //        var asboluteURI = new Uri(absolutePath);
    //        return Uri.UnescapeDataString(baseURI.MakeRelativeUri(asboluteURI).ToString());
    //    }
    //}
}