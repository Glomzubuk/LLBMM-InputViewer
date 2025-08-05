using System;
using System.Collections.Generic;
using LLHandlers;
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

#pragma warning disable IDE0051 // Remove unused private members
        public const string modVersion = "1.2.0";
        private const string repositoryOwner = "Daioutzu";
        private const string repositoryName = "LLBMM-InputViewer";
#pragma warning restore IDE0051

        public static InputViewer Instance { get; private set; } = null;


        private float saveTimer;

        void Awake()
        {
            Instance = this;
            IVStyle.ATInit();
            ConfigInit();
            inputRect.position = inputViewerPosition.Value;
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

        public ConfigEntry<int> selectViewingMode;
        public ConfigEntry<int> backgroundTransparency;
        public ConfigEntry<bool> scaleToResolution;
        public ConfigEntry<bool> excludeExpressions;
        public ConfigEntry<Vector2> inputViewerPosition;

        void ConfigInit()
        {
            selectViewingMode = Config.Bind<int>("General", "selectViewingMode", 4,
                new ConfigDescription("Viewing mode index", new AcceptableValueRange<int>(0, 4)));
            backgroundTransparency = Config.Bind<int>("General", "backgroundTransparency", 0,
                new ConfigDescription("Background transparency", new AcceptableValueRange<int>(0, 6)));

            scaleToResolution = Config.Bind<bool>("Toggles", "scaleWithResolution", false);
            excludeExpressions = Config.Bind<bool>("Toggles", "miniInputViewer", false);

            inputViewerPosition = Config.Bind<Vector2>("Position", "inputViewerPosition", new Vector2(30, GUITools.GUI_Height - 147));
        }
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
            if (inputRect.position != inputViewerPosition.Value)
            {
                saveTimer += Time.deltaTime;
                if (CountDown(ref saveTimer, 5f))
                {
                    inputViewerPosition.Value = inputRect.position;
                    Config.Save();
                }
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

        Vector2 inputSizeMini = new Vector2(165, 117);
        Vector2 inputSize = new Vector2(300, 117);
        Rect inputRect = new Rect(30, GUITools.GUI_Height - 147, 300, 117);

        void OnGUI()
        {
            if (scaleToResolution.Value)
            {
                GUITools.ScaleGUIToViewPort();
            }
            //TODO Remove hard dep to modmenu for LLBMM compatibility
            if (ViewingMode((ViewMode)selectViewingMode.Value) || ModDependenciesUtils.InModOptions())
            {
                inputRect.size = excludeExpressions.Value ? inputSizeMini : inputSize;

                inputRect = GUILayout.Window(102289, inputRect, InputWindow, "", IVStyle.InputViewerBG);
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

        void InputWindow(int wId)
        {
            Player player = Player.GetPlayer(0);
            for (int i = 0; i < 4; i++)
            {
                var tmpPlayer = Player.GetPlayer(i);
                if (tmpPlayer.IsLocalPeer && tmpPlayer.IsInMatch)
                {
                    player = tmpPlayer;
                    break;
                }
            };

            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                font = IVStyle.inputViewerFont,
                fontSize = 20,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(5, 5, 6, 16),
                padding = new RectOffset(0, 0, 0, 0),
                wordWrap = false,
                clipping = TextClipping.Overflow,
            };

            GUIStyle border = new GUIStyle()
            {
                padding = new RectOffset(3, 3, 0, 0),
            };

            GUI.DragWindow();
            GUILayoutOption[] gUILayoutOption = new GUILayoutOption[]
            {
                GUILayout.MinWidth(inputRect.size.x),
                GUILayout.MinHeight(inputRect.size.y),
                GUILayout.MaxWidth(inputRect.size.x),
                GUILayout.MaxHeight(inputRect.size.y),
            };
            GUILayout.BeginHorizontal(border, gUILayoutOption);

            GUILayout.BeginVertical();
            GUILayout.Label(excludeExpressions.Value ? "Inputs" : "Input Viewer", headerStyle);
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Toggle(InputHandler.GetInput(player, InputAction.JUMP), "", IVStyle.JumpStyle);
            GUILayout.Toggle(InputHandler.GetInput(player, InputAction.UP), "", IVStyle.DirUpStyle);
            //GUILayout.Toggle(InputHandler.currentInput[player.CJFLMDNNMIE, InputAction.ActionToIndex(InputAction.JUMP)] == 100, "", IVStyle.JumpStyle);
            //GUILayout.Toggle(InputHandler.currentInput[player.CJFLMDNNMIE, InputAction.ActionToIndex(InputAction.UP)] == 100, "", IVStyle.DirUpStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Toggle(InputHandler.GetInput(player, InputAction.LEFT), "", IVStyle.DirLefStyle);
            GUILayout.Toggle(InputHandler.GetInput(player, InputAction.DOWN), "", IVStyle.DirDwnStyle);
            GUILayout.Toggle(InputHandler.GetInput(player, InputAction.RIGHT), "", IVStyle.DirRigStyle);
            //GUILayout.Toggle(InputHandler.currentInput[player.CJFLMDNNMIE, InputAction.ActionToIndex(InputAction.LEFT)] == 100, "", IVStyle.DirLefStyle);
            //GUILayout.Toggle(InputHandler.currentInput[player.CJFLMDNNMIE, InputAction.ActionToIndex(InputAction.DOWN)] == 100, "", IVStyle.DirDwnStyle);
            //GUILayout.Toggle(InputHandler.currentInput[player.CJFLMDNNMIE, InputAction.ActionToIndex(InputAction.RIGHT)] == 100, "", IVStyle.DirRigStyle);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Toggle(InputHandler.GetInput(player, InputAction.SWING), "", IVStyle.SwingStyle); //Swing Button
            GUILayout.Toggle(InputHandler.GetInput(player, InputAction.BUNT), "", IVStyle.BuntStyle); //Bunt Button
            //GUILayout.Toggle(InputHandler.currentInput[player.CJFLMDNNMIE, InputAction.ActionToIndex(InputAction.SWING)] == 100, "", IVStyle.SwingStyle); //Swing Button
            //GUILayout.Toggle(InputHandler.currentInput[player.CJFLMDNNMIE, InputAction.ActionToIndex(InputAction.BUNT)] == 100, "", IVStyle.BuntStyle); //Bunt Button
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Toggle(InputHandler.GetInput(player, InputAction.GRAB), "", IVStyle.GrabStyle); //Grab Button
            GUILayout.Toggle(InputHandler.GetInput(player, InputAction.TAUNT), "", IVStyle.TauntStyle); //Taunt Button
            //GUILayout.Toggle(InputHandler.currentInput[player.CJFLMDNNMIE, InputAction.ActionToIndex(InputAction.GRAB)] == 100, "", IVStyle.GrabStyle); //Grab Button
            //GUILayout.Toggle(InputHandler.currentInput[player.CJFLMDNNMIE, InputAction.ActionToIndex(InputAction.TAUNT)] == 100, "", IVStyle.TauntStyle); //Taunt Button
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (excludeExpressions.Value == false)
            {
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Toggle(InputHandler.GetInput(player, InputAction.EXPRESS_UP), "", IVStyle.ExpNiceStyle);
                //GUILayout.Toggle(InputHandler.currentInput[player.CJFLMDNNMIE, InputAction.ActionToIndex(InputAction.EXPRESS_UP)] == 100, "", IVStyle.ExpNiceStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Toggle(InputHandler.GetInput(player, InputAction.EXPRESS_LEFT), "", IVStyle.ExpOopsStyle);
                GUILayout.Toggle(InputHandler.GetInput(player, InputAction.EXPRESS_RIGHT), "", IVStyle.ExpWowStyle);
                //GUILayout.Toggle(InputHandler.currentInput[player.CJFLMDNNMIE, InputAction.ActionToIndex(InputAction.EXPRESS_LEFT)] == 100, "", IVStyle.ExpOopsStyle);
                //GUILayout.Toggle(InputHandler.currentInput[player.CJFLMDNNMIE, InputAction.ActionToIndex(InputAction.EXPRESS_RIGHT)] == 100, "", IVStyle.ExpWowStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Toggle(InputHandler.GetInput(player, InputAction.EXPRESS_DOWN), "", IVStyle.ExpBringItStyle);
                //GUILayout.Toggle(InputHandler.currentInput[player.CJFLMDNNMIE, InputAction.ActionToIndex(InputAction.EXPRESS_DOWN)] == 100, "", IVStyle.ExpBringItStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

    }
}
