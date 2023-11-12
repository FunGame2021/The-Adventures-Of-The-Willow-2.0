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

        // Resetando todas as animações
        animator.SetBool("WalkingTop", false);
        animator.SetBool("WalkingDown", false);
        animator.SetBool("WalkingNormal", false);
        animator.SetBool("IdleNormal", false);
        animator.SetBool("IdleTop", false);
        animator.SetBool("IdleDown", false);

        if (verticalInput > 0f)
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
        else if (horizontalInput != 0f)
        {
            animator.SetBool("WalkingNormal", true);
            lastHorizontalInput = Mathf.Sign(horizontalInput);
            lastVerticalInput = 0f;
        }
        else
        {
            // Se não houver entrada, configura a animação de idle com base na última direção
            if (lastVerticalInput > 0f && lastHorizontalInput == 0f)
            {
                animator.SetBool("IdleTop", true);
                animator.SetBool("IdleDown", false);
                animator.SetBool("IdleNormal", false);
            }
            else if (lastVerticalInput < 0f && lastHorizontalInput == 0f)
            {
                animator.SetBool("IdleDown", true);
                animator.SetBool("IdleTop", false);
                animator.SetBool("IdleNormal", false);
            }
            else if (lastHorizontalInput != 0f && lastVerticalInput == 0f)
            {
                animator.SetBool("IdleNormal", true);
                animator.SetBool("IdleTop", false);
                animator.SetBool("IdleDown", false);
            }
        }
    }
}
