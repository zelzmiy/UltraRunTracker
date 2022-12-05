using GameConsole;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using Unity;

namespace UltraRunTracker
{
    [HarmonyPatch(typeof(FinalPit), "OnTriggerEnter")]
    public class MainGame
    {
        readonly string dir = Directory.GetCurrentDirectory() + @"\BepInEx\UMM Mods\UltraRunTracker\runStats.csv";
        private List<string> stats = new List<string>();
        static string Runs;
        public string LvlNumToString(int levelnum)
        {  // yandaredev tier code but it looks better on the spreadsheet
            if (levelnum <= 5) { return ("0_" + levelnum); }
            else if (levelnum <= 9) { return ("1_" + (levelnum - 5)); }
            else if (levelnum <= 13) { return ("2_" + (levelnum - 9)); }
            else if (levelnum <= 15) { return ("3_") + (levelnum - 13); }
            else if (levelnum <= 19) { return ("4_" + (levelnum - 15)); }
            else if (levelnum <= 23) { return ("5_" + (levelnum - 23)); }
            else if (levelnum <= 25) { return ("6_") + (levelnum - 25); }
            else if (levelnum == 666) { return ("P_1"); }
            else { return ("oops. something went wrong. the mod is probably not updated to support this level!"); }
        }
        public string NoFormat(string str) {
            return str switch
            {
                "<color=#FFFFFF>P</color>" => ("P"),
                "<color=#FFFFFF>-</color>" => ("-"), // thanks visual studio refactoring, i didn't know what a switch expression was before now,
                "<color=#FF0000>S</color>" => ("S"), // now, i still don't know what a switch expression is, but it looks nicer and it works better.
                "<color=#FF6A00>A</color>" => ("A"), 
                "<color=#FFD800>B</color>" => ("B"),
                "<color=#4CFF00>C</color>" => ("C"),
                "<color=#0094FF>D</color>" => ("D"),
                _ => ("shit something went wrong"),
            };
        }
        public void resetBool() { hasrun = false; }
        private void AddStats()
        {
            
           stats.Add(NoFormat(StatsManager.Instance.fr.totalRank.text));
            stats.Add(NoFormat(StatsManager.Instance.fr.timeRank.text));
            stats.Add(NoFormat(StatsManager.Instance.fr.killsRank.text));
            stats.Add(NoFormat(StatsManager.Instance.fr.styleRank.text)); 

            stats.Add(LvlNumToString(StatsManager.Instance.levelNumber));

            stats.Add(ChallengeManager.Instance.challengeDone.ToString());
            stats.Add(StatsManager.Instance.tookDamage.ToString());
            stats.Add(StatsManager.Instance.kills.ToString());
            stats.Add(StatsManager.Instance.seconds.ToString());
            stats.Add(StatsManager.Instance.stylePoints.ToString());
            stats.Add(StatsManager.Instance.restarts.ToString());

            stats.Add(PrefsManager.Instance.GetInt("difficulty").ToString());
            stats.Add(CheatsController.Instance.cheatsEnabled.ToString());
            stats.Add(AssistController.Instance.majorEnabled.ToString());
        }
        public void WriteToFile() {
            for (int i = 0; i < stats.Count; i++)
            {
                File.AppendAllText(dir, stats[i]);
                if (i != stats.Count - 1) { File.AppendAllText(dir, ","); }
                else { File.AppendAllText(dir, "\n"); }
            }
        }
        static bool hasrun = false;
        
        static void Postfix()
        {           
            MainGame game = new MainGame();
            if (!hasrun) { Runs = File.ReadAllText(game.dir); hasrun = true; }
            game.AddStats();                    
            File.WriteAllText(game.dir, Runs);
            game.WriteToFile();
        }
    }
}
