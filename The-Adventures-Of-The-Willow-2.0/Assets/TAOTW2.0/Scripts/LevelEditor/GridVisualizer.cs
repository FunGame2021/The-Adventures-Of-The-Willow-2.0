using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridVisualizer : MonoBehaviour
{
    #region Tile normal Grid
    [SerializeField] private GameObject GridVisualizerObj;
    [SerializeField] private Transform squareTransform; // Referência ao Transform do quad
    [SerializeField] private Material materialWithShader; // Material com o Shader Graph
    public bool isGridEnabled;
    [SerializeField] private Button gridViewButton;

    private void Start()
    {
        isGridEnabled = true;
    }

    public void OnGridSizeUpdated()
    {
        if (isGridEnabled)
        {
            GridVisualizerObj.SetActive(true);
            // Atualiza a escala do quad com base no tamanho do grid
            Vector3 newScale = new Vector3(LevelEditorManager.instance.currentGridWidth, LevelEditorManager.instance.currentGridHeight, 1f);
            squareTransform.localScale = newScale;

            // Altera o valor do TileAmount no Material usando Shader Graph
            Vector2 newTileAmountVector = new Vector2(LevelEditorManager.instance.currentGridWidth, LevelEditorManager.instance.currentGridHeight); // Novo valor do TileAmount como Vector2
            materialWithShader.SetVector("_TileAmount", newTileAmountVector);
        }
        else
        {
            GridVisualizerObj.SetActive(false);
        }
    }

    public void OnEnableGrid()
    {
        isGridEnabled = !isGridEnabled;


        // Defina a cor do botão com base no estado do snapGrid
        ColorBlock colors = gridViewButton.colors;
        colors.normalColor = isGridEnabled ? Color.red : Color.white; // Altera para vermelho se snapGrid estiver ativado
        gridViewButton.colors = colors;
        OnGridSizeUpdated();
    }

    #endregion

    #region snap Object Size 
    [SerializeField] private GameObject SnapGridVisualizerObj;
    [SerializeField] private Transform SnapSquareTransform; // Referência ao Transform do quad
    [SerializeField] private Material SnapMaterialWithShader; // Material com o Shader Graph
    public void OnSnapGridSizeUpdated()
    {
        if (LevelEditorManager.instance.snapGrid)
        {
            SnapGridVisualizerObj.SetActive(true);

            // Tamanho total do tilemap
            Vector3 totalGridSize = new Vector3(LevelEditorManager.instance.currentGridWidth, LevelEditorManager.instance.currentGridHeight, 1f);

            // Escala do quad em relação ao tamanho total do tilemap
            SnapSquareTransform.localScale = totalGridSize;

            // Valor do TileAmount no Material usando Shader Graph, ajustado para o tamanho do SnapGridSize
            Vector2 newTileAmountVector = new Vector2(totalGridSize.x / LevelEditorManager.instance.SnapGridSize, totalGridSize.y / LevelEditorManager.instance.SnapGridSize);
            SnapMaterialWithShader.SetVector("_TileAmount", newTileAmountVector);
        }
        else
        {
            SnapGridVisualizerObj.SetActive(false);
        }
    }


    #endregion

}
