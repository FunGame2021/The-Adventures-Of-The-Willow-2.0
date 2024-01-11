using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerObject : MonoBehaviour
{
    public Vector2 thisScale;
    public string thisTriggerType; // Armazena o tipo selecionado como uma string
    public string customScript;
    public float timeToPlayTrigger;
    public bool wasTriggerWaitTime;


    private void Update()
    {
        thisScale = transform.localScale;
    }

}
