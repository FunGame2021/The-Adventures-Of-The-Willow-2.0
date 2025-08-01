using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class TriggerObjectSelector : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown typeDropdown;
    private Button[] buttons;
    private GameObject selectedObject;
    private TriggerObject triggerObjectScript;
    [SerializeField] private GameObject panelPrefab;
    private GameObject panelInstance;
    private TMP_InputField[] inputFields;
    [SerializeField] private string scriptWritted;
    [SerializeField] private Transform panelLocalization;
    private Toggle wasWaitTimeToggle;
    private TMP_InputField scriptInputField;
    private TMP_InputField timeToPlayInputField;
    [SerializeField] private float timeToPlay;

    private bool isOpened = false;
    private Button backButton;
    private Button okButton;
    private List<string> typeOptions;


    [SerializeField] private float doubleTapTimeThreshold = 0.3f; // Tempo máximo entre toques
    [SerializeField] private float maxTapDistance = 50f;          // Distância máxima entre toques

    private float lastTapTime = 0f;
    private Vector2 lastTapPosition = Vector2.zero;
    private int tapCount = 0;

    void Awake()
    {
        typeOptions = new List<string>
        {
            "Play Particles",
            "Stop Particles",
            "Ladder",
            "Play sfx"
        };
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // Clique direito do mouse
        if (Keyboard.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            TryOpenPanelFromPosition(Mouse.current.position.ReadValue());
        }
#else
        // Toque duplo no mobile
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var touch = Touchscreen.current.touches[0];

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                Vector2 currentTapPosition = touch.position.ReadValue();
                float currentTime = Time.time;

                if (currentTime - lastTapTime < doubleTapTimeThreshold &&
                    Vector2.Distance(currentTapPosition, lastTapPosition) < maxTapDistance)
                {
                    tapCount++;

                    if (tapCount >= 2)
                    {
                        tapCount = 0;

                        if (LevelEditorManager.instance.isActiveSelectPoint)
                        {
                            TryOpenPanelFromPosition(currentTapPosition);
                        }
                    }
                }
                else
                {
                    tapCount = 1;
                }

                lastTapTime = currentTime;
                lastTapPosition = currentTapPosition;
            }
        }
#endif
        // Atualiza script durante o painel aberto
        if (isOpened && panelInstance != null && scriptInputField != null)
        {
            scriptWritted = scriptInputField.text;
        }
        else if (!isOpened)
        {
            selectedObject = null;
            panelInstance = null;
            triggerObjectScript = null;
        }

        try
        {
            if (Camera.main == null)
                Debug.LogWarning("Camera.main is null");
            if (LevelEditorManager.instance == null)
                Debug.LogWarning("LevelEditorManager.instance is null");
            if (Mouse.current == null)
                Debug.LogWarning("Mouse.current is null");
            if (Keyboard.current == null)
                Debug.LogWarning("Keyboard.current is null");
            if (Touchscreen.current == null)
                Debug.LogWarning("Touchscreen.current is null");

            // código existente aqui

        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro no Update TriggerObjectSelector: " + e.Message + "\n" + e.StackTrace);
        }
    }

    private void TryOpenPanelFromPosition(Vector2 screenPosition)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null && !isOpened && LevelEditorManager.instance.isActiveSelectPoint)
        {
            if (hit.collider.CompareTag("GameObject") && hit.collider.gameObject.name.StartsWith("Trigger"))
            {
                OpenPanel(hit.collider.gameObject);
            }
        }
    }

    private void OpenPanel(GameObject obj)
    {
        isOpened = true;
        selectedObject = obj;
        triggerObjectScript = selectedObject.GetComponentInChildren<TriggerObject>();
        panelInstance = Instantiate(panelPrefab, panelLocalization);

        if (panelInstance != null)
        {
            typeDropdown = panelInstance.GetComponentInChildren<TMP_Dropdown>();
            if (typeDropdown != null)
            {
                typeDropdown.ClearOptions();
                typeDropdown.AddOptions(typeOptions);
            }

            inputFields = panelInstance.GetComponentsInChildren<TMP_InputField>();
            foreach (TMP_InputField inputField in inputFields)
            {
                if (inputField.name == "timeToPlayInputField")
                    timeToPlayInputField = inputField;
                else if (inputField.name == "scriptInputField")
                    scriptInputField = inputField;
            }

            wasWaitTimeToggle = panelInstance.GetComponentInChildren<Toggle>();

            buttons = panelInstance.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                if (button.name == "BackButton")
                    backButton = button;
                else if (button.name == "OKButton")
                    okButton = button;
            }

            backButton.onClick.AddListener(() =>
            {
                isOpened = false;
                Destroy(panelInstance);
            });

            okButton.onClick.AddListener(() =>
            {
                triggerObjectScript.thisTriggerType = typeDropdown.options[typeDropdown.value].text;
                triggerObjectScript.customScript = scriptWritted;
                triggerObjectScript.wasTriggerWaitTime = wasWaitTimeToggle.isOn;
                if (float.TryParse(timeToPlayInputField.text, out float parsedTime))
                {
                    timeToPlay = parsedTime;
                }
                else
                {
                    Debug.LogError("Failed to parse timeToPlay.");
                }
                triggerObjectScript.timeToPlayTrigger = timeToPlay;
                isOpened = false;
                Destroy(panelInstance);
            });

            UpdateUIValues();
        }
    }

    void UpdateUIValues()
    {
        if (triggerObjectScript != null)
        {
            int index = typeOptions.IndexOf(triggerObjectScript.thisTriggerType);
            if (index >= 0)
            {
                typeDropdown.value = index;
            }
            scriptWritted = triggerObjectScript.customScript;
            if (scriptInputField != null)
                scriptInputField.text = scriptWritted;

            if (wasWaitTimeToggle != null)
                wasWaitTimeToggle.isOn = triggerObjectScript.wasTriggerWaitTime;

            timeToPlay = triggerObjectScript.timeToPlayTrigger;
            if (timeToPlayInputField != null)
                timeToPlayInputField.text = timeToPlay.ToString();
        }
    }
}
