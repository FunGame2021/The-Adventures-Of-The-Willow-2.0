using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMapController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Detecta a entrada do jogador
        float horizontalInput = UserInput.instance.moveInput.x;
        float verticalInput = UserInput.instance.moveInput.y;

        // Calcula o vetor de movimento com base na entrada do jogador
        Vector2 movement = new Vector2(horizontalInput, verticalInput);

        // Normaliza o vetor de movimento para evitar movimento mais rápido na diagonal
        movement.Normalize();

        // Aplica a força de movimento ao Rigidbody2D
        rb.velocity = movement * moveSpeed;
    }
}
