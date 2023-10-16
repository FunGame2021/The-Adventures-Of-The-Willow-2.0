using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;

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
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            
        }
    }
}
