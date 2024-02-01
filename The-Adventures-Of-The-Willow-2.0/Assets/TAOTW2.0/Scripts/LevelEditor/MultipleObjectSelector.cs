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


    [Header("Doors")]
    #region Doors
    [SerializeField] private GameObject DoorPanelPrefab;
    private GameObject DoorSelectedObject;
    private Door doorObjectScript;
    private GameObject DoorPanelInstance; 
    
    private TMP_InputField[] doorInputField;
    private TMP_InputField DoorIDInput;
    private TMP_InputField SecondDoorIDInput;
    private TMP_InputField PositionPointInput;

    private string DoorIDString;
    private string SecondDoorIDString;
    private string PositionPointString;
    private string SectorNameString;

    private Toggle[] doorToggles;
    private Toggle WithKey;
    private Toggle ToSector;

    private TMP_Dropdown[] doorDropdowns;
    private TMP_Dropdown sectorDropdown;

    private bool doorIsOpened = false;
    private Button[] doorButtons;
    private Button doorBackButton;
    private Button doorOkButton;

    #endregion

    [Header("SpawnPoints")]
    #region SpawnPoints

    [SerializeField] private GameObject spawnPointPanelPrefab;
    private GameObject spawnPointSelectedObject;
    private SpawnPoint spawnPointObjectScript;
    private GameObject spawnPointPanelInstance;

    private string SpawnPointID;

    private TMP_InputField[] spawnPointInputField;
    private TMP_InputField spawnPointIDInput;

    private string spawnPointIDString;

    private bool spawnPointIsOpened = false;
    private Button[] spawnPointButtons;
    private Button spawnPointBackButton;
    private Button spawnPointOkButton;
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

                        foreach (Toggle toggle in toggles)
                        {
                            if (toggle.name == "initial")
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
                            if (mpIsPingPong.isOn)
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
            #region Doors
            if (hit.collider != null && !doorIsOpened)
            {
                if (hit.collider != null && hit.collider.CompareTag("ObjectObject") && hit.collider.gameObject.name.StartsWith("Door"))
                {
                    doorIsOpened = true;
                    DoorSelectedObject = hit.collider.gameObject;
                    doorObjectScript = DoorSelectedObject.GetComponent<Door>();
                    DoorPanelInstance = Instantiate(DoorPanelPrefab, panelLocalization);

                    if (DoorPanelInstance != null)
                    {
                        doorToggles = DoorPanelInstance.GetComponentsInChildren<Toggle>();

                        foreach (Toggle toggle in doorToggles)
                        {
                            if (toggle.name == "WithKey")
                            {
                                WithKey = toggle;
                            }
                            if (toggle.name == "ToSector")
                            {
                                ToSector = toggle;
                            }
                        }
                        doorInputField = DoorPanelInstance.GetComponentsInChildren<TMP_InputField>();
                        foreach (TMP_InputField doorInput in doorInputField)
                        {
                            if (doorInput.name == "DoorID")
                            {
                                DoorIDInput = doorInput;
                            }
                            if (doorInput.name == "SecondDoorID")
                            {
                                SecondDoorIDInput = doorInput;
                            }
                            if (doorInput.name == "PositionPoint")
                            {
                                PositionPointInput = doorInput;
                            }
                        }
                        doorDropdowns = DoorPanelInstance.GetComponentsInChildren<TMP_Dropdown>();

                        foreach (TMP_Dropdown DoorDropdowns in doorDropdowns)
                        {
                            if (DoorDropdowns.name == "SectorName")
                            {
                                sectorDropdown = DoorDropdowns;

                                if (sectorDropdown != null)
                                {
                                    // Chame o método para preencher o Dropdown com os setores
                                    PopulateSectorDropdown();
                                    // Registre o método como um ouvinte para o evento OnValueChanged do Dropdown
                                    sectorDropdown.onValueChanged.AddListener(OnSectorDropdownValueChanged);

                                    // Localize o índice correspondente à SectorNameString
                                    int sectorIndex = GetSectorIndex(doorObjectScript.SectorName);

                                    // Defina o índice do Dropdown
                                    sectorDropdown.value = sectorIndex;
                                }
                            }
                        }

                        doorButtons = DoorPanelInstance.GetComponentsInChildren<Button>();
                        foreach (Button button in doorButtons)
                        {
                            if (button.name == "BackButton")
                            {
                                doorBackButton = button;
                            }
                            else if (button.name == "OKButton")
                            {
                                doorOkButton = button;
                            }
                        }

                        // Configurar eventos para botões
                        doorBackButton.onClick.AddListener(() =>
                        {
                            doorIsOpened = false;
                            Destroy(DoorPanelInstance);
                        });

                        doorOkButton.onClick.AddListener(() =>
                        {
                            doorObjectScript.DoorID = DoorIDString;
                            doorObjectScript.SecondDoorID = SecondDoorIDString;
                            doorObjectScript.WithKey = WithKey.isOn;
                            doorObjectScript.toSector = ToSector.isOn;
                            doorObjectScript.PositionPoint = PositionPointString;
                            doorObjectScript.SectorName = SectorNameString;

                            doorIsOpened = false;
                            Destroy(DoorPanelInstance);
                        });

                        UpdateUIValuesDoor();
                    }
                }

                
            }
            #endregion
            #region SpawnPoints
            if (hit.collider != null && !spawnPointIsOpened)
            {
                if (hit.collider != null && hit.collider.CompareTag("GameObject") && hit.collider.gameObject.name.StartsWith("SpawnPoint"))
                {
                    spawnPointIsOpened = true;
                    spawnPointSelectedObject = hit.collider.gameObject;
                    spawnPointObjectScript = spawnPointSelectedObject.GetComponent<SpawnPoint>();
                    spawnPointPanelInstance = Instantiate(spawnPointPanelPrefab, panelLocalization);

                    if (spawnPointPanelInstance != null)
                    {
                        spawnPointInputField = spawnPointPanelInstance.GetComponentsInChildren<TMP_InputField>();
                        foreach (TMP_InputField spawnPointInput in spawnPointInputField)
                        {
                            if (spawnPointInput.name == "SpawnPointID")
                            {
                                spawnPointIDInput = spawnPointInput;
                            }
                        }

                        spawnPointButtons = spawnPointPanelInstance.GetComponentsInChildren<Button>();
                        foreach (Button button in spawnPointButtons)
                        {
                            if (button.name == "BackButton")
                            {
                                spawnPointBackButton = button;
                            }
                            else if (button.name == "OKButton")
                            {
                                spawnPointOkButton = button;
                            }
                        }

                        // Configurar eventos para botões
                        spawnPointBackButton.onClick.AddListener(() =>
                        {
                            spawnPointIsOpened = false;
                            Destroy(spawnPointPanelInstance);
                        });

                        spawnPointOkButton.onClick.AddListener(() =>
                        {
                            spawnPointObjectScript.SpawnPointID = spawnPointIDString;

                            spawnPointIsOpened = false;
                            Destroy(spawnPointPanelInstance);
                        });

                        UpdateUIValuesSpawnPoint();
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
                if (speedInput != null)
                {
                    speedString = speedInput.text;
                }
                if (stopDistanceInput != null)
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

        #region Door
        if (doorIsOpened)
        {
            if (DoorPanelInstance != null)
            {
                if (DoorIDInput != null)
                {
                    DoorIDString = DoorIDInput.text;
                }
                if (SecondDoorIDInput != null)
                {
                    SecondDoorIDString = SecondDoorIDInput.text;
                }
                if (PositionPointInput != null)
                {
                    PositionPointString = PositionPointInput.text;
                }
            }
        }
        else
        {
            DoorSelectedObject = null;
            DoorPanelInstance = null;
            doorObjectScript = null;
        }
        #endregion
        #region SpawnPoint
        if (spawnPointIsOpened)
        {
            if (spawnPointPanelInstance != null)
            {
                if (spawnPointIDInput != null)
                {
                    spawnPointIDString = spawnPointIDInput.text;
                }
            }
        }
        else
        {
            spawnPointSelectedObject = null;
            spawnPointPanelInstance = null;
            spawnPointObjectScript = null;
        }
        #endregion
    }
    void PopulateSectorDropdown()
    {
        // Substitua isso pela lógica real de obtenção de setores ou adicione manualmente
        List<string> sectorNames = new List<string> { "Sector1", "Sector2", "Sector3", "Sector4", "Sector5" };

        // Limpe as opções existentes
        sectorDropdown.ClearOptions();

        // Adicione os setores ao Dropdown
        sectorDropdown.AddOptions(sectorNames);
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

    #region Door
    void UpdateUIValuesDoor()
    {
        if (doorObjectScript != null)
        {
            DoorIDString = doorObjectScript.DoorID;
            PositionPointString = doorObjectScript.PositionPoint;

            DoorIDInput.text = doorObjectScript.DoorID;
            PositionPointInput.text = doorObjectScript.PositionPoint;
            WithKey.isOn = doorObjectScript.WithKey;
            ToSector.isOn = doorObjectScript.toSector;
            SectorNameString = doorObjectScript.SectorName;
            SecondDoorIDInput.text = doorObjectScript.SecondDoorID;
            SecondDoorIDString = doorObjectScript.SecondDoorID;
        }
    }
    void OnSectorDropdownValueChanged(int value)
    {
        SectorNameString = sectorDropdown.options[value].text;

    }
    int GetSectorIndex(string sectorName)
    {
        // Obtenha o índice do setor no Dropdown com base no nome do setor
        List<TMP_Dropdown.OptionData> options = sectorDropdown.options;

        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].text == sectorName)
            {
                return i;
            }
        }

        // Se não encontrar, retorne um valor padrão (pode ajustar conforme necessário)
        return 0;
    }
    #endregion

    #region SpawnPoint

    void UpdateUIValuesSpawnPoint()
    {
        if (spawnPointObjectScript != null)
        {
            spawnPointIDString = spawnPointObjectScript.SpawnPointID;
            spawnPointIDInput.text = spawnPointObjectScript.SpawnPointID;
        }
    }
    #endregion
}
