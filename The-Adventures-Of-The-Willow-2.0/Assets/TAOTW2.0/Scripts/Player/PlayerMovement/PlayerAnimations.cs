using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    public static PlayerAnimations PlayerAnimationsInstance;

    [HideInInspector]public Animator animationPlayer;

    [SerializeField] private ParticleSystem getPowerUp;

    [SerializeField] private int numberOfIdleAnimations = 3; // Número total de animações Idle disponíveis
    [SerializeField] private string[] idleAnimationTriggers = { "Idle", "Idle2", "Idle3" }; // Triggers das animações Idle


    private bool hasRandomizedIdle = false; // Flag para verificar se já foi feita a randomização da animação Idle
    private bool isIdle;
    public bool isJump;
    public bool isMoving;

    private void Start()
    {
        animationPlayer = GetComponentInChildren<Animator>();
        if(PlayerAnimationsInstance == null)
        {
            PlayerAnimationsInstance = this;
        }
    }

    // Evento chamado no final da animação Idle 1
    public void OnIdle1AnimationEnd()
    {
        hasRandomizedIdle = false;
    }

    // Evento chamado no final da animação Idle 2
    public void OnIdle2AnimationEnd()
    {
        hasRandomizedIdle = false;
    }

    // Evento chamado no final da animação Idle 3
    public void OnIdle3AnimationEnd()
    {
        hasRandomizedIdle = false;
    }

    private void PlayRandomIdleAnimation()
    {
        if (!isIdle)
        {
            return;
        }

        // Escolhe um índice aleatório para o Trigger de animação Idle
        int randomIndex = Random.Range(0, idleAnimationTriggers.Length);

        // Reproduz a animação Idle correspondente ao Trigger aleatório
        string randomTrigger = idleAnimationTriggers[randomIndex];
        animationPlayer.SetTrigger(randomTrigger);

        hasRandomizedIdle = true;
    }

    void FixedUpdate()
    {
        // Randomiza uma nova animação Idle se estiver parado e ainda não tiver sido randomizado
        if (!hasRandomizedIdle)
        {
            PlayRandomIdleAnimation();
        }
        if (PlayerMovement2D.PlayerMovement2Dinstance.RB.velocity.x == 0f && PlayerMovement2D.PlayerMovement2Dinstance.isGrounded
            && PlayerMovement2D.PlayerMovement2Dinstance.RB.velocity.y == 0 && !PlayerMovement2D.PlayerMovement2Dinstance.isClimbing
            && !PlayerMovement2D.PlayerMovement2Dinstance.IsWallJumping && !isJump && !isMoving)
        {
            isIdle = true;
        }
        else
        {
            isIdle = false;
        }

        //walk anim
        if (PlayerMovement2D.PlayerMovement2Dinstance.isGrounded 
            && !PlayerMovement2D.PlayerMovement2Dinstance.IsWallJumping)
        {
            animationPlayer.SetBool("FallingV", false);
            animationPlayer.SetBool("FallingH", false);
            animationPlayer.SetBool("Climbing", false);
            animationPlayer.SetBool("SwimmingMoving", false);

            if (PlayerMovement2D.PlayerMovement2Dinstance.RB.velocity.x != 0 
                && PlayerMovement2D.PlayerMovement2Dinstance._moveInput.x != 0 && !isJump)
            {
                isMoving = true;
                animationPlayer.SetBool("IsWalking", true);
            }
            else
            {
                isMoving = false;
                animationPlayer.SetBool("IsWalking", false);
            }

        }

        else
        {
            //Jump anim

            if (PlayerMovement2D.PlayerMovement2Dinstance.RB.velocity.x == 0)
            {
                Debug.Log("velocity.x 0 no jump");
                animationPlayer.SetBool("IsWalking", false);

                if (PlayerMovement2D.PlayerMovement2Dinstance.RB.velocity.y > 0)
                {
                    Debug.Log("Jump");
                    isJump = true;
                    animationPlayer.SetBool("JumpingV", true);
                    animationPlayer.SetBool("FallingV", false);
                    animationPlayer.SetBool("JumpingH", false);
                    animationPlayer.SetBool("FallingH", false);
                }
                //else if (PlayerMovement2D.PlayerMovement2Dinstance.RB.velocity.y < 0
                //    && !PlayerMovement2D.PlayerMovement2Dinstance.isClimbing
                //    && !PlayerMovement2D.PlayerMovement2Dinstance.IsWallJumping)
                //{
                //    isIdle = false;
                //    animationPlayer.SetBool("JumpingV", false);
                //    animationPlayer.SetBool("FallingV", true);
                //    animationPlayer.SetBool("JumpingH", false);
                //    animationPlayer.SetBool("FallingH", false);
                //}
            }
            else
            {
                if (PlayerMovement2D.PlayerMovement2Dinstance.RB.velocity.y > 0
                    && !PlayerMovement2D.PlayerMovement2Dinstance.isClimbing
                    && !PlayerMovement2D.PlayerMovement2Dinstance.IsWallJumping)
                {
                    isJump = true;
                    animationPlayer.SetBool("JumpingV", false);
                    animationPlayer.SetBool("FallingV", false);
                    animationPlayer.SetBool("JumpingH", true);
                    animationPlayer.SetBool("FallingH", false);
                }
                //else if (PlayerMovement2D.PlayerMovement2Dinstance.RB.velocity.y < 0
                //    && !PlayerMovement2D.PlayerMovement2Dinstance.isClimbing
                //    && !PlayerMovement2D.PlayerMovement2Dinstance.IsWallJumping)
                //{
                //    isIdle = false;
                //    animationPlayer.SetBool("JumpingV", false);
                //    animationPlayer.SetBool("FallingV", false);
                //    animationPlayer.SetBool("JumpingH", false);
                //    animationPlayer.SetBool("FallingH", true);
                //}

            }

            //Wall Jump
            if(PlayerMovement2D.PlayerMovement2Dinstance.IsWallJumping 
                && !PlayerMovement2D.PlayerMovement2Dinstance.isGrounded 
                && !PlayerMovement2D.PlayerMovement2Dinstance.isLadder)
            {
                animationPlayer.SetBool("WallJumping", true);
            }
            else if(PlayerMovement2D.PlayerMovement2Dinstance.IsWallJumping 
                && !PlayerMovement2D.PlayerMovement2Dinstance.isGrounded 
                && !PlayerMovement2D.PlayerMovement2Dinstance.isLadder 
                && PlayerMovement2D.PlayerMovement2Dinstance.RB.velocity.y != 0)
            {
                animationPlayer.SetBool("WallJumping", true);
            }
            else
            {
                animationPlayer.SetBool("WallJumping", false);
            }


            //Clibing anim
            if (PlayerMovement2D.PlayerMovement2Dinstance.isClimbing)
            {
                animationPlayer.SetBool("Climbing", true);
            }
            else
            {
                animationPlayer.SetBool("Climbing", false);
            }

            //swim
            if (PlayerMovement2D.PlayerMovement2Dinstance.isOnWater)
            {
                animationPlayer.SetBool("JumpingV", false);
                animationPlayer.SetBool("FallingV", false);
                animationPlayer.SetBool("JumpingH", false);
                animationPlayer.SetBool("FallingH", false);

                animationPlayer.SetBool("Swimming", true);

                if (PlayerMovement2D.PlayerMovement2Dinstance.RB.velocity.x != 0 && PlayerMovement2D.PlayerMovement2Dinstance._moveInput.x != 0)
                {
                    animationPlayer.SetBool("SwimmingMoving", true);
                }
                else if (PlayerMovement2D.PlayerMovement2Dinstance.RB.velocity.y != 0 && PlayerMovement2D.PlayerMovement2Dinstance._moveInput.y != 0)
                {
                    animationPlayer.SetBool("SwimmingMoving", true);
                }
                else
                {
                    animationPlayer.SetBool("SwimmingMoving", false);
                }
            }
            else
            {
                animationPlayer.SetBool("Swimming", false);
                animationPlayer.SetBool("SwimmingMoving", false);
            }
        }

    }

    public void PlayerStart()
    {
        animationPlayer.SetBool("Dead", false);
    }

    public void PlayerDie()
    {
        isIdle = false;
        animationPlayer.SetBool("Dead", true);
        animationPlayer.SetBool("JumpingV", false);
        animationPlayer.SetBool("FallingV", false);
        animationPlayer.SetBool("JumpingH", false);
        animationPlayer.SetBool("FallingH", false);
        animationPlayer.SetBool("IsWalking", false);
        animationPlayer.SetBool("Swimming", false);
        animationPlayer.SetBool("Climbing", false);
    }

    public void AnimGetSkill()
    {
        animationPlayer.SetTrigger("GetPowerup");
        getPowerUp.Play();
        PlayerMovement2D.PlayerMovement2Dinstance.GetUpSkill(5, 2);
    }
}
