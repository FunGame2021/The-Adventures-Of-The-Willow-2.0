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
    [SerializeField] private TMP_InputField scriptInputField;
    [SerializeField] private string scriptWritted;
    [SerializeField] private Transform panelLocalization;
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

            if (hit.collider != null && !isOpened)
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
                        scriptInputField = panelInstance.GetComponentInChildren<TMP_InputField>();
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
        }
    }
}