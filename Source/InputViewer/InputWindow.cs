using BepInEx.Configuration;
using LLBML.Players;
using LLBML.Utils;
using LLGUI;
using LLHandlers;
using LLScreen;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InputViewer
{

public class InputWindow : LLClickable
{
    private RectTransform rectTransform;
    public bool isMiniSize;
    public Player boundPlayer;
    private ConfigEntry<Vector2> savedPosition;
    private bool isDraggable;

    private TextMeshProUGUI lbHeader;
    private Image imgBG;
    private Image imgStripe;

    private InputDisplay inputJump;
    private InputDisplay inputUp;
    private InputDisplay inputDown;
    private InputDisplay inputLeft;
    private InputDisplay inputRight;

    private InputDisplay inputSwing;
    private InputDisplay inputBunt;
    private InputDisplay inputGrab;
    private InputDisplay inputTaunt;

    private InputDisplay inputNice;
    private InputDisplay inputWow;
    private InputDisplay inputBringIt;
    private InputDisplay inputOops;

    public static InputWindow Create(Transform parent, string name, ConfigEntry<Vector2> savedPosition, bool isMiniSize, bool isDraggable)
    {
        GameObject viewerPrefab = IVStyle.uiPrefabAssets[isMiniSize ? "ViewerMini" : "ViewerRegular"];
        GameObject viewer = Instantiate(viewerPrefab, parent);
        viewer.name = name;

        InputWindow window = viewer.AddComponent<InputWindow>();
        window.imgBG = window.GetComponent<Image>();
        window.savedPosition = savedPosition;
        window.isMiniSize = isMiniSize;
        window.isDraggable = isDraggable;
        window.InitUI();
        return window;
    }

    private void InitUI()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(savedPosition.Value.x, savedPosition.Value.y);

        lbHeader = rectTransform.Find("Header").gameObject.AddComponent<TextMeshProUGUI>();
        lbHeader.alignment = TextAlignmentOptions.Center;
        lbHeader.fontSize = 20;
        TextHandler.SetText(lbHeader, isMiniSize ? "Inputs" : "Input Viewer");

        imgStripe = rectTransform.Find("Stripe").GetComponent<Image>();
        
        Transform tfMovement = rectTransform.Find("Movement");
        inputJump = tfMovement.Find("Jump").gameObject.AddComponent<InputDisplay>();
        inputJump.Init("JumpOn", "JumpOff");
        inputUp = tfMovement.Find("Up").gameObject.AddComponent<InputDisplay>();
        inputUp.Init("ArrowUOn", "ArrowUOff");
        inputDown = tfMovement.Find("Down").gameObject.AddComponent<InputDisplay>();
        inputDown.Init("ArrowDOn", "ArrowDOff");
        inputLeft = tfMovement.Find("Left").gameObject.AddComponent<InputDisplay>();
        inputLeft.Init("ArrowLOn", "ArrowLOff");
        inputRight = tfMovement.Find("Right").gameObject.AddComponent<InputDisplay>();
        inputRight.Init("ArrowROn", "ArrowROff");

        Transform tfActions = rectTransform.Find("Actions");
        inputSwing = tfActions.Find("Swing").gameObject.AddComponent<InputDisplay>();
        inputSwing.Init("SwingOn", "SwingOff");
        inputBunt = tfActions.Find("Bunt").gameObject.AddComponent<InputDisplay>();
        inputBunt.Init("BuntOn", "BuntOff");
        inputGrab = tfActions.Find("Grab").gameObject.AddComponent<InputDisplay>();
        inputGrab.Init("GrabOn", "GrabOff");
        inputTaunt = tfActions.Find("Taunt").gameObject.AddComponent<InputDisplay>();
        inputTaunt.Init("TauntOn", "TauntOff");
        
        Transform tfExpressions = rectTransform.Find("Expressions");
        inputNice = tfExpressions.Find("Nice").gameObject.AddComponent<InputDisplay>();
        inputNice.Init("NiceOn", "NiceOff");
        inputWow = tfExpressions.Find("Wow").gameObject.AddComponent<InputDisplay>();
        inputWow.Init("WowOn", "WowOff");
        inputBringIt = tfExpressions.Find("Bring It").gameObject.AddComponent<InputDisplay>();
        inputBringIt.Init("BringItOn", "BringItOff");
        inputOops = tfExpressions.Find("Oops").gameObject.AddComponent<InputDisplay>();
        inputOops.Init("OopsOn", "OopsOff");
        
        tfExpressions.gameObject.SetActive(!isMiniSize);
        UpdateColor();

        onClick = StartStopDrag;
    }

    public void UpdateColor()
    {
        Team team = boundPlayer == null ? Team.NONE : boundPlayer.Team;
        float transparency = InputViewer.Instance.backgroundTransparency.Value switch
        {
            5 => 0.5f,
            4 => 0.6f,
            3 => 0.7f,
            2 => 0.8f,
            1 => 0.9f,
            0 => 1f,
            _ => 0f
        };
        
        Color colorStripe = team == Team.NONE || !InputViewer.Instance.useTeamColors.Value ? Color.white : IVStyle.TeamColors[(int)team];
        colorStripe.a = transparency;
        Color colorBg = Color.white;
        colorBg.a = transparency;
        
        imgStripe.color = colorStripe;
        imgBG.color = colorBg;
    }

    public void BindPlayer(Player player)
    {
        boundPlayer = player;
    }

    private void LateUpdate()
    {
        if (isDragging)
        {
            Vector2 vector = UIScreen.activeCamera.ScreenToViewportPoint(UIInput.mainCursor.GetPosition());
            Vector2 cursorPos = new Vector2(1280f * (vector.x - 0.5f) + 32f, 720f * (vector.y - 0.5f) - 32f);
            Vector2 delta = cursorPos - cursorStartPos;
            rectTransform.localPosition = windowStartPos + delta;
        }

        if (boundPlayer == null) return;
        
        inputJump.SetState(InputHandler.GetInput(boundPlayer, InputAction.JUMP));
        inputUp.SetState(InputHandler.GetInput(boundPlayer, InputAction.UP));
        inputDown.SetState(InputHandler.GetInput(boundPlayer, InputAction.DOWN));
        inputLeft.SetState(InputHandler.GetInput(boundPlayer, InputAction.LEFT));
        inputRight.SetState(InputHandler.GetInput(boundPlayer, InputAction.RIGHT));
        
        inputSwing.SetState(InputHandler.GetInput(boundPlayer, InputAction.SWING));
        inputBunt.SetState(InputHandler.GetInput(boundPlayer, InputAction.BUNT));
        inputGrab.SetState(InputHandler.GetInput(boundPlayer, InputAction.GRAB));
        inputTaunt.SetState(InputHandler.GetInput(boundPlayer, InputAction.TAUNT));

        if (isMiniSize) return;
        inputNice.SetState(InputHandler.GetInput(boundPlayer, InputAction.EXPRESS_UP));
        inputWow.SetState(InputHandler.GetInput(boundPlayer, InputAction.EXPRESS_RIGHT));
        inputBringIt.SetState(InputHandler.GetInput(boundPlayer, InputAction.EXPRESS_DOWN));
        inputOops.SetState(InputHandler.GetInput(boundPlayer, InputAction.EXPRESS_LEFT));
    }

    public bool IsPositionUnsaved()
    {
        return rectTransform.anchoredPosition != savedPosition.Value;
    }

    public void SavePosition()
    {
        savedPosition.Value = rectTransform.anchoredPosition;
    }

    private bool isDragging;
    private Vector2 cursorStartPos;
    private Vector2 windowStartPos;
    private void StartStopDrag(int playerNr)
    {
        if (!isDraggable) return;
        
        if (!isDragging)
        {
            if (!ModDependenciesUtils.InModOptions()) return;
            Vector2 vector = UIScreen.activeCamera.ScreenToViewportPoint(UIInput.mainCursor.GetPosition());
            cursorStartPos = new Vector2(1280f * (vector.x - 0.5f) + 32f, 720f * (vector.y - 0.5f) - 32f);
            windowStartPos = rectTransform.localPosition;
            isDragging = true;
        }
        else
        {
            isDragging = false;
        }
    }
}
}
