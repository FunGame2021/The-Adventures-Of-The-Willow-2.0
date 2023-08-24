using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileCategoryData", menuName = "LevelEditor/Tile Category Data")]
public class TileCategoryData : ScriptableObject
{
    public string categoryName; // Nome da categoria
    public TileBase[] tiles; // Conjunto de telhas da categoria
}

