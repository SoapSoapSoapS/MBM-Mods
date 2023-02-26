using HarmonyLib;
using MBMScripts;
using UnityEngine;
using System;
using ConfigurationManager;

namespace CMLoader
{
    public class Bootstrapper : MonoBehaviour
    {
        private static GameObject plugin = null;

        internal static GameObject Create(string name)
        {
            var obj = new GameObject(name);
            DontDestroyOnLoad(obj);
            var component = new Bootstrapper(obj.AddComponent(Il2CppInterop.Runtime.Il2CppType.Of<Bootstrapper>()).Pointer);
            return obj;
        }

        public Bootstrapper(IntPtr intPtr) : base(intPtr) { }

        public void Awake()
        {
            // Note: You can't create the trainer in Awake() or OnEnable(). It just won't Intstatiate. However, BepInEx will hook Awake()
            //CMLoaderPlugin.log.LogMessage("Bootstrapper Awake() Fired!");
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.Start))]
        [HarmonyPostfix]
        public static void Update()
        {
            //CMLoaderPlugin.log.LogMessage("Bootstrapper Update() Fired!");

            if (plugin == null)
            {
                CMLoaderPlugin.log.LogMessage(" ");
                CMLoaderPlugin.log.LogMessage("Bootstrapping Trainer...");
                try
                {
                    plugin = ConfigurationWindowManager.Create("TrainerComponentGO");
                    if (plugin != null) { CMLoaderPlugin.log.LogMessage("Trainer Bootstrapped!"); CMLoaderPlugin.log.LogMessage(" "); }
                }
                catch (Exception e)
                {
                    CMLoaderPlugin.log.LogMessage("ERROR Bootstrapping Trainer: " + e.Message);
                    CMLoaderPlugin.log.LogMessage(" ");
                }
            }
        }
    }
}
