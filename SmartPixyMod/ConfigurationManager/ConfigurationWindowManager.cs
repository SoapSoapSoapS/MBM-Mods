// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

using BepInEx.Logging;
using SBH.ConfigurationManager.Drawers;
using SBH.ConfigurationManager.Models;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Input = BepInEx.IL2CPP.UnityEngine.Input;
using KeyCode = BepInEx.IL2CPP.UnityEngine.KeyCode;

namespace SBH.ConfigurationManager
{
    public class ConfigurationWindowManager : MonoBehaviour
    {
        public static GameObject? obj;

        public ConfigurationWindowModel ConfigurationWindow;

        private PropertyInfo? _curLockState;
        private PropertyInfo? _curVisible;
        private int _previousCursorLockState;
        private bool _previousCursorVisible;
        private bool _obsoleteCursor;
        private bool _hotkeyWasDown;

        /// <summary>
        /// Enable to display the main window
        /// </summary>
        public bool DisplayingWindow
        {
            get => _displayingWindow;
            set
            {
                if (_displayingWindow == value) return;
                _displayingWindow = value;

                SettingDrawer.ClearCache();

                if (_displayingWindow)
                {
                    ConfigurationWindow = new ConfigurationWindowModel();

                    ConfigurationWindow.FocusSearchBox = true;

                    // Do through reflection for unity 4 compat
                    if (_curLockState != null)
                    {
                        _previousCursorLockState = _obsoleteCursor ? Convert.ToInt32((bool)_curLockState.GetValue(null, null)) : (int)_curLockState.GetValue(null, null);
                        _previousCursorVisible = (bool)_curVisible.GetValue(null, null);
                    }
                }
                else
                {
                    if (!_previousCursorVisible || _previousCursorLockState != 0) // 0 = CursorLockMode.None
                        SetUnlockCursor(_previousCursorLockState, _previousCursorVisible);
                }
            }
        }
        private bool _displayingWindow;

        public static GameObject Create(string name)
        {
            CMPlugin.log?.LogMessage("Creating");

            obj = new GameObject(name);
            DontDestroyOnLoad(obj);

            obj.AddComponent(UnhollowerRuntimeLib.Il2CppType.Of<ConfigurationWindowManager>());

            return obj;
        }

        public ConfigurationWindowManager(IntPtr ptr) : base(ptr)
        {
            CMPlugin.log?.LogMessage("Constructor");
            ConfigurationWindow = new ConfigurationWindowModel();
        }

        public void Start()
        {
            CMPlugin.log?.LogMessage("Starting");

            ConfigurationDrawer.InitializeTextureConstants();

            // Use reflection to keep compatibility with unity 4.x since it doesn't have Cursor
            var tCursor = typeof(Cursor);
            _curLockState = tCursor.GetProperty("lockState", BindingFlags.Static | BindingFlags.Public);
            _curVisible = tCursor.GetProperty("visible", BindingFlags.Static | BindingFlags.Public);

            if (_curLockState == null && _curVisible == null)
            {
                _obsoleteCursor = true;

                _curLockState = typeof(Screen).GetProperty("lockCursor", BindingFlags.Static | BindingFlags.Public);
                _curVisible = typeof(Screen).GetProperty("showCursor", BindingFlags.Static | BindingFlags.Public);
            }

            // Check if user has permissions to write config files to disk
            try { CMPlugin.config?.Save(); }
            catch (IOException ex) { CMPlugin.log?.Log(LogLevel.Message | LogLevel.Warning, "WARNING: Failed to write to config directory, expect issues!\nError message:" + ex.Message); }
            catch (UnauthorizedAccessException ex) { CMPlugin.log?.Log(LogLevel.Message | LogLevel.Warning, "WARNING: Permission denied to write to config directory, expect issues!\nError message:" + ex.Message); }
        }

        public void Update()
        {
            if (DisplayingWindow) SetUnlockCursor(0, true);

            var hotkeyIsDown = Input.GetKeyInt(KeyCode.F1);
            if (_hotkeyWasDown && !hotkeyIsDown)
                DisplayingWindow = !DisplayingWindow;

            _hotkeyWasDown = hotkeyIsDown;
        }

        public void LateUpdate()
        {
            if (DisplayingWindow) SetUnlockCursor(0, true);
        }

        public void OnGUI()
        {
            if (!DisplayingWindow) return;

            SetUnlockCursor(0, true);

            if (GUI.Button(ConfigurationWindow.ScreenRect, string.Empty, GUI.skin.box) &&
                    !ConfigurationWindow.SettingWindowRect.Contains(UnityEngine.Input.mousePosition))
                DisplayingWindow = false;

            GUI.Box(ConfigurationWindow.SettingWindowRect, GUIContent.none,
                new GUIStyle { normal = new GUIStyleState { background = ConfigurationDrawer.WindowBackground } });

            //GUILayout.Window(ConfigurationDrawer.WindowId, ConfigurationWindow.SettingWindowRect,
            //    (GUI.WindowFunction)SettingsWindow, "Plugin / mod settings");

            GUIUtility.CheckOnGUI();
            GUI.DoWindow(
                ConfigurationDrawer.WindowId,
                ConfigurationWindow.SettingWindowRect,
                (GUI.WindowFunction)SettingsWindow,
                GUIContent.Temp("Plugin / mod settings"),
                GUI.skin.window, GUI.skin, true);
        }

        public void SettingsWindow(int i)
        {
            if(ConfigurationWindow != null)
                ConfigurationDrawer.DrawConfiguration(ConfigurationWindow);
        }

        private void SetUnlockCursor(int lockState, bool cursorVisible)
        {
            if (_curLockState != null)
            {
                // Do through reflection for unity 4 compat
                //Cursor.lockState = CursorLockMode.None;
                //Cursor.visible = true;
                if (_obsoleteCursor)
                    _curLockState.SetValue(null, Convert.ToBoolean(lockState), null);
                else
                    _curLockState.SetValue(null, lockState, null);

                _curVisible.SetValue(null, cursorVisible, null);
            }
        }
    }
}
