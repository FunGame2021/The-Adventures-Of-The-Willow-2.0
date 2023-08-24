using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DecorButton : MonoBehaviour
{
    public static DecorButton instance;
    public string selectedDecorName;
    private string thiselectedDecorName;
    public Image uiImage; // O elemento de UI onde você deseja exibir a imagem
    public Sprite SpriteImageToUI;

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        GetComponent<Button>().onClick.AddListener(OnClick);

        // Atribuir o sprite à imagem
        uiImage.sprite = SpriteImageToUI;
    }

    private void OnClick()
    {
        DeselectAllObjects();
        if (EnemyButton.instance != null)
        {
            EnemyButton.instance.Deselect();
        }
        if (ObjectsButton.instance != null)
        {
            ObjectsButton.instance.Deselect();
        }
        if (Decor2Button.instance != null)
        {
            Decor2Button.instance.Deselect();
        }

        TileButton.instance.DisableFill();
        TileButton.instance.ClearSelectedTile();
        thiselectedDecorName = selectedDecorName;

        LevelEditorManager.instance.SelectDecor(thiselectedDecorName);
    }
    private void DeselectAllObjects()
    {
        DecorButton[] decorButtons = FindObjectsOfType<DecorButton>();
        foreach (DecorButton decorButton in decorButtons)
        {
            // Desmarcar o botão se for diferente deste
            if (decorButton != this)
            {
                decorButton.Deselect();
            }
        }
    }

    public void Deselect()
    {
        thiselectedDecorName = null;
    }
}
