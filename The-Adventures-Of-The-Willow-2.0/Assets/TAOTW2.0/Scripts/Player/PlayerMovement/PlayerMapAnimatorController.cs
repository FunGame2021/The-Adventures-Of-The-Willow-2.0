using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMapAnimatorController : MonoBehaviour
{
    [SerializeField] private PlayerMapController _playerMapController;
    private Animator animator;

    private float lastVerticalInput;
    private float lastHorizontalInput;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontalInput = _playerMapController.horizontalInput;
        float verticalInput = _playerMapController.verticalInput;

        // Resetar todas as animações
        animator.SetBool("WalkingTop", false);
        animator.SetBool("WalkingDown", false);
        animator.SetBool("WalkingNormal", false);
        animator.SetBool("WalkingTopLeft", false);
        animator.SetBool("WalkingFrontLeft", false);
        animator.SetBool("IdleTop", false);
        animator.SetBool("IdleDown", false);
        animator.SetBool("IdleNormal", false);
        animator.SetBool("IdleTopLeft", false);
        animator.SetBool("IdleFrontLeft", false);

        if (!Mathf.Approximately(horizontalInput, 0f) || !Mathf.Approximately(verticalInput, 0f))
        {
            // Movimento com prioridade para diagonais
            if (verticalInput > 0f && horizontalInput < 0f)
            {
                animator.SetBool("WalkingTopLeft", true);
                lastVerticalInput = 1f;
                lastHorizontalInput = -1f;
            }
            else if (verticalInput > 0f && horizontalInput > 0f)
            {
                animator.SetBool("WalkingTopLeft", true); // Espelha
                lastVerticalInput = 1f;
                lastHorizontalInput = 1f;
            }
            else if (verticalInput < 0f && horizontalInput < 0f)
            {
                animator.SetBool("WalkingFrontLeft", true);
                lastVerticalInput = -1f;
                lastHorizontalInput = -1f;
            }
            else if (verticalInput < 0f && horizontalInput > 0f)
            {
                animator.SetBool("WalkingFrontLeft", true); // Espelha
                lastVerticalInput = -1f;
                lastHorizontalInput = 1f;
            }
            else if (verticalInput > 0f)
            {
                animator.SetBool("WalkingTop", true);
                lastVerticalInput = 1f;
                lastHorizontalInput = 0f;
            }
            else if (verticalInput < 0f)
            {
                animator.SetBool("WalkingDown", true);
                lastVerticalInput = -1f;
                lastHorizontalInput = 0f;
            }
            else if (!Mathf.Approximately(horizontalInput, 0f))
            {
                animator.SetBool("WalkingNormal", true);
                lastHorizontalInput = Mathf.Sign(horizontalInput);
                lastVerticalInput = 0f;
            }
        }
        else
        {
            // Zera inputs para evitar valores residuais
            lastVerticalInput = Mathf.Round(lastVerticalInput);
            lastHorizontalInput = Mathf.Round(lastHorizontalInput);

            if (lastVerticalInput > 0f && lastHorizontalInput < 0f)
            {
                animator.SetBool("IdleTopLeft", true);
            }
            else if (lastVerticalInput > 0f && lastHorizontalInput > 0f)
            {
                animator.SetBool("IdleTopLeft", true); // Espelha
            }
            else if (lastVerticalInput < 0f && lastHorizontalInput < 0f)
            {
                animator.SetBool("IdleFrontLeft", true);
            }
            else if (lastVerticalInput < 0f && lastHorizontalInput > 0f)
            {
                animator.SetBool("IdleFrontLeft", true); // Espelha
            }
            else if (lastVerticalInput > 0f && Mathf.Approximately(lastHorizontalInput, 0f))
            {
                animator.SetBool("IdleTop", true);
            }
            else if (lastVerticalInput < 0f && Mathf.Approximately(lastHorizontalInput, 0f))
            {
                animator.SetBool("IdleDown", true);
            }
            else if (!Mathf.Approximately(lastHorizontalInput, 0f) && Mathf.Approximately(lastVerticalInput, 0f))
            {
                animator.SetBool("IdleNormal", true);
            }
        }
    }

}
