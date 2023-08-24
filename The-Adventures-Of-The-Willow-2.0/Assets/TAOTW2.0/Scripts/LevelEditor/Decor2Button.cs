using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Decor2Button : MonoBehaviour
{
    public static Decor2Button instance;
    public string selectedDecor2Name;
    private string thiselectedDecor2Name;

    public Image uiImage; // O elemento de UI onde você deseja exibir a imagem
    public Sprite SpriteImageToUI;

    private void Start()
    {
        if (instance == null)
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
        if (DecorButton.instance != null)
        {
            DecorButton.instance.Deselect();
        }

        TileButton.instance.DisableFill();
        TileButton.instance.ClearSelectedTile();
        thiselectedDecor2Name = selectedDecor2Name;

        LevelEditorManager.instance.SelectDecor2(thiselectedDecor2Name);
    }
    private void DeselectAllObjects()
    {
        Decor2Button[] decor2Buttons = FindObjectsOfType<Decor2Button>();
        foreach (Decor2Button decor2Button in decor2Buttons)
        {
            // Desmarcar o botão se for diferente deste
            if (decor2Button != this)
            {
                decor2Button.Deselect();
            }
        }
    }

    public void Deselect()
    {
        thiselectedDecor2Name = null;
    }
}
