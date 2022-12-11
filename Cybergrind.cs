using System.Collections.Generic;
using System.IO;
using UMM;
using UnityEngine.SceneManagement;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using GameConsole;
using UnityEngine;
using System;
using JetBrains.Annotations;

namespace UltraRunTracker
{

    [UKPlugin("UltraRunTracker", "1.1.0", "A run history tracker for Ultrakill", true, true)]
    public class CyberGame : UKMod
    {
        MainGame mg = new MainGame();

        DateTime Time = DateTime.Now;


        public ConfigFile Config = new ConfigFile(Path.Combine(Paths.ConfigPath, "UltraRunTracker" + ".cfg"), false);

        public ConfigEntry<bool> SeperateLevels;

        public ConfigEntry<bool> seperateSessions;

        string dir = Directory.GetCurrentDirectory() + @"\BepInEx\UMM Mods\UltraRunTracker\cyberStats.csv";

        private List<string> stats = new List<string>();

        bool loggedStats = false;
        public static string session = "test"; 

        public void Awake()
        {
            Harmony harmony = new Harmony("UltraRunTracker");
            harmony.PatchAll();

            SeperateLevels = Config.Bind("Toggles", "SeperateLevels", false, "whether  or not to seperate each level into it's own file. ");

            seperateSessions = Config.Bind("Toggles", "SeperateSessions", false, "whether  or not to seperate each Session (every time you launch the game) into it's own file. ");

            if (seperateSessions.Value) 
            {
                session = Time.ToString("MM'.'dd'.'HH'.'mm'.'ss");
                Debug.Log(session);
            }       
            SceneManager.sceneLoaded += OnSceneLoaded;

        }
        private void AddStats() {

            stats.Add(EndlessGrid.Instance.currentWave.ToString());
            stats.Add(StatsManager.Instance.kills.ToString());
            stats.Add(StatsManager.Instance.seconds.ToString());
            stats.Add(StatsManager.Instance.stylePoints.ToString());
            stats.Add(PrefsManager.Instance.GetInt("difficulty").ToString());
            stats.Add(CheatsController.Instance.cheatsEnabled.ToString());
            stats.Add(AssistController.Instance.majorEnabled.ToString());
            loggedStats = true;   
            
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            stats.Clear();
            mg.resetBool();
            loggedStats = false;
        }

        public void Update() {
            if (SceneManager.GetActiveScene().name == "Endless" && NewMovement.Instance.dead && !loggedStats) // finalcyberrank.gameover is private :cryge:
            {
                try
                {
                    if (seperateSessions.Value)
                    {
                        dir = Directory.GetCurrentDirectory() + @"\BepInEx\UMM Mods\UltraRunTracker\cyberStats - " + session + ".csv";
                    }

                    if (!File.Exists(dir))
                    {
                        File.AppendAllText(dir, "Wave,Kills,Seconds,Style,Difficulty,Cheats,MajorAssists\n");
                    }

                    AddStats();
                    for (int i = 0; i < stats.Count; i++)
                    {
                        File.AppendAllText(dir, stats[i]);
                        if (i != stats.Count - 1) { File.AppendAllText(dir, ", "); }
                        else { File.AppendAllText(dir, "\n"); }
                    }
                }
                catch (Exception exception) {
                    System.Console.WriteLine(exception);
                    HudMessageReceiver.Instance.SendHudMessage("something went wrong, mostly likely\nyou have this level's <color=#ff0000ff>FILE</color> open, this run will <color=#ff0000ff>not</color> save."); // thanks Pitr
                }
            }          
        }
    }
}
