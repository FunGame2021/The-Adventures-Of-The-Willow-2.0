using UnityEditor;
using UnityEngine;

public class PrefabThumbnailGenerator : MonoBehaviour
{
    public GameObject prefabToGenerateThumbnail; // Arraste o prefab aqui na Inspector
    public Camera thumbnailCamera; // Referência à câmera na cena de edição separada
    public int thumbnailSize = 32; // Tamanho da miniatura em pixels
    public void GenerateThumbnail()
    {
        
        if (prefabToGenerateThumbnail != null && thumbnailCamera != null)
        {
            RenderTexture renderTexture = new RenderTexture(thumbnailSize, thumbnailSize, 24);
            thumbnailCamera.targetTexture = renderTexture;
            thumbnailCamera.Render();

            RenderTexture.active = renderTexture;
            Texture2D thumbnail = new Texture2D(thumbnailSize, thumbnailSize);
            thumbnail.ReadPixels(new Rect(0, 0, thumbnailSize, thumbnailSize), 0, 0);
            thumbnail.Apply();

            RenderTexture.active = null;

            byte[] pngBytes = thumbnail.EncodeToPNG();

            // Construir o caminho do arquivo de saída usando o nome do objeto
            string objectName = prefabToGenerateThumbnail.name;
            string outputPath = "Assets/TAOTW2.0/LevelEditor/UI/Icons/" + objectName + "Thumbnail.png";

            System.IO.File.WriteAllBytes(outputPath, pngBytes);
            Debug.Log("Thumbnail generated and saved at: " + outputPath);

#if UNITY_EDITOR
    // Seu código a ser excluído da build final
            // Importar a miniatura como um asset no projeto
            AssetDatabase.ImportAsset(outputPath);
#endif
        }
        else
        {
            Debug.LogError("Prefab or Thumbnail Camera not assigned.");
        }
    }
}
