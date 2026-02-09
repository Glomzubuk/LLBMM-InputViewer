using System;
using System.Collections.Generic;
using LLScreen;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using LLBML;
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

        public ConfigEntry<int> selectViewingMode;
        public ConfigEntry<int> backgroundTransparency;
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

            excludeExpressions = Config.Bind<bool>("Toggles", "miniInputViewer", false);

            useTeamColors = Config.Bind<bool>("Toggles", "useTeamColors", false);
            enableColorSwapIntegration = Config.Bind<bool>("Toggles", "enableColorSwapIntegration", true);

            Config.Bind("gap", "mm_header_gap", 20, new ConfigDescription("", null, "modmenu_gap"));
            Config.Bind("localViewer", "mm_header_localViewer", "Local Viewer",
                new ConfigDescription("", null, "modmenu_header"));
            enableLocalViewer = Config.Bind<bool>("Toggles", "enableLocalViewer", false);
            trackLocalCPUs = Config.Bind<bool>("Toggles", "trackLocalCPUs", true);

            inputViewerPosition_main = Config.Bind<Vector2>("Position", "inputViewerPosition_main", new Vector2(-520f, -300f));

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
        }

        // API endpoint for other mods (e.g. spectate/replay mod)
        public bool ForceLocalViewers()
        {
            return false;
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

            PlayerInputWindows = new[] {inputWindowFfaP1, inputWindowFfaP2, inputWindowFfaP3, inputWindowFfaP4};
            AllInputWindows = new[] {inputWindowMain, inputWindow1v1Left, inputWindow1v1Right, inputWindowFfaP1, inputWindowFfaP2, inputWindowFfaP3, inputWindowFfaP4};

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
                "Local viewer windows are not draggable to prevent accidental moving, and each have their own saved position in the config"
            });

            if (ModDependenciesUtils.IsModLoaded(DEPENDENCY_COLORSWAP) && ColorSwapPlugin == null)
            {
                ColorSwapPlugin = BepInEx.Bootstrap.Chainloader.PluginInfos[DEPENDENCY_COLORSWAP]?.Instance;

                if (ColorSwapPlugin != null)
                {
                    Logger.LogInfo("Found soft dependency ColorSwap");
                    ColorSwapPlugin.Config.SettingChanged += ColorSwap_SettingChanged;

                    ColorSwap_UpdateTeamColors();
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

            IVStyle.UpdateTeamColors(new[] {p1Color, p2Color, p3Color, p4Color});
        }

        private void SettingChanged(object sender, SettingChangedEventArgs e)
        {
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

        private bool queueSave;
        void LateUpdate()
        {
            if (AllInputWindows == null) return;
            foreach (InputWindow window in AllInputWindows)
            {
                window.UpdateColor();
            }

            if (ModDependenciesUtils.InModOptions())
            {
                if (inputWindowMain.IsPositionUnsaved()) queueSave = true;
            }
            else if (queueSave)
            {
                inputWindowMain.SavePosition();
                Config.Save();
                LogGlobal.LogInfo("Saved InputViewer position");
                queueSave = false;
            }
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
            if (( (localPlayerCount == 1 || StateApi.CurrentGameMode == GameMode.TRAINING || (!enableLocalViewer.Value && !ForceLocalViewers())) && ViewingMode((ViewMode)selectViewingMode.Value) && InGame) || ModDependenciesUtils.InModOptions())
            {
                inputWindowMain.BindPlayer(GetFirstLocalPlayer(false));

                inputWindowMain.gameObject.SetActive(true);
                return;
            }

            if (!enableLocalViewer.Value && !ForceLocalViewers())
            {
                return;
            }

            if (localPlayerCount == 2 && ViewingMode((ViewMode)selectViewingMode.Value) && InGame)
            {
                Player leftPLayer = GetFirstLocalPlayer(true);
                Player rightPlayer = GetSecondLocalPlayer(leftPLayer, true);

                inputWindow1v1Left.BindPlayer(leftPLayer);
                inputWindow1v1Right.BindPlayer(rightPlayer);

                inputWindow1v1Left.gameObject.SetActive(!(leftPLayer.IsAI && !trackLocalCPUs.Value && !ForceLocalViewers()));
                inputWindow1v1Right.gameObject.SetActive(!(rightPlayer.IsAI && !trackLocalCPUs.Value && !ForceLocalViewers()));
            }
            else if (localPlayerCount > 2 && ViewingMode((ViewMode)selectViewingMode.Value) && InGame)
            {
                for (int playerIndex = 0; playerIndex < Player.MAX_PLAYERS; playerIndex++)
                {
                    Player player = Player.GetPlayer(playerIndex);

                    if (!IsLocalPlayer(player, true))
                    {
                        continue;
                    }

                    InputWindow window = PlayerInputWindows[playerIndex];
                    window.BindPlayer(player);
                    window.gameObject.SetActive(!(player.IsAI && !trackLocalCPUs.Value && !ForceLocalViewers()));
                }
            }
        }

        enum ViewMode
        {
            Off,
            Training,
            Local,
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
                case ViewMode.Local:
                    return !NetworkApi.IsOnline;
                case ViewMode.Online:
                    return NetworkApi.IsOnline;
                case ViewMode.All:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsLocalPlayer(Player player, bool allowCPUs)
        {
            bool isLocal = player.IsLocalPeer && player.IsInMatch;
            bool blockAI = player.IsAI && !allowCPUs;
            return isLocal && (!blockAI || ForceLocalViewers());
        }

        private int LocalPlayerCount
        {
            get
            {
                int count = 0;

                for (int playerIndex = 0; playerIndex < Player.MAX_PLAYERS; playerIndex++)
                {
                    Player player = Player.GetPlayer(playerIndex);

                    if (IsLocalPlayer(player, true))
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private Player GetFirstLocalPlayer(bool allowCPUs)
        {
            Player localPlayer = Player.GetPlayer(0);
            for (int playerIndex = 0; playerIndex < Player.MAX_PLAYERS; playerIndex++)
            {
                Player tempPlayer = Player.GetPlayer(playerIndex);
                if (IsLocalPlayer(tempPlayer, allowCPUs))
                {
                    localPlayer = tempPlayer;
                    break;
                }
            }

            return localPlayer;
        }

        private Player GetSecondLocalPlayer(Player firstPlayer, bool allowCPUs)
        {
            Player localPlayer = Player.GetPlayer(0);
            for (int playerIndex = firstPlayer.nr + 1; playerIndex < Player.MAX_PLAYERS; playerIndex++)
            {
                Player tempPlayer = Player.GetPlayer(playerIndex);
                if (IsLocalPlayer(tempPlayer, allowCPUs))
                {
                    localPlayer = tempPlayer;
                    break;
                }
            }

            return localPlayer;
        }

    }
}
