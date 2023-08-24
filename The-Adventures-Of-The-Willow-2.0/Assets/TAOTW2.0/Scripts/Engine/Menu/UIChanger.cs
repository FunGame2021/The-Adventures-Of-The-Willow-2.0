using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChanger : MonoBehaviour
{
    public Image[] buttonImages;
    public Sprite defaultSprite;
    public Sprite xmasSprite;
    public Sprite halloweenSprite;

    void Update()
    {
        if (RemoteConfig.Instance.IsChristmas)
        {
            foreach (Image buttonImage in buttonImages)
            {
                buttonImage.sprite = xmasSprite;
            }
        }
        else if(RemoteConfig.Instance.IsHalloween)
        {
            foreach (Image buttonImage in buttonImages)
            {
                buttonImage.sprite = halloweenSprite;
            }
        }
        else
        {
            foreach (Image buttonImage in buttonImages)
            {
                buttonImage.sprite = defaultSprite;
            }
        }
    }
    
}

