using BepInEx.Configuration;
using LLBML.Players;
using LLHandlers;
using LLModdingTools;
using UnityEngine;

namespace InputViewer
{
    public class InputWindow : MonoBehaviour
    {
        public static readonly Vector2 inputSizeMini = new Vector2(165, 117);
        public static readonly Vector2 inputSize = new Vector2(300, 117);

        private static int _windowID = 10000;
        private static int WindowID => _windowID++;
        
        private static ConfigEntry<bool> scaleToResolution;
        private static ConfigEntry<bool> isMiniSize;

        private Player boundPlayer;
        private ConfigEntry<Vector2> savedPosition;
        private int localWindowID;
        private bool forceMiniSize;
        private bool isDraggable;

        public Rect inputRect = new Rect(0, 0, inputSize.x, inputSize.y);

        public static void BindConfigs(ConfigEntry<bool> scaleToResolution, ConfigEntry<bool> isMiniSize)
        {
            InputWindow.scaleToResolution = scaleToResolution;
            InputWindow.isMiniSize = isMiniSize;
        }

        public void Initialize(ConfigEntry<Vector2> savedPosition, bool isDraggable = true, bool forceMiniSize = false)
        {
            this.savedPosition = savedPosition;
            this.isDraggable = isDraggable;
            this.forceMiniSize = forceMiniSize;
            localWindowID = WindowID;
            
            inputRect.position = savedPosition.Value;
            enabled = false;
        }

        public void BindPlayer(Player player)
        {
            boundPlayer = player;
        }

        public bool IsPositionUnsaved()
        {
            return inputRect.position != savedPosition.Value;
        }

        public void SavePosition()
        {
            savedPosition.Value = inputRect.position;
        }

        public void OnGUI()
        {
            if (boundPlayer == null)
            {
                return;
            }
            
            if (scaleToResolution.Value)
            {
                GUITools.ScaleGUIToViewPort();
            }

            GUIStyle colorStyle = IVStyle.GetBGStyle(boundPlayer.Team);
            
            inputRect.size = (isMiniSize.Value || forceMiniSize) ? inputSizeMini : inputSize;
            inputRect = GUILayout.Window(localWindowID, inputRect, DrawWindow, "", colorStyle);
        }

        private void DrawWindow(int windowID)
        {
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                font = IVStyle.inputViewerFont,
                fontSize = 20,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(5, 5, 6, 16),
                padding = new RectOffset(0, 0, 0, 0),
                wordWrap = false,
                clipping = TextClipping.Overflow
            };

            GUIStyle borderStyle = new GUIStyle()
            {
                padding = new RectOffset(3, 3, 0, 0)
            };

            GUILayoutOption[] layoutOptions =
            [
                GUILayout.MinWidth(inputRect.size.x),
                GUILayout.MinHeight(inputRect.size.y),
                GUILayout.MaxWidth(inputRect.size.x),
                GUILayout.MaxHeight(inputRect.size.y)
            ];

            if (isDraggable)
            {
                GUI.DragWindow();
            }

            GUILayout.BeginHorizontal(borderStyle, layoutOptions);
            
            GUILayout.BeginVertical();
            
            GUILayout.Label((isMiniSize.Value || forceMiniSize) ? "Inputs" : "Input Viewer", headerStyle);
            GUILayout.BeginHorizontal();
            
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Toggle(InputHandler.GetInput(boundPlayer, InputAction.JUMP), "", IVStyle.JumpStyle);
            GUILayout.Toggle(InputHandler.GetInput(boundPlayer, InputAction.UP), "", IVStyle.DirUpStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Toggle(InputHandler.GetInput(boundPlayer, InputAction.LEFT), "", IVStyle.DirLefStyle);
            GUILayout.Toggle(InputHandler.GetInput(boundPlayer, InputAction.DOWN), "", IVStyle.DirDwnStyle);
            GUILayout.Toggle(InputHandler.GetInput(boundPlayer, InputAction.RIGHT), "", IVStyle.DirRigStyle);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Toggle(InputHandler.GetInput(boundPlayer, InputAction.SWING), "", IVStyle.SwingStyle);
            GUILayout.Toggle(InputHandler.GetInput(boundPlayer, InputAction.BUNT), "", IVStyle.BuntStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Toggle(InputHandler.GetInput(boundPlayer, InputAction.GRAB), "", IVStyle.GrabStyle);
            GUILayout.Toggle(InputHandler.GetInput(boundPlayer, InputAction.TAUNT), "", IVStyle.TauntStyle);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (!(isMiniSize.Value || forceMiniSize)) // should include taunt expressions
            {
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Toggle(InputHandler.GetInput(boundPlayer, InputAction.EXPRESS_UP), "", IVStyle.ExpNiceStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Toggle(InputHandler.GetInput(boundPlayer, InputAction.EXPRESS_LEFT), "", IVStyle.ExpOopsStyle);
                GUILayout.Toggle(InputHandler.GetInput(boundPlayer, InputAction.EXPRESS_RIGHT), "", IVStyle.ExpWowStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Toggle(InputHandler.GetInput(boundPlayer, InputAction.EXPRESS_DOWN), "", IVStyle.ExpBringItStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
    }
}