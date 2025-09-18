using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Holds references to UI objects.</summary>
public class HUD : MonoBehaviour
{
    public GameObject ConnectHud;
    public GameObject GameHud;

    public GameObject Connecting;
    public TMP_Text ErrorMessage;

    public RectTransform Stats;
    public TMP_Text Objects;
    public TMP_Text Players;
    public TMP_Text Goodput;
    public TMP_Text Bandwidth;
    public TMP_Text Syncs;

    public RectTransform Room;
    public TMP_Text Server;
    public TMP_Text Host;
    public TMP_Text Port;
    public TMP_Text Protocol;

    public GameObject Overlay;
    public Button OverlayCloseButton;
    public Button OverlayOpenButton;

    public GameObject LocalServerWarning;

    public GameObject ViewingObjects;
    public GameObject EditingObjects;
    public Toggle MovementToggle;
    public Button EditButton;
    public Button ConfirmEditButton;
    public Button CancelEditButton;
    public TMP_InputField ObjectsInput;

    public static HUD Get()
    {
        return m_instance;
    }

    private static HUD m_instance;

    private void Awake()
    {
        m_instance = this;

        Overlay.SetActive(false);
        OverlayCloseButton.onClick.AddListener(CloseOverlay);
        OverlayOpenButton.onClick.AddListener(ToggleOverlay);
    }

    private void Update()
    {
        // Toggle the overlay when tab is pressed.
        if (GameHud.activeSelf && Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleOverlay();
        }
    }

    private void CloseOverlay()
    {
        Overlay.SetActive(false);
    }

    private void ToggleOverlay()
    {
        Overlay.SetActive(!Overlay.activeSelf);
    }

    /// <summary>
    /// Resizes the width of a <see cref="RectTransform"/> to fit all <see cref="TMP_Text"/> descendants.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="minWidth"></param>
    /// <param name="pad"></param>
    public void ResizeWidth(RectTransform rect, float minWidth = 0f, float pad = 20f)
    {
        float width = minWidth;
        foreach (TMP_Text text in rect.GetComponentsInChildren<TMP_Text>())
        {
            width = Mathf.Max(width, text.rectTransform.anchoredPosition.x + text.preferredWidth);
        }
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width + pad);
    }
}
