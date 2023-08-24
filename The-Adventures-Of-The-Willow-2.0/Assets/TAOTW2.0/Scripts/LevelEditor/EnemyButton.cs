using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EnemyButton : MonoBehaviour
{
    public static EnemyButton instance;
    public string selectedEnemyName;
    private string thiselectedEnemyName;

    public Image uiImage; // O elemento de UI onde você deseja exibir a imagem
    public Sprite SpriteImageToUI;

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }

        // Adicionar um ouvinte de clique ao botão
        GetComponent<Button>().onClick.AddListener(OnClick);

        // Atribuir o sprite à imagem
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
        // Deselecionar todos os outros botões de inimigo
        DeselectAllEnemies();
        TileButton.instance.DisableFill();
        TileButton.instance.ClearSelectedTile();
        thiselectedEnemyName = selectedEnemyName;
        // Informar ao LevelEditor qual inimigo foi selecionado
        LevelEditorManager.instance.SelectEnemy(thiselectedEnemyName);
    }

    public void DeselectAllEnemies()
    {
        // Percorrer todos os botões de inimigo
        EnemyButton[] enemyButtons = FindObjectsOfType<EnemyButton>();
        foreach (EnemyButton enemyButton in enemyButtons)
        {
            // Desmarcar o botão se for diferente deste
            if (enemyButton != this)
            {
                enemyButton.Deselect();
            }
        }
    }

    public void Deselect()
    {
        // Limpar o nome do inimigo selecionado
        thiselectedEnemyName = null;
    }
}
