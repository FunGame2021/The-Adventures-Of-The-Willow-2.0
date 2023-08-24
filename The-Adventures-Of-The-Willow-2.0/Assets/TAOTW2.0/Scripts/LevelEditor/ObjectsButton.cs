using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectsButton : MonoBehaviour
{
    public static ObjectsButton instance;
    public string selectedObjectName;
    private string thiselectedObjectName;

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

        TileButton.instance.DisableFill();
        TileButton.instance.ClearSelectedTile();

        thiselectedObjectName = selectedObjectName;

        if (EnemyButton.instance != null)
        {
            EnemyButton.instance.Deselect();
        }
        if (DecorButton.instance != null)
        {
            DecorButton.instance.Deselect();
        }
        if (Decor2Button.instance != null)
        {
            Decor2Button.instance.Deselect();
        }

        LevelEditorManager.instance.SelectObject(thiselectedObjectName);
    }
    private void DeselectAllObjects()
    {
        ObjectsButton[] objectsButtons = FindObjectsOfType<ObjectsButton>();
        foreach (ObjectsButton objectsButton in objectsButtons)
        {
            if (objectsButton != this)
            {
                objectsButton.Deselect();
            }
        }
    }

    public void Deselect()
    {
        thiselectedObjectName = null;
    }
}
