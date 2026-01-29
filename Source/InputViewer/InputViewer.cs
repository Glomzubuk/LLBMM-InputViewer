using System;
using System.Collections.Generic;
using LLScreen;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using LLBML;
using LLBML.GameEvents;
using LLBML.Players;
using LLBML.States;
using LLBML.Networking;
using LLBML.Utils;
using LLGUI;

namespace InputViewer
{
    [BepInPlugin(PluginInfos.PLUGIN_ID, PluginInfos.PLUGIN_NAME, PluginInfos.PLUGIN_VERSION)]
    [BepInDependency(LLBML.PluginInfos.PLUGIN_ID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("no.mrgentle.plugins.llb.modmenu", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(DEPENDENCY_COLORSWAP, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LLBlaze.exe")]
    class InputViewer : BaseUnityPlugin
    {

        public const string DEPENDENCY_COLORSWAP = "com.gitlab.axolotlll.llb-colorswap";
        public BaseUnityPlugin ColorSwapPlugin = null;
        
        public static InputViewer Instance { get; private set; } = null;
        public static ManualLogSource LogGlobal { get; private set; }

        private RectTransform inputWindowContainer;
        private InputWindow inputWindowMain;
        private InputWindow inputWindow1v1Left;
        private InputWindow inputWindow1v1Right;
        private InputWindow inputWindowFfaP1;
        private InputWindow inputWindowFfaP2;
        private InputWindow inputWindowFfaP3;
        private InputWindow inputWindowFfaP4;
        private InputWindow[] PlayerInputWindows;
        private InputWindow[] AllInputWindows;

        private bool inputWindowsCreated = false;

        private float saveTimer;
        private bool regenerateColorTextures;
        
        public ConfigEntry<int> selectViewingMode;
        public ConfigEntry<int> backgroundTransparency;
        public ConfigEntry<bool> scaleToResolution;
        public ConfigEntry<bool> excludeExpressions;
        public ConfigEntry<bool> useTeamColors;
        public ConfigEntry<bool> enableColorSwapIntegration;

        public ConfigEntry<bool> enableLocalViewer;
        public ConfigEntry<bool> trackLocalCPUs;
        
        public ConfigEntry<Vector2> inputViewerPosition_main;
        public ConfigEntry<Vector2> inputViewerPosition_1v1_left;
        public ConfigEntry<Vector2> inputViewerPosition_1v1_right;
        public ConfigEntry<Vector2> inputViewerPosition_FFA_P1;
        public ConfigEntry<Vector2> inputViewerPosition_FFA_P2;
        public ConfigEntry<Vector2> inputViewerPosition_FFA_P3;
        public ConfigEntry<Vector2> inputViewerPosition_FFA_P4;

        void ConfigInit()
        {
            selectViewingMode = Config.Bind<int>("General", "selectViewingMode", 4,
                new ConfigDescription("Viewing mode index", new AcceptableValueRange<int>(0, 4)));
            backgroundTransparency = Config.Bind<int>("General", "backgroundTransparency", 0,
                new ConfigDescription("Background transparency", new AcceptableValueRange<int>(0, 6)));

            scaleToResolution = Config.Bind<bool>("Toggles", "scaleWithResolution", true);
            excludeExpressions = Config.Bind<bool>("Toggles", "miniInputViewer", false);
            
            useTeamColors = Config.Bind<bool>("Toggles", "useTeamColors", false);
            enableColorSwapIntegration = Config.Bind<bool>("Toggles", "enableColorSwapIntegration", true);

            Config.Bind("gap", "mm_header_gap", 20, new ConfigDescription("", null, "modmenu_gap"));
            Config.Bind("localViewer", "mm_header_localViewer", "Local Viewer",
                new ConfigDescription("", null, "modmenu_header"));
            enableLocalViewer = Config.Bind<bool>("Toggles", "enableLocalViewer", false);
            trackLocalCPUs = Config.Bind<bool>("Toggles", "trackLocalCPUs", true);

            inputViewerPosition_main = Config.Bind<Vector2>("Position", "inputViewerPosition", new Vector2(-520f, -300f));
            
            inputViewerPosition_1v1_left = Config.Bind<Vector2>("Position", "inputViewerPosition_1v1_left", new Vector2(-520f, -300f));
            inputViewerPosition_1v1_right = Config.Bind<Vector2>("Position", "inputViewerPosition_1v1_right", new Vector2(520f, -300f));

            inputViewerPosition_FFA_P1 = Config.Bind<Vector2>("Position", "inputViewerPosition_FFA_P1",
                new Vector2(-570f, -300f));
            inputViewerPosition_FFA_P2 = Config.Bind<Vector2>("Position", "inputViewerPosition_FFA_P2",
                new Vector2(-450f, -300f));
            inputViewerPosition_FFA_P3 = Config.Bind<Vector2>("Position", "inputViewerPosition_FFA_P3",
                new Vector2(450f, -300f));
            inputViewerPosition_FFA_P4 = Config.Bind<Vector2>("Position", "inputViewerPosition_FFA_P4",
                new Vector2(570f, -300f));
        }

        void Awake()
        {
            Instance = this;
            LogGlobal = Logger;
            IVStyle.ATInit();
            ConfigInit();
            Config.SettingChanged += SettingChanged;

            GameStateEvents.OnStateChange += StateChanged;
        }

        private void CreateInputWindows()
        {
            inputWindowContainer = LLControl.CreatePanel(UIScreen.tfUIRoot, "Input Windows", 0, 0);
            inputWindowContainer.anchorMin = new Vector2(0f, 0f);
            inputWindowContainer.anchorMax = new Vector2(1f, 1f);
            inputWindowContainer.localPosition = Vector2.zero;
            
            inputWindow1v1Left = InputWindow.Create(inputWindowContainer, "inputWindow1v1Left", inputViewerPosition_1v1_left, false, false);
            inputWindow1v1Right = InputWindow.Create(inputWindowContainer, "inputWindow1v1Right", inputViewerPosition_1v1_right, false, false);
            
            inputWindowFfaP1 = InputWindow.Create(inputWindowContainer, "inputWindowFfaP1", inputViewerPosition_FFA_P1, true, false);
            inputWindowFfaP2 = InputWindow.Create(inputWindowContainer, "inputWindowFfaP2", inputViewerPosition_FFA_P2, true, false);
            inputWindowFfaP3 = InputWindow.Create(inputWindowContainer, "inputWindowFfaP3", inputViewerPosition_FFA_P3, true, false);
            inputWindowFfaP4 = InputWindow.Create(inputWindowContainer, "inputWindowFfaP4", inputViewerPosition_FFA_P4, true, false);
            
            inputWindowMain = InputWindow.Create(inputWindowContainer, "inputWindowMain", inputViewerPosition_main, excludeExpressions.Value, true);

            PlayerInputWindows = [inputWindowFfaP1, inputWindowFfaP2, inputWindowFfaP3, inputWindowFfaP4];
            AllInputWindows = [inputWindowMain, inputWindow1v1Left, inputWindow1v1Right, inputWindowFfaP1, inputWindowFfaP2, inputWindowFfaP3, inputWindowFfaP4];

            inputWindowsCreated = true;
        }

        private void Start()
        {
            Logger.LogInfo("InputViewer Started");
            ModDependenciesUtils.RegisterToModMenu(this.Info, new List<String> {
                "<b>Select View Mode Index</b>:",
                "0 : <b>Off</b>",
                "1 : <b>Training Mode</b>",
                "2 : <b>Local Games</b>",
                "3 : <b>Online Games</b>",
                "4 : <b>All Games</b>",
                "",
                "<b>Enable local viewer</b>: shows an individual input viewer window for each player in LAN games",
                "Local viewer windows are not draggable to prevent accidental moving, and each have their own saved position in the config",
                "Highly recommended to also enable '<b>Scale With Resolution</b>' when enabling local viewer"
            });

            if (ModDependenciesUtils.IsModLoaded(DEPENDENCY_COLORSWAP) && ColorSwapPlugin == null)
            {
                ColorSwapPlugin = BepInEx.Bootstrap.Chainloader.PluginInfos[DEPENDENCY_COLORSWAP]?.Instance;

                if (ColorSwapPlugin != null)
                {
                    Logger.LogInfo("Found soft dependency ColorSwap");
                    ColorSwapPlugin.Config.SettingChanged += ColorSwap_SettingChanged;
                    
                    ColorSwap_UpdateTeamColors();
                    RegenerateColorTextures();
                }
            }
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

        private ConfigEntry<bool> colorSwap_p1Enabled;
        private ConfigEntry<int> colorSwap_p1R;
        private ConfigEntry<int> colorSwap_p1G;
        private ConfigEntry<int> colorSwap_p1B;

        private ConfigEntry<bool> colorSwap_p2Enabled;
        private ConfigEntry<int> colorSwap_p2R;
        private ConfigEntry<int> colorSwap_p2G;
        private ConfigEntry<int> colorSwap_p2B;
        
        private ConfigEntry<bool> colorSwap_p3Enabled;
        private ConfigEntry<int> colorSwap_p3R;
        private ConfigEntry<int> colorSwap_p3G;
        private ConfigEntry<int> colorSwap_p3B;
        
        private ConfigEntry<bool> colorSwap_p4Enabled;
        private ConfigEntry<int> colorSwap_p4R;
        private ConfigEntry<int> colorSwap_p4G;
        private ConfigEntry<int> colorSwap_p4B;

        private void ColorSwap_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            regenerateColorTextures = true;
            ColorSwap_UpdateTeamColors();
        }

        private void ColorSwap_UpdateTeamColors()
        {
            ColorSwapPlugin.Config.TryGetEntry("Toggles", "p1Enabled", out colorSwap_p1Enabled);
            ColorSwapPlugin.Config.TryGetEntry("Tuning", "p1R", out colorSwap_p1R);
            ColorSwapPlugin.Config.TryGetEntry("Tuning", "p1G", out colorSwap_p1G);
            ColorSwapPlugin.Config.TryGetEntry("Tuning", "p1B", out colorSwap_p1B);
            Color p1Color = colorSwap_p1Enabled.Value && enableColorSwapIntegration.Value ? new Color(colorSwap_p1R.Value/255f, colorSwap_p1G.Value/255f, colorSwap_p1B.Value/255f) : Color.clear;
            
            ColorSwapPlugin.Config.TryGetEntry("Toggles", "p2Enabled", out colorSwap_p2Enabled);
            ColorSwapPlugin.Config.TryGetEntry("Tuning", "p2R", out colorSwap_p2R);
            ColorSwapPlugin.Config.TryGetEntry("Tuning", "p2G", out colorSwap_p2G);
            ColorSwapPlugin.Config.TryGetEntry("Tuning", "p2B", out colorSwap_p2B);
            Color p2Color = colorSwap_p2Enabled.Value && enableColorSwapIntegration.Value ? new Color(colorSwap_p2R.Value/255f, colorSwap_p2G.Value/255f, colorSwap_p2B.Value/255f) : Color.clear;
            
            ColorSwapPlugin.Config.TryGetEntry("Toggles", "p3Enabled", out colorSwap_p3Enabled);
            ColorSwapPlugin.Config.TryGetEntry("Tuning", "p3R", out colorSwap_p3R);
            ColorSwapPlugin.Config.TryGetEntry("Tuning", "p3G", out colorSwap_p3G);
            ColorSwapPlugin.Config.TryGetEntry("Tuning", "p3B", out colorSwap_p3B);
            Color p3Color = colorSwap_p3Enabled.Value && enableColorSwapIntegration.Value ? new Color(colorSwap_p3R.Value/255f, colorSwap_p3G.Value/255f, colorSwap_p3B.Value/255f) : Color.clear;
            
            ColorSwapPlugin.Config.TryGetEntry("Toggles", "p4Enabled", out colorSwap_p4Enabled);
            ColorSwapPlugin.Config.TryGetEntry("Tuning", "p4R", out colorSwap_p4R);
            ColorSwapPlugin.Config.TryGetEntry("Tuning", "p4G", out colorSwap_p4G);
            ColorSwapPlugin.Config.TryGetEntry("Tuning", "p4B", out colorSwap_p4B);
            Color p4Color = colorSwap_p4Enabled.Value && enableColorSwapIntegration.Value ? new Color(colorSwap_p4R.Value/255f, colorSwap_p4G.Value/255f, colorSwap_p4B.Value/255f) : Color.clear;
            
            IVStyle.UpdateTeamColors([p1Color, p2Color, p3Color, p4Color]);
        }
        
        private void SettingChanged(object sender, SettingChangedEventArgs e)
        {
            regenerateColorTextures = true;

            if (ColorSwapPlugin != null)
            {
                ColorSwap_UpdateTeamColors();
            }

            if (inputWindowMain.isMiniSize != excludeExpressions.Value)
            {
                InputWindow windowOld = inputWindowMain;
                InputWindow windowNew = InputWindow.Create(inputWindowContainer, "inputWindowMain", inputViewerPosition_main, excludeExpressions.Value, true);
                windowNew.BindPlayer(windowOld.boundPlayer);
                
                Destroy(windowOld.gameObject);
                inputWindowMain = windowNew;
                AllInputWindows[0] = inputWindowMain;
            }
        }

        void StateChanged(object sender, OnStateChangeArgs e)
        {
            if (!regenerateColorTextures)
            {
                return;
            }
            
            RegenerateColorTextures();
            regenerateColorTextures = false;
        }

        private void RegenerateColorTextures()
        {
            Logger.LogInfo("Regenerating color textures");
            IVStyle.CreateColorBGAssets();
        }
        
        void Auto_Save()
        {
            if (!inputWindowMain.IsPositionUnsaved()) return;

            saveTimer += Time.deltaTime;
            if (CountDown(ref saveTimer, 5f))
            {
                inputWindowMain.SavePosition();
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

        void LateUpdate()
        {
            if (AllInputWindows == null) return;
            foreach (InputWindow window in AllInputWindows)
            {
                window.UpdateColor();
            }
            
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

        private void Update()
        {
            if (!inputWindowsCreated)
            {
                if (UIScreen.tfUIRoot != null) CreateInputWindows();
            }
            
            if (!inputWindowsCreated) return;

            inputWindowContainer.SetAsLastSibling();
            foreach (InputWindow window in AllInputWindows)
            {
                window.gameObject.SetActive(false);
            }
            
            int localPlayerCount = LocalPlayerCount;

            // training, local with CPUs, online
            if (( (localPlayerCount == 1 || !enableLocalViewer.Value) && ViewingMode((ViewMode)selectViewingMode.Value) && InGame) || ModDependenciesUtils.InModOptions())
            {
                //_legacyInputWindowMain.BindPlayer(GetFirstLocalPlayer());
                inputWindowMain.BindPlayer(GetFirstLocalPlayer());
            
                //_legacyInputWindowMain.enabled = true;
                inputWindowMain.gameObject.SetActive(true);
                return;
            }

            if (!enableLocalViewer.Value)
            {
                return;
            }
            
            if (localPlayerCount == 2 && ViewingMode((ViewMode)selectViewingMode.Value) && InGame)
            {
                Player leftPLayer = GetFirstLocalPlayer();
                Player rightPlayer = GetSecondLocalPlayer(leftPLayer);
                
                //_legacyInputWindow1V1Left.BindPlayer(leftPLayer);
                //_legacyInputWindow1V1Right.BindPlayer(rightPlayer);
                inputWindow1v1Left.BindPlayer(leftPLayer);
                inputWindow1v1Right.BindPlayer(rightPlayer);

                //_legacyInputWindow1V1Left.enabled = true;
                //_legacyInputWindow1V1Right.enabled = true;
                inputWindow1v1Left.gameObject.SetActive(true);
                inputWindow1v1Right.gameObject.SetActive(true);
            }
            else if (localPlayerCount > 2 && ViewingMode((ViewMode)selectViewingMode.Value) && InGame)
            {
                for (int playerIndex = 0; playerIndex < Player.MAX_PLAYERS; playerIndex++)
                {
                    Player player = Player.GetPlayer(playerIndex);

                    if (!IsLocalPlayer(player))
                    {
                        continue;
                    }

                    //LegacyInputWindow window = PlayerLegacyInputWindows[playerIndex];
                    //window.BindPlayer(player);
                    //window.enabled = true;
                    InputWindow window = PlayerInputWindows[playerIndex];
                    window.BindPlayer(player);
                    window.gameObject.SetActive(true);
                }
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
                    return StateApi.CurrentGameMode == GameMode.TRAINING;
                case ViewMode.local:
                    return !NetworkApi.IsOnline;
                case ViewMode.Online:
                    return NetworkApi.IsOnline;
                case ViewMode.All:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsLocalPlayer(Player player)
        {
            return player.IsLocalPeer && player.IsInMatch && (!player.IsAI || trackLocalCPUs.Value);
        }
        
        private int LocalPlayerCount
        {
            get
            {
                int count = 0;
                
                for (int playerIndex = 0; playerIndex < Player.MAX_PLAYERS; playerIndex++)
                {
                    Player player = Player.GetPlayer(playerIndex);

                    if (IsLocalPlayer(player))
                    {
                        count++;
                    }
                }

                return count;
            }
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
        
        private Player GetSecondLocalPlayer(Player firstPlayer)
        {
            Player localPlayer = Player.GetPlayer(0);
            for (int playerIndex = firstPlayer.nr + 1; playerIndex < Player.MAX_PLAYERS; playerIndex++)
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
