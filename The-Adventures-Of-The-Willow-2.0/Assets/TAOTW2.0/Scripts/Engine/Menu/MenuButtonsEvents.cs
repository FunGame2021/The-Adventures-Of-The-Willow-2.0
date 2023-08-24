using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class MenuButtonsEvents : MonoBehaviour
{
    public void buttonClickAudioPlay()
    {
        AudioManager.instance.PlayOneShotNo3D(FMODMenuEvents.instance.buttonClick);
    }

    public void buttonSelectAudioPlay()
    {
        AudioManager.instance.PlayOneShotNo3D(FMODMenuEvents.instance.buttonSelect);
    }
}
