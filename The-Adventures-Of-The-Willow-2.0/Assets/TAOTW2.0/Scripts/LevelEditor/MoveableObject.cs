using System.Linq;
using UnityEngine;

public class MoveableObject : MonoBehaviour
{
    private Renderer myRenderer;
    private Color defaultColor;
    public Color selectedColor;

    // Propriedades adicionadas
    public float ZPos;
    public int ShortLayer;
    public string ShortLayerName;

    private void Start()
    {
        myRenderer = GetComponent<Renderer>();
        defaultColor = myRenderer.material.color;

        // Obter as opções de short layers
        string[] shortLayerOptions = SortingLayer.layers.Select(layer => layer.name).ToArray();
        // Definir a opção inicial como o short layer atual
        ShortLayerName = GetShortLayerName(myRenderer.sortingLayerID);
    }

    void CheckForClick()
    {
        Vector3 positionToCheck = MoveAndSelectTool.mousePosition;
        positionToCheck.z = myRenderer.bounds.center.z;

        if (myRenderer.bounds.Contains(positionToCheck))
        {
            MoveAndSelectTool.selectedDecorObject = gameObject;

            // Define as propriedades ShortLayer e ZPos com os valores desejados
            ShortLayer = myRenderer.sortingLayerID; // Obter o valor int do short layer
            ZPos = transform.position.z; // Use a posição z do objeto como valor para ZPos

            // Obtenha o nome do ShortLayer
            ShortLayerName = GetShortLayerName(ShortLayer);

            // Altere a cor do objeto para a cor selecionada
            myRenderer.material.color = selectedColor;
        }
        else
        {
            // Restaurar a cor original
            myRenderer.material.color = defaultColor;
        }
    }


    private string GetShortLayerName(int layerID)
    {
        // Encontre o nome correspondente ao ID do short layer no objeto atual
        foreach (var layer in SortingLayer.layers)
        {
            if (layer.id == layerID)
            {
                return layer.name;
            }
        }

        // Retorna uma string vazia se não encontrar correspondência
        return string.Empty;
    }

    void OnEnable()
    {
        MoveAndSelectTool.OnMouseClick += CheckForClick;
    }

    void OnDisable()
    {
        MoveAndSelectTool.OnMouseClick -= CheckForClick;

        // Restaurar a cor original antes de desativar o objeto
        myRenderer.material.color = defaultColor;
    }

    public void ApplyChanges()
    {
        // Obtém o componente SpriteRenderer deste objeto MoveableObject
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Define a ordem de classificação (sorting order) do SpriteRenderer com o valor de ShortLayer
            spriteRenderer.sortingOrder = ShortLayer;

            // Atualiza o nome do sorting layer do Renderer
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sortingLayerName = ShortLayerName;
            }
        }

        // Define a posição Z do objeto com o valor de ZPos
        Vector3 newPosition = transform.position;
        newPosition.z = ZPos;
        transform.position = newPosition;
    }


}
