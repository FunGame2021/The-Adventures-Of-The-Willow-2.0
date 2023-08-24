using UnityEngine;
using UnityEngine.UI;

public class GameObjectButton : MonoBehaviour
{
    public static GameObjectButton instance;
    public string selectedGameObjectName;
    private string thiselectedGameObjectName;

    public Image uiImage; // O elemento de UI onde voc� deseja exibir a imagem
    public Sprite SpriteImageToUI;

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }

        // Adicionar um ouvinte de clique ao bot�o
        GetComponent<Button>().onClick.AddListener(OnClick);

        // Atribuir o sprite � imagem
        uiImage.sprite = SpriteImageToUI;
    }

    private void OnClick()
    {
        if (DecorButton.instance != null)
        {
            DecorButton.instance.Deselect();
        }
        if (ObjectsButton.instance != null)
        {
            ObjectsButton.instance.Deselect();
        }
        if (Decor2Button.instance != null)
        {
            Decor2Button.instance.Deselect();
        }
        if (EnemyButton.instance != null)
        {
            EnemyButton.instance.Deselect();
        }

        // Deselecionar todos os outros bot�es de inimigo
        DeselectAllGameObjects();
        TileButton.instance.DisableFill();
        TileButton.instance.ClearSelectedTile();
        thiselectedGameObjectName = selectedGameObjectName;
        // Informar ao LevelEditor qual inimigo foi selecionado
        LevelEditorManager.instance.SelectGameObject(thiselectedGameObjectName);
    }

    public void DeselectAllGameObjects()
    {
        // Percorrer todos os bot�es de inimigo
        GameObjectButton[] gameObjectButtons = FindObjectsOfType<GameObjectButton>();
        foreach (GameObjectButton gameObjectButton in gameObjectButtons)
        {
            // Desmarcar o bot�o se for diferente deste
            if (gameObjectButton != this)
            {
                gameObjectButton.Deselect();
            }
        }
    }

    public void Deselect()
    {
        // Limpar o nome do inimigo selecionado
        thiselectedGameObjectName = null;
    }
}
