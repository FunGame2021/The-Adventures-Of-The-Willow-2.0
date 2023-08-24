using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapFixer : MonoBehaviour
{
    public Grid tilemapGrid; // A referência para o Grid que contém os Tilemaps
    public Material newMaterial; // O novo material que você quer atribuir


    private void Start()
    {
        RefreshAllTilemaps();
    }

    public void RefreshAllTilemaps()
    {
        Tilemap[] tilemaps = tilemapGrid.GetComponentsInChildren<Tilemap>();

        foreach (Tilemap tilemap in tilemaps)
        {
            // Atribui o novo material ao TilemapRenderer
            TilemapRenderer tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
            if (tilemapRenderer != null)
            {
                tilemapRenderer.material = newMaterial;
            }
            else
            {
                Debug.LogWarning("TilemapRenderer component not found on Tilemap: " + tilemap.name);
            }

            // Atualiza os materiais e shaders
            tilemap.RefreshAllTiles();
        }
    }
}
