using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

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
    // Declare a list for typeOptions
    private List<string> typeOptions;

    void Awake()
    {
        // Initialize typeOptions in Awake or Start
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
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Vector2 clickPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(clickPosition, Vector2.zero);

            if (hit.collider != null && !isOpened && LevelEditorManager.instance.isActiveSelectPoint)
            {
                if (hit.collider != null && hit.collider.CompareTag("GameObject") && hit.collider.gameObject.name.StartsWith("Trigger"))
                {
                    isOpened = true;
                    selectedObject = hit.collider.gameObject;
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
                            if(inputField.name == "timeToPlayInputField")
                            {
                                timeToPlayInputField = inputField;
                            }
                            else if(inputField.name == "scriptInputField")
                            {
                                scriptInputField = inputField;
                            }
                        }
                        wasWaitTimeToggle = panelInstance.GetComponentInChildren<Toggle>();
                        buttons = panelInstance.GetComponentsInChildren<Button>();
                        foreach (Button button in buttons)
                        {
                            if (button.name == "BackButton")
                            {
                                backButton = button;
                            }
                            else if (button.name == "OKButton")
                            {
                                okButton = button;
                            }
                        }
                        // Configurar eventos para botões
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
                            // Converte a string para float e atribui a timeToPlay
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
            }
        }
        if (isOpened)
        {
            if (panelInstance != null)
            {
                if (scriptInputField != null)
                {
                    scriptWritted = scriptInputField.text;
                }
            }
        }
        else
        {
            selectedObject = null;
            panelInstance = null;
            triggerObjectScript = null;
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
            scriptInputField.text = scriptWritted;
            wasWaitTimeToggle.isOn = triggerObjectScript.wasTriggerWaitTime;
            timeToPlayInputField.text = timeToPlay.ToString();
            timeToPlay = triggerObjectScript.timeToPlayTrigger;
            timeToPlayInputField.text = timeToPlay.ToString();
        }
    }
}