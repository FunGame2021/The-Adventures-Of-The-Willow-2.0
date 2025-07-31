using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeEditorInfoText : MonoBehaviour
{
    [Header("UI")]

    private Button[] mpButtons;
    private GameObject mpPanelInstance;

    private TMP_InputField[] mpInputField;

    private string speedtimer;
    private string stoptimer;

    //[SerializeField] private Toggle rightStart;

    private bool mpIsOpened = false;
    private Button mpBackButton;
    private Button mpOkButton;


    [Header("References")]
    [SerializeField] private GameObject nodePanelPrefab;

    private GameObject nodePanelInstance;
    private Transform panelLocalization;

    private TMP_InputField timeNodeInput;
    private TMP_InputField stopTimeInput;
    public TextMeshProUGUI nodeNumberText;

    private Button backButton;
    private Button okButton;

    private PlatformNodeEditor.EditorWaypointData currentWaypointData;
    private bool isOpened = false;

    private void Start()
    {
        panelLocalization = FindFirstObjectByType<MultipleObjectSelector>().panelLocalization;
    }

    public void OpenNodePanel(PlatformNodeEditor.EditorWaypointData waypointData)
    {
        if (isOpened || waypointData == null) return;

        isOpened = true;
        currentWaypointData = waypointData;

        nodePanelInstance = Instantiate(nodePanelPrefab, panelLocalization);

        if (nodePanelInstance != null)
        {
            // Corrigido: usar nodePanelInstance
            var inputs = nodePanelInstance.GetComponentsInChildren<TMP_InputField>();
            foreach (TMP_InputField input in inputs)
            {
                if (input.name == "TimeNodeInput") timeNodeInput = input;
                else if (input.name == "StopTimeInput") stopTimeInput = input;
            }

            var buttons = nodePanelInstance.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                if (button.name == "BackButton") backButton = button;
                else if (button.name == "OKButton") okButton = button;
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(ClosePanel);
            }

            if (okButton != null)
            {
                okButton.onClick.RemoveAllListeners();
                okButton.onClick.AddListener(SaveAndClose);
            }

            UpdateUIValues();
        }
        else
        {
            Debug.LogError("Falha ao instanciar painel do Node.");
            isOpened = false;
        }
    }

    private void UpdateUIValues()
    {
        if (currentWaypointData == null) return;

        timeNodeInput.text = currentWaypointData.TimeNode.ToString("F2");
        stopTimeInput.text = currentWaypointData.StopTime.ToString("F2");
        nodeNumberText.text = currentWaypointData.NumberText != null ? currentWaypointData.NumberText.text : "-";
    }

    private void SaveAndClose()
    {
        if (float.TryParse(timeNodeInput.text, out float time))
            currentWaypointData.TimeNode = time;

        if (float.TryParse(stopTimeInput.text, out float stop))
            currentWaypointData.StopTime = stop;


        // Atualiza o PlatformNodeEditor para repassar valores ao PlatformController
        if (PlatformNodeEditor.instance != null)
        {
            PlatformNodeEditor.instance.UpdateWaypointsEditor();
        }

        ClosePanel();
    }

    private void ClosePanel()
    {
        isOpened = false;
        if (nodePanelInstance != null)
            Destroy(nodePanelInstance);
    }
}
