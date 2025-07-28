using UnityEngine;
using System.Collections;

public class PlayerAnimatorController : MonoBehaviour
{
    public static PlayerAnimatorController instance;

    [Header("Animation References")]
    [SerializeField] private Animator animationPlayer;
    [SerializeField] private ParticleSystem getPowerUp;

    [Header("Idle Animations")]
    [SerializeField] private int numberOfIdleAnimations = 3;
    [SerializeField] private string[] idleAnimationTriggers = { "Idle", "Idle2", "Idle3" };

    private bool hasRandomizedIdle = false;
    private bool isIdle;
    private bool wasMovingBeforeJump = false;
    private float movementThreshold = 0.1f;
    private float inputThreshold = 0.2f;

    [Header("Fall Detection")]
    [SerializeField] private float fallSpeedThreshold = -1f; // Ajustado para melhor detecção
    [SerializeField] private float groundCheckDistance = 0.5f; // Distância reduzida
    [SerializeField] private LayerMask groundLayer;
    private bool wasFalling = false;
    private bool isNearGround = false;
    private bool triggerLock = false; // Evita múltiplos triggers

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void SetFinishState(bool isFinishing)
    {
        if (isFinishing)
        {
            ResetAllAnimations();
            animationPlayer.SetBool("Walking", true);
        }
        else
        {
            ResetAllAnimations();
            animationPlayer.SetBool("Idle", true);
        }
    }

    private void ResetAllAnimations()
    {
        animationPlayer.SetBool("Walking", false);
        animationPlayer.SetBool("JumpingV", false);
        animationPlayer.SetBool("FallingV", false);
        animationPlayer.SetBool("JumpingH", false);
        animationPlayer.SetBool("FallingH", false);
        animationPlayer.SetBool("Climbing", false);
        animationPlayer.SetBool("ClimbingIdle", false);
        animationPlayer.SetBool("Swim", false);
        animationPlayer.SetBool("IdleSwim", false);
    }
    // Adicionar no OnDrawGizmos para visualização (opcional)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
    void Update()
    {
        if (PlayerController.instance.isFinishing) return;

        // Atualiza o estado de movimento antes do salto
        if (PlayerController.instance.isGrounded)
        {
            wasMovingBeforeJump = Mathf.Abs(PlayerController.instance.RB.linearVelocity.x) > movementThreshold ||
                                Mathf.Abs(PlayerController.instance.moveInput) > inputThreshold;
        }

        UpdateMovementAnimations();

        // Só verifica animações idle se não estiver em movimento
        if (PlayerController.instance.isGrounded &&
           !PlayerController.instance.Swimming &&
           !PlayerController.instance.isOnWater &&
           Mathf.Abs(PlayerController.instance.RB.linearVelocity.x) < movementThreshold &&
           Mathf.Abs(PlayerController.instance.moveInput) < inputThreshold)
        {
            HandleIdleAnimations();
        }
    }

    private void HandleIdleAnimations()
    {
        if (!hasRandomizedIdle)
        {
            PlayRandomIdleAnimation();
        }
    }

    private void PlayRandomIdleAnimation()
    {
        int randomIndex = Random.Range(0, idleAnimationTriggers.Length);
        animationPlayer.SetTrigger(idleAnimationTriggers[randomIndex]);
        hasRandomizedIdle = true;
    }

    private void UpdateMovementAnimations()
    {
        if (PlayerController.instance.stopPlayer) return;

        bool isGrounded = PlayerController.instance.isGrounded;
        bool isSwimming = PlayerController.instance.Swimming || PlayerController.instance.isOnWater;
        bool isClimbing = PlayerController.instance.isClimbing;

        ResetAllAnimations();

        if (isSwimming)
        {
            UpdateSwimAnimations();
        }
        else if (isClimbing)
        {
            UpdateClimbAnimations();
        }
        else if (isGrounded)
        {
            UpdateGroundAnimations();
        }
        else
        {
            UpdateAirAnimations();
        }
    }

    private void UpdateSwimAnimations()
    {
        bool isMoving = PlayerController.instance.moveInput != 0 || PlayerController.instance.moveInputUp != 0;
        animationPlayer.SetBool(isMoving ? "Swim" : "IdleSwim", true);
    }

    private void UpdateClimbAnimations()
    {
        bool isClimbing = PlayerController.instance.moveInputUp != 0;
        animationPlayer.SetBool(isClimbing ? "Climbing" : "ClimbingIdle", true);
    }

    private void UpdateGroundAnimations()
    {
        bool isMoving = Mathf.Abs(PlayerController.instance.RB.linearVelocity.x) > movementThreshold &&
                       Mathf.Abs(PlayerController.instance.moveInput) > inputThreshold;

        animationPlayer.SetBool(isMoving ? "Walking" : "Idle", true);
        isIdle = !isMoving;
    }

    private void UpdateAirAnimations()
    {
        float velocityY = PlayerController.instance.RB.linearVelocity.y;
        bool isMovingHorizontally = Mathf.Abs(PlayerController.instance.RB.linearVelocity.x) > movementThreshold ||
                                  Mathf.Abs(PlayerController.instance.moveInput) > inputThreshold;

        // Verificação mais precisa do chão
        isNearGround = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        bool isFalling = velocityY < fallSpeedThreshold;

        // Resetar estados
        animationPlayer.SetBool("JumpingV", false);
        animationPlayer.SetBool("FallingV", false);
        animationPlayer.SetBool("JumpingH", false);
        animationPlayer.SetBool("FallingH", false);

        if (isFalling)
        {
            if (isMovingHorizontally)
            {
                animationPlayer.SetBool("FallingH", true);

                // Trigger quando está perto do chão e não estava antes
                if (isNearGround && !wasFalling && !triggerLock)
                {
                    animationPlayer.SetTrigger("FallingHEnd");
                    triggerLock = true;
                    StartCoroutine(ResetTriggerLock());
                }
            }
            else
            {
                animationPlayer.SetBool("FallingV", true);

                if (isNearGround && !wasFalling && !triggerLock)
                {
                    animationPlayer.SetTrigger("FallingVEnd");
                    triggerLock = true;
                    StartCoroutine(ResetTriggerLock());
                }
            }
            wasFalling = true;
        }
        else
        {
            // Estado de subida
            if (isMovingHorizontally)
            {
                animationPlayer.SetBool("JumpingH", true);
            }
            else
            {
                animationPlayer.SetBool("JumpingV", true);
            }
            wasFalling = false;
            triggerLock = false;
        }
    }

    private IEnumerator ResetTriggerLock()
    {
        yield return new WaitForSeconds(0.5f);
        triggerLock = false;
    }
    public void OnIdleAnimationEnd() => hasRandomizedIdle = false;

    public void PlayDeathAnimation()
    {
        ResetAllAnimations();
        animationPlayer.SetBool("dead", true);
    }

    public void StopDeathAnimation() => animationPlayer.SetBool("dead", false);
}