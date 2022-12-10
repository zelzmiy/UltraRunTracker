using BepInEx.Configuration;
using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

namespace UltraRunTracker 
{
    [HarmonyPatch(typeof(FinalPit), "OnTriggerEnter")]
    public class MainGame
    {
        string dir = Directory.GetCurrentDirectory() + @"\BepInEx\UMM Mods\UltraRunTracker\runStats.csv";

        public ConfigFile Config = new ConfigFile(Path.Combine(Paths.ConfigPath, "UltraRunTracker" + ".cfg"), false);

        public ConfigEntry<bool> SeperateLevels;

        private List<string> stats = new List<string>();

        static string Runs; // alternate variable name: MyNose.

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
                _ => (str),
            };
        }

        static bool hasrun = false;

        public void WriteToFile(string path) {
            AddStats();
            try
            {
                if (!hasrun)
                {
                    Runs = File.ReadAllText(path);
                    hasrun = true;
                }
                File.WriteAllText(path, Runs);

                for (int i = 0; i < stats.Count; i++)
                {
                    File.AppendAllText(path, stats[i]);

                    if (i != stats.Count - 1)
                    {
                        File.AppendAllText(path, ",");
                    }
                    else
                    {
                        File.AppendAllText(path, "\n");
                    }
                }
            }
            catch (Exception) {

                HudMessageReceiver.Instance.SendHudMessage("you have this level's <color=#ff0000ff>FILE</color> open, this run will <color=#ff0000ff>not</color> save."); // thanks Pitr
            }
        }

        public void resetBool() 
        { 
            hasrun = false; 
        }

        private void AddStats()
        {          
            stats.Add(NoFormat(StatsManager.Instance.fr.totalRank.text));
            stats.Add(NoFormat(StatsManager.Instance.fr.timeRank.text));
            stats.Add(NoFormat(StatsManager.Instance.fr.killsRank.text));
            stats.Add(NoFormat(StatsManager.Instance.fr.styleRank.text));
            
            stats.Add(SceneManager.GetActiveScene().name);
            if (ChallengeManager.Instance.challengeDone && !ChallengeManager.Instance.challengeFailed)
            {
                stats.Add(true.ToString()); //why true.ToString() instead of just "TRUE"? because for some reason it automatically translates it to diffrent languages when you open it in excel which is cool
            }
            else {  
                stats.Add(false.ToString());
            }              
            stats.Add(StatsManager.Instance.tookDamage.ToString());
            stats.Add(StatsManager.Instance.kills.ToString());
            stats.Add(StatsManager.Instance.seconds.ToString());
            stats.Add(StatsManager.Instance.stylePoints.ToString());
            stats.Add(StatsManager.Instance.restarts.ToString());

            stats.Add(PrefsManager.Instance.GetInt("difficulty").ToString());
            stats.Add(CheatsController.Instance.cheatsEnabled.ToString());
            stats.Add(AssistController.Instance.majorEnabled.ToString());
        }


        static void Postfix()
        {  
            MainGame game = new MainGame();
            
            game.SeperateLevels = game.Config.Bind("General", "SeperateLevels", false, "whether  or not to seperate each level into it's own file. ");

            if (game.SeperateLevels.Value)
            {
                string dir = Directory.GetCurrentDirectory() + @"\BepInEx\UMM Mods\UltraRunTracker\" + SceneManager.GetActiveScene().name + @".csv";

                if (!File.Exists(dir)) // i haven no clue how to use StreamWriter or StreamReader, please bully me relentelsly for it and then teach me to use it if this is a bad way of doing this.
                {
                     File.AppendAllText(dir, "FinalRank,TimeRank,KillRank,StyleRank,Level,Challange,TookDamage,Kills,Seconds,Stylepoints,Restarts,Difficulty,Cheats,MajorAssists\n");
                }

                 game.WriteToFile(dir);

            }

            else
            {               
                game.WriteToFile(game.dir);
            }                     
        }
    } 
}
