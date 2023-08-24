using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

public class AddLevelToDot : MonoBehaviour
{

    [SerializeField] private Transform levelButtonContainer; // Container para os bot�es dos n�veis
    [SerializeField] private RectTransform levelContent;
    [SerializeField] private Button levelButtonPrefab; // Prefab do bot�o do n�vel
    [HideInInspector] public string levelEditorPath; // Caminho para a pasta "LevelEditor"
    [SerializeField] private GameObject levelListPanel; // Refer�ncia para o painel com a lista de mundos

    private string currentLevelName;
    private string currentLevelPath;


    public delegate void MouseClick();
    public static event MouseClick OnMouseClick;
    public static Vector3 mousePosition;
    public static GameObject selectedLevelDot;
    Vector3 DotOffset;

    [SerializeField] private GameObject PanelOptions;
    [SerializeField] private TextMeshProUGUI LevelDotLevelName;
    [SerializeField] private Toggle isFirstLevel;
    private LevelDot currentLevelDotComponent;


    private void Start()
    {
        // Obt�m o caminho completo para a pasta "LevelEditor"
        levelEditorPath = Path.Combine(Application.persistentDataPath, "LevelEditor");
    }
    private void Update()
    {
        if (OnMouseClick != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            OnMouseClick();
            if (selectedLevelDot)
            {
                PanelOptions.SetActive(true);
                UpdateLevelDotValues();
                currentLevelDotComponent = selectedLevelDot.GetComponent<LevelDot>();
            }
        }

        if (OnMouseClick != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnMouseClick();
            if (selectedLevelDot)
            {
                DotOffset = selectedLevelDot.transform.position - mousePosition;
            }
        }
        else if (OnMouseClick != null && Mouse.current.leftButton.isPressed)
        {
            if (selectedLevelDot)
            {
                Vector3 newPosition = mousePosition + DotOffset;
                newPosition.z = selectedLevelDot.transform.position.z; // Mant�m o valor do ZPos
                selectedLevelDot.transform.position = newPosition;
            }
        }

    }

    private void LateUpdate()
    {
        if (selectedLevelDot != null && Mouse.current.leftButton.isPressed)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) + DotOffset;
            newPosition.z = selectedLevelDot.transform.position.z; // Mant�m o valor do ZPos
            selectedLevelDot.transform.position = newPosition;

            // Obt�m o componente MoveableObject do objeto selecionado
            LevelDot levelDot = selectedLevelDot.GetComponent<LevelDot>();

        }
    }

    private void UpdateLevelDotValues()
    { 
        LevelDotLevelName.text = currentLevelDotComponent.levelName;
        currentLevelDotComponent.levelName = Path.GetFileNameWithoutExtension(currentLevelDotComponent.levelPath);
        currentLevelPath = currentLevelDotComponent.levelPath;
        isFirstLevel.isOn = currentLevelDotComponent.isFirstLevel;
    }

    public void OnOkClick()
    {
        currentLevelDotComponent.levelPath = currentLevelPath;
        currentLevelDotComponent.isFirstLevel = isFirstLevel;
        PanelOptions.SetActive(false);
    }

    public void AddLevelDot()
    {
        InstantiateLevelButtons(WorldManager.instance.currentWorldName);
    }

    private void InstantiateLevelButtons(string worldName)
    {
        // Verifica se o container dos bot�es de n�vel est� definido
        if (levelButtonContainer == null)
        {
            UnityEngine.Debug.LogWarning("O container de bot�es de n�vel n�o est� definido.");
            return;
        }

        // Remove os bot�es de n�vel existentes
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Obt�m o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo existe
        if (Directory.Exists(worldFolderPath))
        {
            // Obt�m todos os arquivos com a extens�o ".TAOWLE" dentro da pasta do mundo
            string[] levelFiles = Directory.GetFiles(worldFolderPath, "*.TAOWLE");

            // Instancia um bot�o para cada n�vel dispon�vel
            foreach (string levelFile in levelFiles)
            {
                // Obt�m o nome do n�vel
                string levelName = Path.GetFileNameWithoutExtension(levelFile);

                // Cria um novo bot�o de n�vel
                Button levelButton = Instantiate(levelButtonPrefab, levelButtonContainer);
                levelButton.GetComponentInChildren<TextMeshProUGUI>().text = levelName;

                // Configura o callback de clique para abrir o n�vel correspondente
                string level = levelName; // Vari�vel tempor�ria para evitar closure
                levelButton.onClick.AddListener(() =>
                {
                    // Define o nome do n�vel selecionado
                    currentLevelName = level;

                    // Obt�m o caminho completo para o n�vel
                    currentLevelPath = Path.Combine(worldFolderPath, level + ".TAOWLE");

                    // Seleciona o n�vel no LevelDot
                    UpdateLevelDotValues();

                    // Fecha o painel atual
                    if (levelListPanel != null)
                    {
                        levelListPanel.SetActive(false);
                    }

                });

                // Ativa o bot�o
                levelButton.gameObject.SetActive(true);
            }

            // Atualiza o tamanho vertical do Content do ScrollView de acordo com o n�mero de bot�es
            float buttonHeight = levelButtonPrefab.GetComponent<RectTransform>().rect.height;
            float totalHeight = levelFiles.Length * buttonHeight;
            levelContent.sizeDelta = new Vector2(levelContent.sizeDelta.x, totalHeight);
        }
    }
}
