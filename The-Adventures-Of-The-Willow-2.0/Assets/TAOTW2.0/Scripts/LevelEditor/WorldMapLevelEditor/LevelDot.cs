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
    public string levelPath;
    public bool isFirstLevel;
    private Vector3 dotPosition;

    private void Start()
    {
        levelName = Path.GetFileNameWithoutExtension(levelPath);

        myRenderer = GetComponent<Renderer>();
        defaultColor = myRenderer.material.color;
    }
    public void SetLevelPath(string path)
    {
        levelPath = path;
    }

    public string GetLevelPath()
    {
        return levelPath;
    }

    public void SetDotPosition(Vector3 position)
    {
        dotPosition = position;
    }

    public Vector3 GetDotPosition()
    {
        return dotPosition;
    }
    void CheckForClick()
    {
        Vector3 positionToCheck = AddLevelToDot.mousePosition;

        if (myRenderer.bounds.Contains(positionToCheck))
        {
            AddLevelToDot.selectedLevelDot = gameObject;

            // Altere a cor do objeto para a cor selecionada
            myRenderer.material.color = selectedColor;
        }
        else
        {
            // Restaurar a cor original
            myRenderer.material.color = defaultColor;
        }
    }
    void OnEnable()
    {
        AddLevelToDot.OnMouseClick += CheckForClick;
    }

    void OnDisable()
    {
        AddLevelToDot.OnMouseClick -= CheckForClick;

        // Restaurar a cor original antes de desativar o objeto
        myRenderer.material.color = defaultColor;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            
        }
    }
}
