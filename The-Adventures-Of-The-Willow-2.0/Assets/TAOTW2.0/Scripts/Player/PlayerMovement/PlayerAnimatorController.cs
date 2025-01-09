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
        if (!isIdle && PlayerController.instance.Swimming || PlayerController.instance.isOnWater)
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
        if (FinishPoint.instance.isFinished)
        {
            animationPlayer.SetBool("JumpingV", false);
            animationPlayer.SetBool("FallingV", false);
            animationPlayer.SetBool("JumpingH", false);
            animationPlayer.SetBool("FallingH", false);
            animationPlayer.SetBool("Climbing", false);
            animationPlayer.SetBool("ClimbingIdle", false);
            isIdle = false;
            animationPlayer.SetBool("Walking", true);
        }
        if (!FinishPoint.instance.isFinished)
        {
            //se estiver no ch�o pode andar e parar //n�o est� a nadar
            if (PlayerController.instance.isGrounded && !PlayerController.instance.Swimming && !PlayerController.instance.isOnWater)
            {
                animationPlayer.SetBool("JumpingV", false);
                animationPlayer.SetBool("FallingV", false);
                animationPlayer.SetBool("JumpingH", false);
                animationPlayer.SetBool("FallingH", false);
                animationPlayer.SetBool("Climbing", false);
                animationPlayer.SetBool("ClimbingIdle", false);
                animationPlayer.SetBool("Swim", false);
                animationPlayer.SetBool("IdleSwim", false);

                //andar movimento
                if (PlayerController.instance.RB.linearVelocity.x != 0
                    && PlayerController.instance.moveInput != 0)
                {
                    isIdle = false;
                    animationPlayer.SetBool("Walking", true);
                    animationPlayer.SetBool("Swim", false);
                    animationPlayer.SetBool("IdleSwim", false);
                }
                else
                {
                    animationPlayer.SetBool("Walking", false);
                    isIdle = true;
                    animationPlayer.SetBool("Swim", false);
                    animationPlayer.SetBool("IdleSwim", false);
                }

            }
            else if (PlayerController.instance.Swimming || PlayerController.instance.isOnWater)//est� a nadar
            {
                animationPlayer.SetBool("Climbing", false);
                animationPlayer.SetBool("ClimbingIdle", false);
                animationPlayer.SetBool("JumpingV", false);
                animationPlayer.SetBool("FallingV", false);
                animationPlayer.SetBool("JumpingH", false);
                animationPlayer.SetBool("FallingH", false);
                animationPlayer.SetBool("Walking", false);
                isIdle = false;

                if (PlayerController.instance.moveInput != 0 || PlayerController.instance.moveInputUp != 0)
                {
                    animationPlayer.SetBool("Swim", true);
                    animationPlayer.SetBool("IdleSwim", false);
                }
                else //est� idle na �gua
                {
                    animationPlayer.SetBool("IdleSwim", true);
                    animationPlayer.SetBool("Swim", false);

                }
            }
            else if (!PlayerController.instance.Swimming && !PlayerController.instance.isOnWater) //se n�o estiver no ch�o
            {
                if (!PlayerController.instance.Swimming && !PlayerController.instance.isOnWater)
                {

                    //Se velocidade for igual a 0 salta normal verticar.
                    if (PlayerController.instance.RB.linearVelocity.x == 0 && !PlayerController.instance.isOnLadder)
                    {
                        animationPlayer.SetBool("Walking", false);

                        if (PlayerController.instance.RB.linearVelocity.y > 0)
                        {
                            isIdle = false;
                            animationPlayer.SetBool("JumpingV", true);
                            animationPlayer.SetBool("FallingV", false);
                            animationPlayer.SetBool("JumpingH", false);
                            animationPlayer.SetBool("FallingH", false);
                            animationPlayer.SetBool("Climbing", false);
                            animationPlayer.SetBool("ClimbingIdle", false);
                            animationPlayer.SetBool("IdleSwim", false);
                            animationPlayer.SetBool("Swim", false);
                        }
                        if (PlayerController.instance.RB.linearVelocity.y < 0)
                        {
                            isIdle = false;
                            animationPlayer.SetBool("JumpingV", false);
                            animationPlayer.SetBool("FallingV", true);
                            animationPlayer.SetBool("JumpingH", false);
                            animationPlayer.SetBool("FallingH", false);
                            animationPlayer.SetBool("Climbing", false);
                            animationPlayer.SetBool("ClimbingIdle", false);
                            animationPlayer.SetBool("IdleSwim", false);
                            animationPlayer.SetBool("Swim", false);
                        }
                    }
                    //Salto horizontal
                    else if (PlayerController.instance.RB.linearVelocity.x != 0 && !PlayerController.instance.isOnLadder)
                    {
                        if (PlayerController.instance.RB.linearVelocity.y > 0)
                        {
                            isIdle = false;
                            animationPlayer.SetBool("JumpingV", false);
                            animationPlayer.SetBool("FallingV", false);
                            animationPlayer.SetBool("JumpingH", true);
                            animationPlayer.SetBool("FallingH", false);
                            animationPlayer.SetBool("Climbing", false);
                            animationPlayer.SetBool("ClimbingIdle", false);
                        }
                        if (PlayerController.instance.RB.linearVelocity.y < 0)
                        {
                            isIdle = false;
                            animationPlayer.SetBool("JumpingV", false);
                            animationPlayer.SetBool("FallingV", false);
                            animationPlayer.SetBool("JumpingH", false);
                            animationPlayer.SetBool("FallingH", true);
                            animationPlayer.SetBool("Climbing", false);
                            animationPlayer.SetBool("ClimbingIdle", false);
                        }
                    }

                    //escalando
                    else if (PlayerController.instance.isClimbing)
                    {
                        if (PlayerController.instance.moveInputUp != 0)
                        {
                            isIdle = false;
                            animationPlayer.SetBool("Climbing", true);
                            animationPlayer.SetBool("JumpingV", false);
                            animationPlayer.SetBool("FallingV", false);
                            animationPlayer.SetBool("JumpingH", false);
                            animationPlayer.SetBool("FallingH", false);
                            animationPlayer.SetBool("Walking", false);
                            animationPlayer.SetBool("ClimbingIdle", false);
                        }
                        else
                        {
                            isIdle = false;
                            animationPlayer.SetBool("JumpingV", false);
                            animationPlayer.SetBool("FallingV", false);
                            animationPlayer.SetBool("JumpingH", false);
                            animationPlayer.SetBool("FallingH", false);
                            animationPlayer.SetBool("Walking", false);
                            animationPlayer.SetBool("Climbing", false);
                            animationPlayer.SetBool("ClimbingIdle", true);
                        }
                    }
                    else //n�o est� a escalar
                    {
                        animationPlayer.SetBool("Climbing", false);
                        animationPlayer.SetBool("ClimbingIdle", false);
                    }
                }
            }
        }
    }

    public void PlayDeathAnimation()
    {
        animationPlayer.SetBool("dead", true);
    }
    public void StopDeathAnimation()
    {
        animationPlayer.SetBool("dead", false);
    }
}
