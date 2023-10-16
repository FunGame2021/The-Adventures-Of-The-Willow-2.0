using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LevelShoot : MonoBehaviour
{
    public static LevelShoot instance;

    public Camera shotCamera; // A câmera usada para visualizar a grade

    private int gridWidth;
    private int gridHeight;

    public GridVisualizer gridVisualizer;
    private bool isGridEnabledVisualizer;
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void CaptureAndSaveScreenshot(string levelName, int width, int height)
    {
        shotCamera.gameObject.SetActive(true);
        isGridEnabledVisualizer = gridVisualizer.isGridEnabled;

        if (isGridEnabledVisualizer)
        {
            gridVisualizer.OnEnableGrid();
        }

        // Renderize a cena na câmera
        shotCamera.Render();


        // Registre a posição e o tamanho da câmera original
        Vector3 originalCameraPosition = shotCamera.transform.position;
        float originalCameraSize = shotCamera.orthographicSize;

        // Calcule o tamanho necessário da câmera para cobrir o nível mantendo a proporção de 250x190
        float aspectRatio = 300.0f / 245.0f; // Proporção de aspecto 250x190
        float cameraSizeY = height / 2.0f;
        float cameraSizeX = cameraSizeY * aspectRatio;

        // Defina o tamanho da câmera
        shotCamera.orthographicSize = Mathf.Max(cameraSizeX, cameraSizeY);

        // Posicione a câmera no centro do nível
        Vector3 cameraPosition = new Vector3(width / 2.0f, height / 2.0f, originalCameraPosition.z);

        shotCamera.transform.position = cameraPosition;

        // Crie um RenderTexture para capturar o screenshot
        RenderTexture rt = new RenderTexture(300, 245, 24);
        shotCamera.targetTexture = rt;
        Texture2D screenshot = new Texture2D(300, 245, TextureFormat.RGB24, false);

        // Renderize a cena na câmera
        shotCamera.Render();

        // Leia os pixels na textura de screenshot
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, 300, 245), 0, 0);
        shotCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Converta a textura do screenshot em um array de bytes JPEG
        byte[] bytes = screenshot.EncodeToJPG(100);

        // Volte a câmera para a posição e tamanho originais
        shotCamera.transform.position = originalCameraPosition;
        shotCamera.orthographicSize = originalCameraSize;

        // Defina o nome do arquivo para o screenshot com base no nome do nível
        string screenshotFileName = levelName + ".jpg";


        // Crie o caminho completo para a pasta "WorldShots" na pasta do mundo atual
        string worldShotsFolder = Path.Combine(Path.Combine(WorldManager.instance.levelEditorPath, WorldManager.instance.currentWorldName), "WorldShots");

        // Verifique se a pasta "WorldShots" existe e, se não existir, crie-a
        if (!Directory.Exists(worldShotsFolder))
        {
            Directory.CreateDirectory(worldShotsFolder);
        }

        // Combine o caminho da pasta "WorldShots" com o nome do arquivo do screenshot
        string screenshotFilePath = Path.Combine(worldShotsFolder, screenshotFileName);

        // Verifique se o arquivo do screenshot já existe
        if (File.Exists(screenshotFilePath))
        {
            // Se o arquivo existir, exclua-o antes de salvar o novo screenshot
            File.Delete(screenshotFilePath);
        }

        // Salve o screenshot em formato JPEG na pasta "WorldShots"
        File.WriteAllBytes(screenshotFilePath, bytes);

        Debug.Log("Screenshot saved to: " + screenshotFilePath);
        shotCamera.gameObject.SetActive(false);
        if (isGridEnabledVisualizer)
        {
            gridVisualizer.OnEnableGrid();
        }
    }

}
