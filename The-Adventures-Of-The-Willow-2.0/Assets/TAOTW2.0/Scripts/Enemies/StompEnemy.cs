using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StompEnemy : MonoBehaviour
{
    [HideInInspector] public bool isStomped = false;

    private void Start()
    {
        isStomped = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("PlayerFoots"))
        {
            isStomped = true;
        }
    }
}
