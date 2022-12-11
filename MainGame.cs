using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltraRunTracker 
{
    [HarmonyPatch(typeof(FinalPit), "OnTriggerEnter")]
    public class MainGame
    {

        string dir = Directory.GetCurrentDirectory() + @"\BepInEx\UMM Mods\UltraRunTracker\";

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
            catch (Exception exception) {
                Console.WriteLine(exception);
                HudMessageReceiver.Instance.SendHudMessage("something went wrong, mostly likely\nyou have this level's <color=#ff0000ff>FILE</color> open, this run will <color=#ff0000ff>not</color> save."); // thanks Pitr
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
                stats.Add(true.ToString());
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

        //this method being static has cause me so many fucking problems
        static void Postfix()
        {  
            
            MainGame game = new MainGame();

            CyberGame cg = new CyberGame();

            string dir = game.dir;
            string session = CyberGame.session; 

            // why the fuck do i have to do this twice??  
            ConfigEntry<bool> SeperateLevels = cg.Config.Bind("Toggles", "SeperateLevels", false, "whether  or not to seperate each level into it's own file. ");

            ConfigEntry<bool>  seperateSessions = cg.Config.Bind("Toggles", "SeperateSessions", false, "whether  or not to seperate each Session (every time you launch the game) into it's own file. ");

            if (!seperateSessions.Value && !SeperateLevels.Value)
            {
                dir += "runstats";
            }

            if (SeperateLevels.Value) {
                dir += SceneManager.GetActiveScene().name; 
            }

            if (seperateSessions.Value
                ) { // trying to get the session to work properly is hell why does it keep bieng nothing 
                dir += " - " + session;
            }   
            
            dir += ".csv";

            if (!File.Exists(dir)) {
                File.AppendAllText(dir, "FinalRank,TimeRank,KillRank,StyleRank,Level,Challange,TookDamage,Kills,Seconds,Stylepoints,Restarts,Difficulty,Cheats,MajorAssists\n");
            } 

            game.WriteToFile(dir);        
        }
    } 
}
    
