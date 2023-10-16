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

    [SerializeField] private string currentLevel;
    [SerializeField] private string currentWorld;

    public delegate void MouseClick();
    public static event MouseClick OnMouseClick;
    public static Vector3 mousePosition;
    public static GameObject selectedLevelDot;
    public bool isSelectingLevel;
    Vector3 DotOffset;

    [SerializeField] private GameObject PanelOptions;
    [SerializeField] private TextMeshProUGUI LevelDotLevelName;
    [SerializeField] private Toggle isFirstLevel;
    private LevelDot currentLevelDotComponent;


    private void Start()
    {
        // Obtém o caminho completo para a pasta "LevelEditor"
        levelEditorPath = Path.Combine(Application.persistentDataPath, "LevelEditor");
        isSelectingLevel = false;
    }
    private void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && (hit.collider.CompareTag("LevelDot") || hit.collider.GetComponent<LevelDot>()))
            {
                selectedLevelDot = hit.collider.gameObject;
                OnMouseClick?.Invoke();
                if (selectedLevelDot)
                {
                    isSelectingLevel = true;
                    PanelOptions.SetActive(true);
                    currentLevelDotComponent = selectedLevelDot.GetComponent<LevelDot>();
                    UpdateLevelDotValues();
                }
            }
        }


        //if (OnMouseClick != null && Mouse.current.leftButton.wasPressedThisFrame && !isSelectingLevel)
        //{
        //    OnMouseClick();
        //    if (selectedLevelDot)
        //    {
        //        // Converte a posição do mouse para as coordenadas locais do selectedLevelDot
        //        Vector3 localMousePosition = selectedLevelDot.transform.InverseTransformPoint(mousePosition);
        //        DotOffset = selectedLevelDot.transform.localPosition - localMousePosition;
        //    }
        //}
        //else if (OnMouseClick != null && Mouse.current.leftButton.isPressed && !isSelectingLevel)
        //{
        //    if (selectedLevelDot)
        //    {
        //        // Converte a posição do mouse para as coordenadas locais do selectedLevelDot
        //        Vector3 localMousePosition = selectedLevelDot.transform.InverseTransformPoint(mousePosition);
        //        Vector3 newPosition = localMousePosition + DotOffset;
        //        selectedLevelDot.transform.localPosition = newPosition;
        //    }
        //}
    }

    //private void LateUpdate()
    //{
    //    if (selectedLevelDot != null && Mouse.current.leftButton.isPressed && !isSelectingLevel)
    //    {
    //        // Converte a posição do mouse para as coordenadas locais do selectedLevelDot
    //        Vector3 localMousePosition = selectedLevelDot.transform.InverseTransformPoint(mousePosition);
    //        Vector3 newPosition = localMousePosition + DotOffset;
    //        selectedLevelDot.transform.localPosition = newPosition;

    //        // Obtém o componente MoveableObject do objeto selecionado
    //        LevelDot levelDot = selectedLevelDot.GetComponent<LevelDot>();
    //    }
    //}

    private void UpdateLevelDotValues()
    {
        currentWorld = currentLevelDotComponent.worldName;
        currentLevel = currentLevelDotComponent.levelName;
        LevelDotLevelName.text = currentLevelDotComponent.levelName;
        isFirstLevel.isOn = currentLevelDotComponent.isFirstLevel;
    }


    public void OnOkClick()
    {
        if (currentLevelDotComponent != null)
        {
            currentLevelDotComponent.SetLevelPath(currentWorld, currentLevel);
            LevelDotLevelName.text = currentLevel;
            currentLevelDotComponent.isFirstLevel = isFirstLevel.isOn;
            PanelOptions.SetActive(false);
            isSelectingLevel = false;
            UpdateLevelDotValues();
        }
    }

    public void AddLevelDot()
    {
        InstantiateLevelButtons(WorldManager.instance.currentWorldName);
        levelListPanel.SetActive(true);
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
                    currentLevel = level;
                    currentWorld = WorldManager.instance.currentWorldName;

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
