using System;
using System.Collections.Generic;
using LLModdingTools;
using LLScreen;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using LLBML;
using LLBML.Players;
using LLBML.States;
using LLBML.Networking;
using LLBML.Utils;

namespace InputViewer
{
    [BepInPlugin(PluginInfos.PLUGIN_ID, PluginInfos.PLUGIN_NAME, PluginInfos.PLUGIN_VERSION)]
    [BepInDependency(LLBML.PluginInfos.PLUGIN_ID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("no.mrgentle.plugins.llb.modmenu", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LLBlaze.exe")]
    class InputViewer : BaseUnityPlugin
    {
        
        public static InputViewer Instance { get; private set; } = null;
        
        private InputWindow InputWindow_Main;
        private InputWindow[] AllInputWindows;

        private float saveTimer;
        
        public ConfigEntry<int> selectViewingMode;
        public ConfigEntry<int> backgroundTransparency;
        public ConfigEntry<bool> scaleToResolution;
        public ConfigEntry<bool> excludeExpressions;
        
        public ConfigEntry<Vector2> inputViewerPosition_main;

        void ConfigInit()
        {
            selectViewingMode = Config.Bind<int>("General", "selectViewingMode", 4,
                new ConfigDescription("Viewing mode index", new AcceptableValueRange<int>(0, 4)));
            backgroundTransparency = Config.Bind<int>("General", "backgroundTransparency", 0,
                new ConfigDescription("Background transparency", new AcceptableValueRange<int>(0, 6)));

            scaleToResolution = Config.Bind<bool>("Toggles", "scaleWithResolution", false);
            excludeExpressions = Config.Bind<bool>("Toggles", "miniInputViewer", false);

            inputViewerPosition_main = Config.Bind<Vector2>("Position", "inputViewerPosition", new Vector2(30, GUITools.GUI_Height - 147));
        }

        void Awake()
        {
            Instance = this;
            IVStyle.ATInit();
            ConfigInit();

            InputWindow_Main = gameObject.AddComponent<InputWindow>();
            InputWindow_Main.Initialize(inputViewerPosition_main, scaleToResolution, excludeExpressions);
            
            AllInputWindows = [InputWindow_Main];
        }

        private void Start()
        {
            Logger.LogInfo("InputViewer Started");
            ModDependenciesUtils.RegisterToModMenu(this.Info, new List<String> {
                "<b>Select View Mode Index</b>:",
                "",
                "0 : <b>Off</b>",
                "1 : <b>Training Mode</b>",
                "2 : <b>Local Games</b>",
                "3 : <b>Online Games</b>",
                "4 : <b>All Games</b>"
            });
        }

        private void OnDestroy()
        {
            Logger.LogInfo("InputViewer Destroyed");
        }

        bool InGame => World.instance != null && (GameStates.GetCurrent() == GameState.GAME || GameStates.GetCurrent() == GameState.GAME_PAUSE) && !UIScreen.loadingScreenActive;
        
#if DEBUG
        //Method to Log all the active game objects
        void PrintAllGameObjects()
        {
            string txt = "";
            foreach (var name in FindObjectsOfType<GameObject>())
            {
                string str = (name.transform.parent != null) ? name.transform.parent.gameObject.name : "NO_PARENT";
                txt += $"{str}/{name.name}\n";
            }
            Debug.Log(txt);
        }
#endif

        void Auto_Save()
        {
            bool needsSave = false;
            
            foreach (InputWindow window in AllInputWindows)
            {
                if (window.IsPositionUnsaved())
                {
                    needsSave = true;
                }
            }

            if (!needsSave)
            {
                return;
            }

            saveTimer += Time.deltaTime;
            if (CountDown(ref saveTimer, 5f))
            {
                foreach (InputWindow window in AllInputWindows)
                {
                    window.SavePosition();
                }

                Config.Save();
            }
        }

        static bool CountDown(ref float timer, float duration)
        {
            if (timer > 0 && timer < duration) // Cooldown in seconds
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0;
            }
            return timer == 0;
        }

        void Update()
        {
            if (ModDependenciesUtils.InModOptions())
            {
                Auto_Save();
            }
            /*
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                Save_InputViewerPosition();
            }

            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                Load_InputViewerPosition();
            }
#endif
            */

            //Experimental Code - not much to see here.
            /*
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                Cursor.visible = !Cursor.visible;
                GameObject header = new GameObject("header", typeof(Image), typeof(LayoutElement));
                GameObject body = new GameObject("body", typeof(Image), typeof(LayoutElement));
                GameObject frame = new GameObject("frame", typeof(VerticalLayoutGroup));
                GameObject panel = new GameObject("panel", typeof(Image));
                GameObject canvas = new GameObject("canvas", typeof(Canvas), typeof(CanvasScaler));

                panel.transform.SetParent(canvas.transform);
                frame.transform.SetParent(panel.transform);
                header.transform.SetParent(frame.transform);
                body.transform.SetParent(frame.transform);

                header.GetComponent<LayoutElement>().minHeight = 50;
                body.GetComponent<LayoutElement>().minHeight = 100;
                body.GetComponent<LayoutElement>().preferredHeight = 999;

                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvas.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;
                canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1280, 720);

                panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
                panel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                panel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
                panel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

                RectTransform frameRect = frame.GetComponent<RectTransform>();
                VerticalLayoutGroup frameVertGroup = frame.GetComponent<VerticalLayoutGroup>();
                frameRect.anchorMin = new Vector2(0, 0);
                frameRect.anchorMax = new Vector2(0, 0);
                frameRect.pivot = new Vector2(0.5f, 0.5f);
                frameRect.sizeDelta = new Vector2(550, 300);
                frameRect.position = new Vector2(300, 203);
                frameVertGroup.spacing = 10;
                frameVertGroup.childControlHeight = true;
                frameVertGroup.childControlWidth = true;
                frameVertGroup.childForceExpandHeight = true;
                frameVertGroup.childForceExpandWidth = true;
            }
#endif
            */
        }

        private void LateUpdate()
        {
            InputWindow_Main.BindPlayer(GetFirstLocalPlayer());
            
            if (ViewingMode((ViewMode)selectViewingMode.Value) || ModDependenciesUtils.InModOptions())
            {
                InputWindow_Main.enabled = true;
            }
            else
            {
                InputWindow_Main.enabled = false;
            }
        }

        enum ViewMode
        {
            Off,
            Training,
            local,
            Online,
            All,
        }

        bool ViewingMode(ViewMode selectedView)
        {
            switch (selectedView)
            {
                case ViewMode.Off:
                    return false;
                case ViewMode.Training:
                    return StateApi.CurrentGameMode == GameMode.TRAINING && InGame;
                case ViewMode.local:
                    return !NetworkApi.IsOnline && InGame;
                case ViewMode.Online:
                    return NetworkApi.IsOnline && InGame;
                case ViewMode.All:
                    return InGame;
                default:
                    return false;
            }
        }

        private bool IsLocalPlayer(Player player)
        {
            return player.IsLocalPeer && player.IsInMatch && !player.IsAI;
        }

        private Player GetFirstLocalPlayer()
        {
            Player localPlayer = Player.GetPlayer(0);
            for (int playerIndex = 0; playerIndex < Player.MAX_PLAYERS; playerIndex++)
            {
                Player tempPlayer = Player.GetPlayer(playerIndex);
                if (IsLocalPlayer(tempPlayer))
                {
                    localPlayer = tempPlayer;
                    break;
                }
            }

            return localPlayer;
        }

    }
}
