using System.Collections.Generic;
using System.IO;
using LLBML.Players;
using UnityEngine;

namespace InputViewer
{
    public static class IVStyle
    {

        readonly static string bundlesFolder = Path.Combine(Path.GetDirectoryName(InputViewer.Instance.Info.Location), "Bundles");

        public static AssetBundle uiBundle;
        public static Font inputViewerFont;
        public static Dictionary<string, Texture2D> uiTexture2DAssets = new Dictionary<string, Texture2D>();
        public static Texture2D viewerBG;

        public static Color[] TeamColors = [Color.red, Color.blue, Color.yellow, Color.green];
        private static string[] transparencyNames = ["", "_90", "_80", "_70", "_60", "_50"];

        static void LoadAssets()
        {
            uiBundle = AssetBundle.LoadFromFile(Path.Combine(bundlesFolder, "ui"));
            Texture2D[] texture = uiBundle.LoadAllAssets<Texture2D>();
            for (int i = 0; i < texture.Length; i++)
            {
                uiTexture2DAssets.Add(texture[i].name, texture[i]);
            }
            uiTexture2DAssets.Add("BlankTexture", new Texture2D(0, 0));
            inputViewerFont = uiBundle.LoadAsset<Font>("assets/ui/elements.ttf");
            foreach (var s in IVStyle.uiTexture2DAssets) Debug.Log(s.Key);

            CreateColorBGAssets();
        }

        public static void ATInit()
        {
            LoadAssets();
        }

        private static void CreateColorBGAssets()
        {
            for (int teamColorIndex = 0; teamColorIndex < TeamColors.Length; teamColorIndex++)
            {
                for (int transparencyIndex = 0; transparencyIndex < transparencyNames.Length; transparencyIndex++)
                {
                    string sourceTextureName = "ViewerBG";
                    sourceTextureName += transparencyNames[transparencyIndex];
                    string destinationTextureName = sourceTextureName + "-Team" + teamColorIndex;
                    
                    uiTexture2DAssets.Remove(destinationTextureName);

                    Texture2D texture = null;
                    SetCopy(ref texture, uiTexture2DAssets[sourceTextureName]);
                    SetColor(ref texture, TeamColors[teamColorIndex]);
                    
                    uiTexture2DAssets.Add(destinationTextureName, texture);
                }
            }
        }

        private static string GetTextureName(int transparencyIndex, Team.Enum team)
        {
            if (transparencyIndex == 6)
            {
                return "BlankTexture";
            }
            
            string textureName = "ViewerBG";
            textureName += transparencyNames[transparencyIndex];

            if (!InputViewer.Instance.useTeamColors.Value)
            {
                return textureName;
            }
            
            switch (team)
            {
                case Team.Enum.RED:
                    textureName += "-Team0"; break;
                case Team.Enum.BLUE:
                    textureName += "-Team1"; break;
                case Team.Enum.YELLOW:
                    textureName += "-Team2"; break;
                case Team.Enum.GREEN:
                    textureName += "-Team3"; break;
                case Team.Enum.NONE:
                    break;
            }
            
            return textureName;
        }
        
        /*
         * RENDER TEXTURE COPY METHOD FROM COLORSWAP MOD
         * https://gitlab.com/axolotlll/colorswap/-/blob/main/ColorSwap/Sprites.cs
         */
        private static void SetCopy(ref Texture2D destination, Texture2D source)
        {
            RenderTexture temp = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);
            Graphics.Blit(source, temp);
            
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = temp;
            
            destination = new Texture2D(source.width, source.height);
            destination.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);
            destination.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(temp);
        }
        
        private static void SetColor(ref Texture2D texture, Color color)
        {
            Color[] pixels = texture.GetPixels();
            for (int pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
            {
                Color currentPixel = pixels[pixelIndex];
                
                pixels[pixelIndex] = new Color(
                    color.r*currentPixel.r,
                    color.g*currentPixel.g,
                    color.b*currentPixel.b,
                    currentPixel.a);
            }

            texture.SetPixels(pixels);
            texture.Apply();
        }

        private static bool CompareToWhite(Color color)
        {
            return color is { r: >= 220, g: >= 220, b: >= 220 };
        }
        
        public static GUIStyle GetBGStyle(Team.Enum team)
        {
            GUIStyleState bg = new GUIStyleState();

            string texName = GetTextureName(InputViewer.Instance.backgroundTransparency.Value, team);
            Debug.Log(texName);
            bg.background = uiTexture2DAssets[texName];
            bg.textColor = Color.white;

            GUIStyle guiStyle = new GUIStyle()
            {
                font = inputViewerFont,
                normal = bg,
                onNormal = bg,
                hover = bg,
                onHover = bg,
                active = bg,
                onActive = bg,
            };

            return guiStyle;
        }

        static readonly int combatKeySize = 32;
        static readonly RectOffset btnMargin = new RectOffset(-3, -3, -3, -3);

        public static GUIStyle CombatBtnStyle
        {
            get
            {
                GUIStyle gUIStyle = new GUIStyle()
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(4, 4, 4, 4),
                    margin = btnMargin,
                    fixedHeight = combatKeySize,
                    fixedWidth = combatKeySize,
                };
                return gUIStyle;
            }
        }

        public static GUIStyle SwingStyle
        {
            get
            {
                GUIStyleState off = new GUIStyleState
                {
                    background = uiTexture2DAssets["SwingOff"],
                    textColor = Color.black
                };

                GUIStyleState on = new GUIStyleState
                {
                    background = uiTexture2DAssets["SwingOn"],
                    textColor = Color.black
                };

                GUIStyle gUIStyle = new GUIStyle(CombatBtnStyle)
                {
                    normal = off,
                    hover = off,
                    active = off,
                    onNormal = on,
                    onHover = on,
                    onActive = on,
                };
                return gUIStyle;
            }
        }

        public static GUIStyle BuntStyle
        {
            get
            {
                GUIStyleState off = new GUIStyleState
                {
                    background = uiTexture2DAssets["BuntOff"],
                    textColor = Color.black
                };

                GUIStyleState on = new GUIStyleState
                {
                    background = uiTexture2DAssets["BuntOn"],
                    textColor = Color.black
                };

                GUIStyle gUIStyle = new GUIStyle(CombatBtnStyle)
                {
                    normal = off,
                    hover = off,
                    active = off,
                    onNormal = on,
                    onHover = on,
                    onActive = on,
                };
                return gUIStyle;
            }
        }
        public static GUIStyle GrabStyle
        {
            get
            {
                GUIStyleState off = new GUIStyleState
                {
                    background = uiTexture2DAssets["GrabOff"],
                    textColor = Color.black
                };

                GUIStyleState on = new GUIStyleState
                {
                    background = uiTexture2DAssets["GrabOn"],
                    textColor = Color.black
                };

                GUIStyle gUIStyle = new GUIStyle(CombatBtnStyle)
                {
                    normal = off,
                    hover = off,
                    active = off,
                    onNormal = on,
                    onHover = on,
                    onActive = on,
                };
                return gUIStyle;
            }
        }
        public static GUIStyle TauntStyle
        {
            get
            {
                GUIStyleState off = new GUIStyleState
                {
                    background = uiTexture2DAssets["TauntOff"],
                    textColor = Color.black
                };

                GUIStyleState on = new GUIStyleState
                {
                    background = uiTexture2DAssets["TauntOn"],
                    textColor = Color.black
                };

                GUIStyle gUIStyle = new GUIStyle(CombatBtnStyle)
                {
                    normal = off,
                    hover = off,
                    active = off,
                    onNormal = on,
                    onHover = on,
                    onActive = on,
                };
                return gUIStyle;
            }
        }
        public static GUIStyle JumpStyle
        {
            get
            {
                GUIStyleState off = new GUIStyleState
                {
                    background = uiTexture2DAssets["JumpOff"],
                    textColor = Color.black
                };

                GUIStyleState on = new GUIStyleState
                {
                    background = uiTexture2DAssets["JumpOn"],
                    textColor = Color.black
                };

                GUIStyle gUIStyle = new GUIStyle(CombatBtnStyle)
                {
                    normal = off,
                    hover = off,
                    active = off,
                    onNormal = on,
                    onHover = on,
                    onActive = on,
                };
                return gUIStyle;
            }
        }
        public static GUIStyle DirUpStyle
        {
            get
            {
                GUIStyleState off = new GUIStyleState
                {
                    background = uiTexture2DAssets["ArrowUOff"],
                    textColor = Color.black
                };

                GUIStyleState on = new GUIStyleState
                {
                    background = uiTexture2DAssets["ArrowUOn"],
                    textColor = Color.white
                };

                GUIStyle gUIStyle = new GUIStyle(CombatBtnStyle)
                {
                    normal = off,
                    hover = off,
                    active = off,
                    onNormal = on,
                    onHover = on,
                    onActive = on,
                };
                return gUIStyle;
            }
        }

        public static GUIStyle DirDwnStyle
        {
            get
            {
                GUIStyleState off = new GUIStyleState
                {
                    background = uiTexture2DAssets["ArrowDOff"],
                    textColor = Color.black
                };

                GUIStyleState on = new GUIStyleState
                {
                    background = uiTexture2DAssets["ArrowDOn"],
                    textColor = Color.white
                };

                GUIStyle gUIStyle = new GUIStyle(CombatBtnStyle)
                {
                    normal = off,
                    hover = off,
                    active = off,
                    onNormal = on,
                    onHover = on,
                    onActive = on,
                };
                return gUIStyle;
            }
        }
        public static GUIStyle DirLefStyle
        {
            get
            {
                GUIStyleState off = new GUIStyleState
                {
                    background = uiTexture2DAssets["ArrowLOff"],
                    textColor = Color.black
                };

                GUIStyleState on = new GUIStyleState
                {
                    background = uiTexture2DAssets["ArrowLOn"],
                    textColor = Color.white
                };

                GUIStyle gUIStyle = new GUIStyle(CombatBtnStyle)
                {
                    normal = off,
                    hover = off,
                    active = off,
                    onNormal = on,
                    onHover = on,
                    onActive = on,
                };
                return gUIStyle;
            }
        }
        public static GUIStyle DirRigStyle
        {
            get
            {
                GUIStyleState off = new GUIStyleState
                {
                    background = uiTexture2DAssets["ArrowROff"],
                    textColor = Color.black
                };

                GUIStyleState on = new GUIStyleState
                {
                    background = uiTexture2DAssets["ArrowROn"],
                    textColor = Color.white
                };

                GUIStyle gUIStyle = new GUIStyle(CombatBtnStyle)
                {
                    normal = off,
                    hover = off,
                    active = off,
                    onNormal = on,
                    onHover = on,
                    onActive = on,
                };
                return gUIStyle;
            }
        }
        public static GUIStyle ExpressStyle
        {
            get
            {
                GUIStyle gUIStyle = new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    fixedWidth = 64,
                    fixedHeight = 32,
                };
                return gUIStyle;
            }
        }

        public static GUIStyle ExpNiceStyle
        {
            get
            {
                GUIStyleState off = new GUIStyleState
                {
                    background = uiTexture2DAssets["NiceOff"],
                    textColor = Color.white
                };

                GUIStyleState on = new GUIStyleState
                {
                    background = uiTexture2DAssets["NiceOn"],
                    textColor = Color.white
                };

                GUIStyle gUIStyle = new GUIStyle(ExpressStyle)
                {
                    normal = off,
                    hover = off,
                    active = off,
                    onNormal = on,
                    onHover = on,
                    onActive = on,
                };
                return gUIStyle;
            }
        }

        public static GUIStyle ExpOopsStyle
        {
            get
            {
                GUIStyleState off = new GUIStyleState
                {
                    background = uiTexture2DAssets["OopsOff"],
                    textColor = Color.white
                };

                GUIStyleState on = new GUIStyleState
                {
                    background = uiTexture2DAssets["OopsOn"],
                    textColor = Color.white
                };

                GUIStyle gUIStyle = new GUIStyle(ExpressStyle)
                {
                    normal = off,
                    hover = off,
                    active = off,
                    onNormal = on,
                    onHover = on,
                    onActive = on,
                };
                return gUIStyle;
            }
        }
        public static GUIStyle ExpWowStyle
        {
            get
            {
                GUIStyleState off = new GUIStyleState
                {
                    background = uiTexture2DAssets["WowOff"],
                    textColor = Color.white
                };

                GUIStyleState on = new GUIStyleState
                {
                    background = uiTexture2DAssets["WowOn"],
                    textColor = Color.white
                };

                GUIStyle gUIStyle = new GUIStyle(ExpressStyle)
                {
                    normal = off,
                    hover = off,
                    active = off,
                    onNormal = on,
                    onHover = on,
                    onActive = on,
                };
                return gUIStyle;
            }
        }

        public static GUIStyle ExpBringItStyle
        {
            get
            {
                GUIStyleState off = new GUIStyleState
                {
                    background = uiTexture2DAssets["BringItOff"],
                    textColor = Color.white
                };

                GUIStyleState on = new GUIStyleState
                {
                    background = uiTexture2DAssets["BringItOn"],
                    textColor = Color.white
                };

                GUIStyle gUIStyle = new GUIStyle(ExpressStyle)
                {
                    normal = off,
                    hover = off,
                    active = off,
                    onNormal = on,
                    onHover = on,
                    onActive = on,
                };
                return gUIStyle;
            }
        }

    }
}
