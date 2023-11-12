using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMapController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;
    private bool facingRight;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (CameraZoom.instance != null)
        {
            CameraZoom.instance.playerRb = rb;
            CameraZoom.instance.playerObject = rb.gameObject;
            // Configura o follow do CinemachineVirtualCamera para seguir o jogador
            CameraZoom.instance._vCam.Follow = CameraZoom.instance.playerObject.transform;
            CameraZoom.instance.AdjustGridColliderSize();
        }
    }

    private void Update()
    {
        // Detecta a entrada do jogador
        horizontalInput = UserInput.instance.moveInput.x;
        verticalInput = UserInput.instance.moveInput.y;

        // Calcula o vetor de movimento com base na entrada do jogador
        Vector2 movement = new Vector2(horizontalInput, verticalInput);

        // Normaliza o vetor de movimento para evitar movimento mais rápido na diagonal
        movement.Normalize();

        // Aplica a força de movimento ao Rigidbody2D
        rb.velocity = movement * moveSpeed;

        if (horizontalInput < 0)
        {
            Turn();
        }
        else if (horizontalInput > 0)
        {
            Turn();
        }
    }
    private void Turn()
    {
        if ((horizontalInput < 0 && !facingRight) || (horizontalInput > 0 && facingRight))
        {
            // Flips the player along the x-axis
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            facingRight = !facingRight;
        }
    }
}
