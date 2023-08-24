using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    public static PlayerAnimatorController instance;

    [SerializeField] public Animator animationPlayer;

    [SerializeField] private ParticleSystem getPowerUp;

    [SerializeField] private int numberOfIdleAnimations = 3; // N�mero total de anima��es Idle dispon�veis
    [SerializeField] private string[] idleAnimationTriggers = { "Idle", "Idle2", "Idle3" }; // Triggers das anima��es Idle


    private bool hasRandomizedIdle = false; // Flag para verificar se j� foi feita a randomiza��o da anima��o Idle
    private bool isIdle;
    public bool isJump;
    public bool isMoving;

    [SerializeField] private float DeadJumpForce = 1f;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    // Evento chamado no final da anima��o Idle 1
    public void OnIdle1AnimationEnd()
    {
        hasRandomizedIdle = false;
    }

    // Evento chamado no final da anima��o Idle 2
    public void OnIdle2AnimationEnd()
    {
        hasRandomizedIdle = false;
    }

    // Evento chamado no final da anima��o Idle 3
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

        // Escolhe um �ndice aleat�rio para o Trigger de anima��o Idle
        int randomIndex = Random.Range(0, idleAnimationTriggers.Length);

        // Reproduz a anima��o Idle correspondente ao Trigger aleat�rio
        string randomTrigger = idleAnimationTriggers[randomIndex];
        animationPlayer.SetTrigger(randomTrigger);

        hasRandomizedIdle = true;
    }

    void Update()
    {
        // Randomiza uma nova anima��o Idle se estiver parado e ainda n�o tiver sido randomizado
        if (!hasRandomizedIdle)
        {
            PlayRandomIdleAnimation();
        }
        //se estiver no ch�o pode andar e parar
        if (PlayerController.instance.isGrounded)
        {
            animationPlayer.SetBool("JumpingV", false);
            animationPlayer.SetBool("FallingV", false);
            animationPlayer.SetBool("JumpingH", false);
            animationPlayer.SetBool("FallingH", false);

            //andar movimento
            if (PlayerController.instance.RB.velocity.x != 0
                && PlayerController.instance.moveInput != 0)
            {
                isIdle = false;
                animationPlayer.SetBool("Walking", true);
            }
            else
            {
                animationPlayer.SetBool("Walking", false);
                isIdle = true;
            }
        }
        else //se n�o estiver no ch�o vai saltar
        {
            //Se velocidade for igual a 0 salta normal verticar.
            if (PlayerController.instance.RB.velocity.x == 0)
            {
                animationPlayer.SetBool("Walking", false);

                if (PlayerController.instance.RB.velocity.y > 0)
                {
                    isIdle = false;
                    animationPlayer.SetBool("JumpingV", true);
                    animationPlayer.SetBool("FallingV", false);
                    animationPlayer.SetBool("JumpingH", false);
                    animationPlayer.SetBool("FallingH", false);
                }
                if (PlayerController.instance.RB.velocity.y < 0)
                {
                    isIdle = false;
                    animationPlayer.SetBool("JumpingV", false);
                    animationPlayer.SetBool("FallingV", true);
                    animationPlayer.SetBool("JumpingH", false);
                    animationPlayer.SetBool("FallingH", false);
                }
            }

            else //Else velocidade for diferente a 0 salta na horizontal.
            {
                if (PlayerController.instance.RB.velocity.y > 0)
                {
                    isIdle = false;
                    animationPlayer.SetBool("JumpingV", false);
                    animationPlayer.SetBool("FallingV", false);
                    animationPlayer.SetBool("JumpingH", true);
                    animationPlayer.SetBool("FallingH", false);
                }
                if (PlayerController.instance.RB.velocity.y < 0)
                {
                    isIdle = false;
                    animationPlayer.SetBool("JumpingV", false);
                    animationPlayer.SetBool("FallingV", false);
                    animationPlayer.SetBool("JumpingH", false);
                    animationPlayer.SetBool("FallingH", true);
                }
            }
        }
    }

    public void PlayerDie()
    {
        // Aplica uma for�a de salto ao corpo do personagem
        PlayerController.instance.RB.velocity = new Vector2(PlayerController.instance.RB.velocity.x, DeadJumpForce);
    }

}
