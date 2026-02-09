using UnityEngine;
using UnityEngine.UI;

namespace InputViewer
{
    public class InputDisplay : MonoBehaviour
    {
        private Image image;
        public Sprite spriteOn;
        public Sprite spriteOff;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        public void Init(string on, string off)
        {
            spriteOn = IVStyle.uiSprites[on];
            spriteOff = IVStyle.uiSprites[off];
        }

        public void SetState(bool state)
        {
            image.sprite = state ? spriteOn : spriteOff;
        }
    }
}
