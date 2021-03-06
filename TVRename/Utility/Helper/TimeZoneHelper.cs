// 
// Main website for TVRename is http://tvrename.com
// 
// Source code available at https://github.com/TV-Rename/tvrename
// 
// Copyright (c) TV Rename. This code is released under GPLv3 https://github.com/TV-Rename/tvrename/blob/master/LICENSE.md
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using NodaTime;

namespace TVRename
{
    public static class TimeZoneHelper
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        [NotNull]
        //public static IEnumerable<string> ZoneNames() => TimeZoneInfo.GetSystemTimeZones().Select(x=>x.StandardName);
        public static IEnumerable<string> ZoneNames() => DateTimeZoneProviders.Tzdb.Ids;

        [NotNull]
        //public static string DefaultTimeZone() => "Eastern Standard Time";
        public static string DefaultTimeZone() => "America/New_York";

        [NotNull]
        public static string TimeZoneForNetwork([CanBeNull] string network,string defaultTimeZone)
        {
            string[] uktv = { "Sky Atlantic (UK)", "BBC One", "Sky1", "BBC Two", "ITV", "Nick Jr.", "BBC Three", "Channel 4", "CBeebies", "Sky Box Office", "Watch", "ITV2", "National Geographic (UK)", "V", "ITV Encore", "ITV1", "BBC", "E4", "Channel 5 (UK)", "BBC Four", "ITVBe" };
            string[] ausTv = { "ABC4Kids", "Stan", "Showcase (AU)", "PBS Kids Sprout", "SBS (AU)", "Nine Network", "ABC (AU)" };
            string[] usTv =
            {
                "ABC (US)", "Amazon", "AMC", "BBC America", "Bravo", "Cartoon Network", "CBC (CA)", "CBS", "Cinemax",
                "CNN", "Comedy Central", "Discovery Family", "Disney Channel", "Disney Junior", "Disney XD", "Disney+",
                "ESPN", "FOX", "FX", "FXX", "HBO", "History", "Hulu", "IFC", "MTV", "National Geographic", "NBC",
                "Netflix", "NHK", "PBS", "Showtime", "Starz", "Syfy", "TBS", "The CW", "TNT (US)", "Travel Channel",
                "USA Network", "Yahoo! Screen", "YouTube", "YouTube Premium"
            };

            if (string.IsNullOrWhiteSpace(network))
            {
                return defaultTimeZone?? DefaultTimeZone();
            }

            if (uktv.Contains(network))
            {
                //return "GMT Standard Time";
                return "Europe/London";
            }

            if (ausTv.Contains(network))
            {
                //return "AUS Eastern Standard Time";
                return "Australia/Sydney";
            }

            if (usTv.Contains(network))
            {
                //return "Eastern Standard Time";
                return "America/New_York";
            }

            return defaultTimeZone ??DefaultTimeZone();
        }

        public static DateTime AdjustTzTimeToLocalTime(LocalDateTime theirDateTime, [CanBeNull] DateTimeZone theirTimeZone)
        {
            try
            {
                return theirTimeZone is null
                    ? theirDateTime.ToDateTimeUnspecified()
                    : theirDateTime.InZoneLeniently(theirTimeZone).ToDateTimeUtc().ToLocalTime();
            }
            catch (ArgumentException)
            {
                try
                {
                    Debug.Assert(theirTimeZone != null, nameof(theirTimeZone) + " != null");
                    DateTime returnValue = theirDateTime.PlusHours(1).InZoneLeniently(theirTimeZone).ToDateTimeUtc().ToLocalTime();
                    Logger.Warn($"Could not convert {theirDateTime} in {theirTimeZone.Id} into {TimeZoneInfo.Local.StandardName} in TimeZoneHelper.AdjustTzTimeToLocalTime (added one hour and it worked ok to account for daylight savings)");
                    return returnValue;
                }
                catch (ArgumentException ae)
                {
                    Logger.Error($"Could not convert {theirDateTime} in {theirTimeZone?.Id} into {TimeZoneInfo.Local.StandardName} in TimeZoneHelper.AdjustTzTimeToLocalTime (tried adding one hour too so that we account for daylight saving): {ae.Message}");
                    return theirDateTime.ToDateTimeUnspecified();
                }
            }
        }

        public static double Epoch(DateTime dt)
        {
            return dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
        }
    }
}
