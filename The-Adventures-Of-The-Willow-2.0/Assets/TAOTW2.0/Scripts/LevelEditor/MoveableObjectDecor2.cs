using System.Linq;
using UnityEngine;

public class MoveableObjectDecor2 : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] myRenderers;

    // Propriedades adicionadas
    public int ShortLayer;
    public string ShortLayerName;


    private void Start()
    {
        // Obter as opções de short layers
        string[] shortLayerOptions = SortingLayer.layers.Select(layer => layer.name).ToArray();
        // Definir a opção inicial como o short layer atual do primeiro SpriteRenderer
        ShortLayerName = GetShortLayerName(myRenderers[0].sortingLayerID);
        ShortLayer = myRenderers[0].sortingOrder;
    }

    void CheckForClick2()
    {
        Vector3 positionToCheck = MoveAndSelectTool.mousePosition;
        positionToCheck.z = myRenderers[0].bounds.center.z;

        if (myRenderers[0].bounds.Contains(positionToCheck))
        {
            MoveAndSelectTool.selectedDecor2Object = gameObject;

            // Obtenha o nome do ShortLayer do primeiro SpriteRenderer
            ShortLayerName = GetShortLayerName(myRenderers[0].sortingLayerID);
            // Obtenha o ShortLayer do primeiro SpriteRenderer
            ShortLayer = myRenderers[0].sortingOrder;
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
        MoveAndSelectTool.OnMouseClick2 += CheckForClick2;
    }

    void OnDisable()
    {
        MoveAndSelectTool.OnMouseClick2 -= CheckForClick2;
    }

    public void ApplyChanges()
    {
        foreach (Renderer renderer in myRenderers)
        {
            // Verificar se o renderer é um SpriteRenderer
            if (renderer is SpriteRenderer spriteRenderer)
            {
                // Define a ordem de classificação (sorting order) do SpriteRenderer com o valor de ShortLayer
                spriteRenderer.sortingOrder = ShortLayer;

                // Atualiza o nome do sorting layer do SpriteRenderer
                spriteRenderer.sortingLayerName = ShortLayerName;
            }
        }
    }


}
