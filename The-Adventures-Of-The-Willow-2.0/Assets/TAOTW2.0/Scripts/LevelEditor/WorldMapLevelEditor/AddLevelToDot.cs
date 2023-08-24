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

    [SerializeField] private Transform levelButtonContainer; // Container para os botões dos níveis
    [SerializeField] private RectTransform levelContent;
    [SerializeField] private Button levelButtonPrefab; // Prefab do botão do nível
    [HideInInspector] public string levelEditorPath; // Caminho para a pasta "LevelEditor"
    [SerializeField] private GameObject levelListPanel; // Referência para o painel com a lista de mundos

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
        // Obtém o caminho completo para a pasta "LevelEditor"
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
                newPosition.z = selectedLevelDot.transform.position.z; // Mantém o valor do ZPos
                selectedLevelDot.transform.position = newPosition;
            }
        }

    }

    private void LateUpdate()
    {
        if (selectedLevelDot != null && Mouse.current.leftButton.isPressed)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) + DotOffset;
            newPosition.z = selectedLevelDot.transform.position.z; // Mantém o valor do ZPos
            selectedLevelDot.transform.position = newPosition;

            // Obtém o componente MoveableObject do objeto selecionado
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
        // Verifica se o container dos botões de nível está definido
        if (levelButtonContainer == null)
        {
            UnityEngine.Debug.LogWarning("O container de botões de nível não está definido.");
            return;
        }

        // Remove os botões de nível existentes
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Obtém o caminho completo para a pasta do mundo
        string worldFolderPath = Path.Combine(levelEditorPath, worldName);

        // Verifica se a pasta do mundo existe
        if (Directory.Exists(worldFolderPath))
        {
            // Obtém todos os arquivos com a extensão ".TAOWLE" dentro da pasta do mundo
            string[] levelFiles = Directory.GetFiles(worldFolderPath, "*.TAOWLE");

            // Instancia um botão para cada nível disponível
            foreach (string levelFile in levelFiles)
            {
                // Obtém o nome do nível
                string levelName = Path.GetFileNameWithoutExtension(levelFile);

                // Cria um novo botão de nível
                Button levelButton = Instantiate(levelButtonPrefab, levelButtonContainer);
                levelButton.GetComponentInChildren<TextMeshProUGUI>().text = levelName;

                // Configura o callback de clique para abrir o nível correspondente
                string level = levelName; // Variável temporária para evitar closure
                levelButton.onClick.AddListener(() =>
                {
                    // Define o nome do nível selecionado
                    currentLevelName = level;

                    // Obtém o caminho completo para o nível
                    currentLevelPath = Path.Combine(worldFolderPath, level + ".TAOWLE");

                    // Seleciona o nível no LevelDot
                    UpdateLevelDotValues();

                    // Fecha o painel atual
                    if (levelListPanel != null)
                    {
                        levelListPanel.SetActive(false);
                    }

                });

                // Ativa o botão
                levelButton.gameObject.SetActive(true);
            }

            // Atualiza o tamanho vertical do Content do ScrollView de acordo com o número de botões
            float buttonHeight = levelButtonPrefab.GetComponent<RectTransform>().rect.height;
            float totalHeight = levelFiles.Length * buttonHeight;
            levelContent.sizeDelta = new Vector2(levelContent.sizeDelta.x, totalHeight);
        }
    }
}
