using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class LevelDot : MonoBehaviour
{
    private Renderer myRenderer;
    private Color defaultColor;
    public Color selectedColor;

    public string levelName;
    public string worldName;
    public string LevelPath;
    public bool isFirstLevel;
    private Vector3 dotPosition;

    //UI Canvas
    [SerializeField] private Transform spawnLocalUI;
    private TextMeshProUGUI TXTLevelName;
    private Animator CanvasUIAnim;
    [SerializeField] private GameObject CanvasWorldUIPrefab;
    private GameObject canvasWorldUIObject;
    private bool canvasWorldUIInstantiated = false;
    private bool isExiting = false;
    public Image levelImage;

    private void Start()
    {
        // Verifica se worldName e levelName já foram definidos antes de criar o caminho do nível
        if (string.IsNullOrEmpty(worldName) || string.IsNullOrEmpty(levelName))
        {
            Debug.LogWarning("WorldName or LevelName not set. Please use SetLevelPath() to set them.");
        }
        else
        {
            string[] pathParts = { Application.persistentDataPath, "LevelEditor", worldName, levelName + ".TAOWLE" };
            LevelPath = Path.Combine(pathParts);
        }

        myRenderer = GetComponent<Renderer>();
        defaultColor = myRenderer.material.color;

    }

    public void SetLevelPath(string setWorldName, string setLevelName)
    {
        worldName = setWorldName;
        levelName = setLevelName;

        // Agora que você tem os valores definidos, você pode criar o caminho do nível
        string[] pathParts = { Application.persistentDataPath, "LevelEditor", worldName, levelName + ".TAOWLE" };
        LevelPath = Path.Combine(pathParts);
    }


    public string GetLevelPath()
    {
        return LevelPath;
    }

    public void SetDotPosition(Vector3 position)
    {
        dotPosition = position;
    }

    public Vector3 GetDotPosition()
    {
        return dotPosition;
    }
    //void CheckForClick()
    //{
    //    Vector3 positionToCheck = AddLevelToDot.mousePosition;

    //    if (myRenderer.bounds.Contains(positionToCheck))
    //    {
    //        AddLevelToDot.selectedLevelDot = gameObject;
    //        Debug.Log("LevelDot selected: " + levelName); // Debug para verificar a seleção do LevelDot

    //        // Altere a cor do objeto para a cor selecionada
    //        myRenderer.material.color = selectedColor;
    //    }
    //    else
    //    {
    //        // Restaurar a cor original
    //        myRenderer.material.color = defaultColor;
    //    }
    //}

    //void OnEnable()
    //{
    //    AddLevelToDot.OnMouseClick += CheckForClick;
    //    Debug.Log("LevelDot enabled and subscribed to mouse click event."); // Debug para verificar a ativação e inscrição no evento de clique do mouse
    //}

    //void OnDisable()
    //{
    //    AddLevelToDot.OnMouseClick -= CheckForClick;

    //    // Restaurar a cor original antes de desativar o objeto
    //    myRenderer.material.color = defaultColor;
    //    Debug.Log("LevelDot disabled and unsubscribed from mouse click event."); // Debug para verificar a desativação e cancelamento da inscrição no evento de clique do mouse
    //}
    // Função para instanciar e configurar o Canvas World UI

    private void LoadLevelImage()
    {
        // Crie o caminho para a imagem com base no nome do nível
        string imagePath = Path.Combine(Application.persistentDataPath, "LevelEditor", worldName, "WorldShots", levelName + ".jpg");

        // Verifique se o arquivo de imagem existe
        if (File.Exists(imagePath))
        {
            // Carregue a textura da imagem
            Texture2D levelImageTexture = new Texture2D(2, 2); // Tamanho arbitrário
            byte[] imageData = File.ReadAllBytes(imagePath);
            if (levelImageTexture.LoadImage(imageData))
            {
                // Concluído o carregamento da imagem

                // Aplique a textura à UI Image
                levelImage.sprite = Sprite.Create(levelImageTexture, new Rect(0, 0, levelImageTexture.width, levelImageTexture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogWarning("Falha ao carregar a imagem.");
            }
        }
        else
        {
            Debug.LogWarning("A imagem não foi encontrada para o nível: " + levelName);
        }
    }

    private void InstantiateCanvasWorldUI()
    {
        // Verifique se o painel já foi instanciado
        if (canvasWorldUIInstantiated)
        {
            // Já foi instanciado, não faça nada
            return;
        }
        // Instancie o objeto do prefab
        canvasWorldUIObject = Instantiate(CanvasWorldUIPrefab, spawnLocalUI);

        // Verifique se a instância foi bem-sucedida
        if (canvasWorldUIObject != null)
        {
            // Obtenha as referências para o TextMeshPro e o Animator no objeto instanciado
            TXTLevelName = canvasWorldUIObject.GetComponentInChildren<TextMeshProUGUI>();
            CanvasUIAnim = canvasWorldUIObject.GetComponentInChildren<Animator>();
            levelImage = FindImageInChild(canvasWorldUIObject.transform, "UILevelImage");

            // Verifique se as referências são válidas
            if (TXTLevelName != null && CanvasUIAnim != null)
            {
                TXTLevelName.text = levelName;
                // txtAuthorName.text = authorName;
                // Aplique a textura da imagem ao UI Image
                if (levelImage != null)
                {
                    LoadLevelImage();
                }
                CanvasUIAnim.SetTrigger("EnterDot");
                canvasWorldUIInstantiated = true;

            }
            else
            {
                Debug.LogError("Texto ou Animator não encontrados no objeto instanciado.");
            }
        }
        else
        {
            Debug.LogError("Falha ao instanciar o Canvas World UI.");
        }
    }
    Image FindImageInChild(Transform parent, string name)
    {
        Image foundImage = null;

        foreach (Transform child in parent)
        {
            Image childImage = child.GetComponent<Image>();

            if (childImage != null && child.name == name)
            {
                foundImage = childImage;
                break; // Encerra o loop se encontrar a imagem
            }

            // Chama a função recursivamente para os filhos deste filho
            Image recursiveChildImage = FindImageInChild(child, name);

            if (recursiveChildImage != null)
            {
                foundImage = recursiveChildImage;
                break; // Encerra o loop se encontrar a imagem
            }
        }

        return foundImage;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            InstantiateCanvasWorldUI();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (canvasWorldUIObject != null && CanvasUIAnim != null && !isExiting)
            {
                isExiting = true;
                CanvasUIAnim.SetTrigger("ExitDot");
                StartCoroutine(DestroyCanvasWithDelay());
            }
        }
    }
    private IEnumerator DestroyCanvasWithDelay()
    {
        // Aguarde um determinado número de segundos antes de destruir o objeto
        yield return new WaitForSeconds(1.7f); // Altere o valor 2.0f para o número de segundos desejado

        // Após o atraso, destrua o objeto
        Destroy(canvasWorldUIObject);
        canvasWorldUIInstantiated = false;
        isExiting = false;
    }
}
