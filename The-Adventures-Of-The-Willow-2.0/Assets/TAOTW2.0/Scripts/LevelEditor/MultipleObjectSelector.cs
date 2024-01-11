using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MultipleObjectSelector : MonoBehaviour
{
    [Header("General")]
    #region general
    [SerializeField] private Transform panelLocalization;
    #endregion

    [Header("Particles")]
    #region particles
    private Button[] buttons;
    private GameObject selectedObject;
    private ParticlesObject particleObjectScript;
    [SerializeField] private GameObject panelPrefab;
    private GameObject panelInstance;

    private TMP_InputField particleNameInputField;
    private string particleType;
    private string particleName;
    private Toggle[] toggles;
    private Toggle isLoop;
    private Toggle isInitialStarted;

    private bool isOpened = false;
    private Button backButton;
    private Button okButton;
    private Button particleTypeButton;
    [SerializeField] private ParticleTypes particleTypesData;
    //UI
    [SerializeField] private GameObject ParticleTypePanel;
    [SerializeField] private TextMeshProUGUI particleTypeText;
    [SerializeField] private GameObject particleButtonPrefab;
    [SerializeField] private Transform buttonParticleContainer;
    private List<Button> particleButtons = new List<Button>(); // Corrigido para usar List<Button> ao invés de Button[]

    #endregion


    [Header("MovingPlatforms")]
    #region movingPlatforms
    private Button[] mpButtons;
    private GameObject mpSelectedObject;
    private PlatformController platformControllerScript;
    [SerializeField] private GameObject mpPanelPrefab;
    private GameObject mpPanelInstance;

    private TMP_InputField[] mpInputField;
    private TMP_InputField platformIDInput;
    private TMP_InputField speedInput;
    private TMP_InputField stopDistanceInput;

    private string platformIDString;
    private string speedString;
    private string stopDistanceString;

    private Toggle[] mpToggles;
    private Toggle mpIsPingPong;
    private Toggle mpIsInitialStarted;
    private Toggle mpIsClosed;
    //[SerializeField] private Toggle rightStart;

    private bool mpIsOpened = false;
    private Button mpBackButton;
    private Button mpOkButton;
    #endregion
    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            #region particles
            Vector2 clickPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(clickPosition, Vector2.zero);

            if (hit.collider != null && !isOpened)
            {
                if (hit.collider != null && hit.collider.CompareTag("GameObject") && hit.collider.gameObject.name.StartsWith("Particle"))
                {
                    isOpened = true;
                    selectedObject = hit.collider.gameObject;
                    particleObjectScript = selectedObject.GetComponent<ParticlesObject>();
                    panelInstance = Instantiate(panelPrefab, panelLocalization);

                    if (panelInstance != null)
                    {
                        toggles = panelInstance.GetComponentsInChildren<Toggle>();

                        foreach(Toggle toggle in toggles)
                        {
                            if(toggle.name == "initial")
                            {
                                isInitialStarted = toggle;
                            }
                            else if (toggle.name == "isLoop")
                            {
                                isLoop = toggle;
                            }
                        }

                        particleNameInputField = panelInstance.GetComponentInChildren<TMP_InputField>();

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
                            else if (button.name == "PaticleTypeButton")
                            {
                                particleTypeButton = button;
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
                            particleObjectScript.particleType = particleType;
                            particleObjectScript.particleName = particleName;
                            particleObjectScript.isLoop = isLoop.isOn;
                            particleObjectScript.initialStarted = isInitialStarted.isOn;

                            isOpened = false;
                            Destroy(panelInstance);
                        });


                        particleTypeButton.onClick.AddListener(() =>
                        {
                            ParticleTypePanel.SetActive(true);
                            InitializeParticleTypeButtons();
                        });

                        UpdateUIValues();
                    }
                }
            }
            #endregion
            #region Moving Platform

            if (hit.collider != null && !mpIsOpened)
            {
                if (hit.collider != null && hit.collider.CompareTag("MovingPlatform"))
                {
                    mpIsOpened = true;
                    mpSelectedObject = hit.collider.gameObject;
                    platformControllerScript = mpSelectedObject.GetComponent<PlatformController>();
                    mpPanelInstance = Instantiate(mpPanelPrefab, panelLocalization);

                    if (mpPanelInstance != null)
                    {
                        mpToggles = mpPanelInstance.GetComponentsInChildren<Toggle>();

                        foreach (Toggle toggle in mpToggles)
                        {
                            if (toggle.name == "initial")
                            {
                                mpIsInitialStarted = toggle;
                            }
                            else if (toggle.name == "isPingPong")
                            {
                                mpIsPingPong = toggle;
                            }
                            else if (toggle.name == "isClosed")
                            {
                                mpIsClosed = toggle;
                            }
                        }

                        mpInputField = mpPanelInstance.GetComponentsInChildren<TMP_InputField>();
                        foreach (TMP_InputField mpInput in mpInputField)
                        {
                            if (mpInput.name == "inputID")
                            {
                                platformIDInput = mpInput;
                            }
                            else if (mpInput.name == "inputSpeed")
                            {
                                speedInput = mpInput;
                            }
                            else if (mpInput.name == "inputStopDistance")
                            {
                                stopDistanceInput = mpInput;
                            }
                        }


                        mpButtons = mpPanelInstance.GetComponentsInChildren<Button>();
                        foreach (Button button in mpButtons)
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
                            mpIsOpened = false;
                            Destroy(mpPanelInstance);
                        });

                        okButton.onClick.AddListener(() =>
                        {
                            if (mpIsClosed.isOn)
                            {
                                platformControllerScript.pathType = WaypointPathType.Closed;
                            }
                            else
                            {
                                platformControllerScript.pathType = WaypointPathType.Open;
                            }
                            if(mpIsPingPong.isOn)
                            {
                                platformControllerScript.behaviorType = WaypointBehaviorType.PingPong;
                            }
                            else
                            {
                                platformControllerScript.behaviorType = WaypointBehaviorType.Loop;
                            }
                            platformControllerScript.initialStart = mpIsInitialStarted.isOn;
                            platformControllerScript.platformMoveid = platformIDString.ToString();
                            float speedValue;
                            float stopDistanceValue;

                            if (float.TryParse(speedString, out speedValue))
                            {
                                platformControllerScript.moveSpeed = speedValue;
                            }

                            if (float.TryParse(stopDistanceString, out stopDistanceValue))
                            {
                                platformControllerScript.stopDistance = stopDistanceValue;
                            }

                            mpIsOpened = false;
                            Destroy(mpPanelInstance);
                        });
                        UpdateUIValuesMP();
                    }
                }
            }
            #endregion
        }
        #region particles
        if (isOpened)
        {
            if (panelInstance != null)
            {
                if (particleNameInputField != null)
                {
                    particleName = particleNameInputField.text;
                }
            }
        }
        else
        {
            selectedObject = null;
            panelInstance = null;
            particleObjectScript = null;
        }
        #endregion

        #region Moving Platform
        if (mpIsOpened)
        {
            if (mpPanelInstance != null)
            {
                if (platformIDInput != null)
                {
                    platformIDString = platformIDInput.text;
                }
                if(speedInput != null)
                {
                    speedString = speedInput.text;
                }
                if(stopDistanceInput != null)
                {
                    stopDistanceString = stopDistanceInput.text;
                }
            }
        }
        else
        {
            mpSelectedObject = null;
            mpPanelInstance = null;
            platformControllerScript = null;
        }
        #endregion
    }

    #region particles
    void UpdateUIValues()
    {
        if (particleObjectScript != null)
        {
            particleType = particleObjectScript.particleType;
            particleName = particleObjectScript.particleName;
            particleNameInputField.text = particleName;
            isLoop.isOn = particleObjectScript.isLoop;
            isInitialStarted.isOn = particleObjectScript.initialStarted;
            
        }
    }

    void InitializeParticleTypeButtons()
    {
        // Limpa a lista de botões antes de criar novos
        ClearParticleTypeButtons();

        // Cria um botão para cada tipo de partícula
        foreach (var category in particleTypesData.categories)
        {
            foreach (var particleType in category.ParticleTypes)
            {
                CreateParticleTypeButton(particleType.ParticleTypesName);
            }
        }
    }

    void ClearParticleTypeButtons()
    {
        // Verifica se a lista de botões está inicializada
        if (particleButtons != null)
        {
            // Destroi cada botão na lista
            foreach (Button button in particleButtons)
            {
                Destroy(button.gameObject);
            }

            // Limpa a lista de botões
            particleButtons.Clear();
        }
    }
    void CreateParticleTypeButton(string typeName)
    {
        // Use o prefab específico para os botões de partículas
        GameObject buttonparticle = Instantiate(particleButtonPrefab);
        Button particleButton = buttonparticle.GetComponent<Button>();
        particleButton.onClick.AddListener(() => OnParticleTypeButtonClicked(typeName));

        TextMeshProUGUI buttonText = buttonparticle.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = typeName;

        buttonparticle.transform.SetParent(buttonParticleContainer);
        particleButtons.Add(particleButton); // Adiciona o botão à lista
    }

    void OnParticleTypeButtonClicked(string typeName)
    {
        particleType = typeName;
        // Atualiza o componente TextMeshPro para mostrar o tipo de partícula selecionado
        particleTypeText.text = "Selected Particle Type: " + particleType;
    }

    #endregion

    #region Moving Platform
    void UpdateUIValuesMP()
    {
        if (platformControllerScript)
        {
            platformIDString = platformControllerScript.platformMoveid.ToString();
            platformIDInput.text = platformIDString;
            speedString = platformControllerScript.moveSpeed.ToString();
            speedInput.text = speedString;
            stopDistanceString = platformControllerScript.stopDistance.ToString();
            stopDistanceInput.text = stopDistanceString;
            if(platformControllerScript.behaviorType == WaypointBehaviorType.PingPong)
            {
                mpIsPingPong.isOn = true;
            }
            else
            {
                mpIsPingPong.isOn = false;
            }
            if(platformControllerScript.pathType == WaypointPathType.Closed)
            {
                mpIsClosed.isOn = true;
            }
            else
            {
                mpIsClosed.isOn = false;
            }
            mpIsInitialStarted.isOn = platformControllerScript.initialStart;
        }
    
                
    }
    #endregion
}
