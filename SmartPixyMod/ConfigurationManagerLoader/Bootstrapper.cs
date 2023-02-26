using System;
using HarmonyLib;
using UnityEngine;
using SBH.ConfigurationManager;
using MBMScripts;

namespace SBH.MBMBootstrapperPlugin
{
    public class Bootstrapper : MonoBehaviour
    {
        private static GameObject? go;

        public Bootstrapper(IntPtr intPtr) : base(intPtr) { }

        public void Awake()
        {
            // Note: You can't create the trainer in Awake() or OnEnable(). It just won't Intstatiate. However, BepInEx will hook Awake()
            //BepInExLoader.log.LogMessage("Bootstrapper Awake() Fired!");
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.Start))]
        [HarmonyPostfix]
        public static void Start()
        {
            MBMBPlugin.log?.LogMessage("Bootstrapper Update() Fired!");

            if (go == null)
            {
                MBMBPlugin.log?.LogMessage(" ");
                MBMBPlugin.log?.LogMessage("Bootstrapping ...");
                try
                {
                    go = ConfigurationWindowManager.Create("ConfigurationManagerGO");
                    if (go != null) { MBMBPlugin.log?.LogMessage("Trainer Bootstrapped!"); MBMBPlugin.log?.LogMessage(" "); }
                }
                catch (Exception e)
                {
                    MBMBPlugin.log?.LogMessage("ERROR Bootstrapping: " + e.Message);
                    MBMBPlugin.log?.LogMessage(e);
                    MBMBPlugin.log?.LogMessage("...");
                }
            }
        }
    }
}