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
        // Obt�m o caminho completo para a pasta "LevelEditor"
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
        //        // Converte a posi��o do mouse para as coordenadas locais do selectedLevelDot
        //        Vector3 localMousePosition = selectedLevelDot.transform.InverseTransformPoint(mousePosition);
        //        DotOffset = selectedLevelDot.transform.localPosition - localMousePosition;
        //    }
        //}
        //else if (OnMouseClick != null && Mouse.current.leftButton.isPressed && !isSelectingLevel)
        //{
        //    if (selectedLevelDot)
        //    {
        //        // Converte a posi��o do mouse para as coordenadas locais do selectedLevelDot
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
    //        // Converte a posi��o do mouse para as coordenadas locais do selectedLevelDot
    //        Vector3 localMousePosition = selectedLevelDot.transform.InverseTransformPoint(mousePosition);
    //        Vector3 newPosition = localMousePosition + DotOffset;
    //        selectedLevelDot.transform.localPosition = newPosition;

    //        // Obt�m o componente MoveableObject do objeto selecionado
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
                    currentLevel = level;
                    currentWorld = WorldManager.instance.currentWorldName;

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
