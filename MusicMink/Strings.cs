using MusicMinkAppLayer.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace MusicMink
{
    public static class Strings
    {
        private static ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse();

        public static string GetResource(string key)
        {
            string s = resourceLoader.GetString(key);

            DebugHelper.Assert(new CallerInfo(), !string.IsNullOrEmpty(s), "Resource Key {0} not found", key);

            return s;
        }

        public static string HandlePlural(int count, string plural, string singular)
        {
            if (count == 1) return count + " " + singular;
            else return count + " " + plural;
        }

        public static string HandlePlural(uint count, string plural, string singular)
        {
            if (count == 1) return count + " " + singular;
            else return count + " " + plural;
        }

        public static string GetFiletype(string fileName)
        {
            string[] fileTypePartsA = fileName.Split('?');

            string[] fileTypeParts = fileTypePartsA[0].Split('.');

            return "." + fileTypeParts.Last().ToLower();
        }

        public static string FormatTimeSpanLong(int p)
        {
            int hours = p / 60;

            string parts = string.Empty;

            if (hours > 0)
            {
                parts = HandlePlural(hours, GetResource("FormatHoursPlural"), GetResource("FormatHoursSingular")) + " ";
            }

            int minutes = p - hours * 60;

            parts = parts + HandlePlural(minutes, GetResource("FormatMinutesPlural"), GetResource("FormatMinutesSingular"));

            return parts;
        }
    }
}
