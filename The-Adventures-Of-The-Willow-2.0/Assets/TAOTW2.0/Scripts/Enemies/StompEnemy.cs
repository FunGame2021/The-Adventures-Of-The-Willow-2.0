using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StompEnemy : MonoBehaviour
{
    [HideInInspector] public bool isStomped = false;
    [SerializeField] private BoxCollider2D bc2D;
    [SerializeField] private GameObject enemyObjectToChangeTag;

    private void Start()
    {
        isStomped = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerFoots"))
        {
            // Verifica qual é o lado do box collider 2D que foi tocado
            Vector2 normal = bc2D.bounds.center - transform.position;
            // Realiza a ação desejada
            if (normal.y < 0)
            {
                enemyObjectToChangeTag.tag = "Untagged";
                PlayerController.instance.RB.velocity = new Vector2(PlayerController.instance.RB.velocity.x, PlayerController.instance.stompForce);
                // Tocou no lado superior
                isStomped = true;
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerFoots"))
        {
            // Verifica qual é o lado do box collider 2D que foi tocado
            Vector2 normal = bc2D.bounds.center - transform.position;
            // Realiza a ação desejada
            if (normal.y < 0)
            {
                enemyObjectToChangeTag.tag = "Untagged";
                PlayerController.instance.RB.velocity = new Vector2(PlayerController.instance.RB.velocity.x, PlayerController.instance.stompForce);
                // Tocou no lado superior
                isStomped = true;
            }
        }
    }
}
