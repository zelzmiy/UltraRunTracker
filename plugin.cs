using System;
using System.Collections.Generic;
using System.IO;
using UMM;
using UnityEngine.SceneManagement;
using HarmonyLib;
using Unity;

namespace UltraRunTracker
{
    [UKPlugin("UltraRunTracker", "1.0.0", "a plugin to track runs in ultrakill\nwarning:having the .csv file open while playing\nwill cause  the mod to malfunction.", true, true)]
    public class CyberGrind : UKMod
    {
        MainGame mg = new MainGame();
        readonly string dirMG = Directory.GetCurrentDirectory() + @"\BepInEx\UMM Mods\UltraRunTracker\runStats.csv";
        readonly string dir = Directory.GetCurrentDirectory() + @"\BepInEx\UMM Mods\UltraRunTracker\cyberStats.csv";
        private List<string> stats = new List<string>();
        bool loggedStats = false;
        public void Awake()
        {
            Harmony harmony = new Harmony("UltraRunTracker");
            harmony.PatchAll();
            if (!File.Exists(dir))
            {
                File.AppendAllText(dirMG, "FinalRank,TimeRank,KillRank,StyleRank,Level,Challange,TookDamage,Kills,Seconds,Stylepoints,Restarts,Difficulty,Cheats,MajorAssists\n");
            }
            if (!File.Exists(dir))
            {
                File.AppendAllText(dir, "Wave,Kills,Seconds,Style,Difficulty,Cheats,MajorAssists\n");
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
                AddStats();
                for (int i = 0; i < stats.Count; i++)
                {
                    File.AppendAllText(dir, stats[i]);
                    if (i != stats.Count - 1) { File.AppendAllText(dir, ", "); }
                    else { File.AppendAllText(dir, "\n"); }
                }
            }          
        }
    }
}
