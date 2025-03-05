using System.Collections.Generic;

namespace InnovateLabs.Utilities
{ 
    public static class ListToText
    {
        public static string Convert(List<string> names)
        {
            string text = "";

            foreach (var t in names)
            {
                text += t + ", ";
            }

            return text;
        }
    }
}