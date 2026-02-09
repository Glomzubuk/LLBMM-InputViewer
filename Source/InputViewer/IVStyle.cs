using System.Collections.Generic;
using System.IO;
using LLBML.Players;
using LLHandlers;
using UnityEngine;

namespace InputViewer
{
    public static class IVStyle
    {

        readonly static string bundlesFolder = Path.Combine(Path.GetDirectoryName(InputViewer.Instance.Info.Location), "Bundles");

        public static AssetBundle uiBundle;
        public static Font inputViewerFont;
        public static Dictionary<string, Texture2D> uiTexture2DAssets = new Dictionary<string, Texture2D>();
        public static Dictionary<string, GameObject> uiPrefabAssets = new Dictionary<string, GameObject>();
        public static Dictionary<string, Sprite> uiSprites = new Dictionary<string, Sprite>();
        public static Texture2D viewerBG;

        public static readonly Color[] DefaultTeamColors = {
            new Color(255/255f, 64/255f, 22/255f),
            new Color(13/255f, 136/255f, 255/255f),
            new Color(255/255f, 255/255f, 61/255f),
            new Color(90/255f, 244/255f, 90/255f)
        };
        public static Color[] TeamColors = new Color[4];
        private static string[] transparencyNames = {"", "_90", "_80", "_70", "_60", "_50"};

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
            //foreach (var s in IVStyle.uiTexture2DAssets) Debug.Log(s.Key);

            for (int teamColorIndex = 0; teamColorIndex < DefaultTeamColors.Length; teamColorIndex++)
            {
                TeamColors[teamColorIndex] = DefaultTeamColors[teamColorIndex];
            }

            GameObject[] gameObjects = uiBundle.LoadAllAssets<GameObject>();
            for (int i = 0; i < gameObjects.Length; i++)
            {
                uiPrefabAssets.Add(gameObjects[i].name, gameObjects[i]);
            }

            foreach (KeyValuePair<string, Texture2D> entry in uiTexture2DAssets)
            {
                string name = entry.Key;
                Texture2D tex = entry.Value;
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                uiSprites.Add(name, sprite);
            }
        }

        public static void ATInit()
        {
            LoadAssets();
        }

        public static void UpdateTeamColors(Color[] newTeamColors)
        {
            for (int teamColorIndex = 0; teamColorIndex < TeamColors.Length; teamColorIndex++)
            {
                TeamColors[teamColorIndex] = newTeamColors[teamColorIndex] != Color.clear
                    ? newTeamColors[teamColorIndex]
                    : DefaultTeamColors[teamColorIndex];
            }
        }

    }
}
