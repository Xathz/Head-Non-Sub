using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeadNonSub {

    public static class People {

        /// <summary>
        /// Wubby's Fun House Discord admins.
        /// </summary>
        public static Dictionary<ulong, string> Administrators => new Dictionary<ulong, string>()
            { { 177657233025400832, "PaymoneyWubby" }, { 113450906359480320, "tt2468" }, { 339786294895050753, "Amanda" }, { 222426598165446656, "ConcealedUnicorn" },
            { 195627168699645953, "xoTigbitties" } };

        /// <summary>
        /// Wubby's Fun House Discord mods.
        /// </summary>
        public static Dictionary<ulong, string> Moderators => new Dictionary<ulong, string>()
            { { 232895628789678080, "Beefwizardus" }, { 146738386583945216, "Tex" }, { 156243299315744768, "HamNCheddar" }, { 335461184285310980, "MrReiter" },
            { 365156279099850755, "satazero" }, { 195362299354021889, "1024x768" }, { 271351283015876618, "sureokay" } };

        /// <summary>
        /// Wubby's Fun House Discord mod-lites.
        /// </summary>
        public static Dictionary<ulong, string> ModeratorLites => new Dictionary<ulong, string>()
            { { 227088829079617536, "Xathz" } };

        /// <summary>
        /// Wubby's Fun House Twitch mods.
        /// </summary>
        public static Dictionary<ulong, string> TwitchModerators => new Dictionary<ulong, string>()
            { { 258818570148904970, "BentheHuman" }, { 197887047682228224, "AverageKangaroo" }, { 201939572311982080, "Impara" }, { 185830396041101321, "jiberjiber" },
            { 141378442271653889, "LinuxBro" }, { 275130855742111745, "Orpis" }, { 245086397239787520, "VilifiedPeanut" }, { 468213004123766797, "SenorStinkfist" },
            { 224056238223130624, "Zadyk Tron" } };

        /// <summary>
        /// Wubby's Fun House Reddit mods.
        /// </summary>
        public static Dictionary<ulong, string> RedditModerators => new Dictionary<ulong, string>()
            { { 202253965188923392, "Foreverdead" }, { 101422646800969728, "Zook" } };

        /// <summary>
        /// Wubby's Fun House bots.
        /// </summary>
        public static Dictionary<ulong, string> Bots => new Dictionary<ulong, string>()
            { { 545778771517636638, "Head Non-Sub" }, { 555800636332179480, "LogBot" }, { 511449458819596289, "MediaShare" }, { 159985870458322944, "MEE6" },
            { 439205512425504771, "NotSoBot" }, { 491769129318088714, "Statbot" }, { 489538588120449039, "WubbyBot" }, { 348350443031625738, "Rawgoat" },
            { 529090224324608000, "WubbyIRLBot" }, { 523299238369820692, "WubbyMail" } };

        /// <summary>
        /// Wubby's Fun House Discord, Twitch, Reddit, and bots.
        /// </summary>
        public static Dictionary<ulong, string> Staff => Administrators.Union(Moderators)
            .Union(ModeratorLites)
            .Union(TwitchModerators)
            .Union(RedditModerators)
            .Union(Bots)
            .ToDictionary(x => x.Key, x => x.Value);

    }

}
