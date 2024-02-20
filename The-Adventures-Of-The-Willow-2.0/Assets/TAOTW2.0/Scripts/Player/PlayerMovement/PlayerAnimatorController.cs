using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    public static PlayerAnimatorController instance;

    [SerializeField] public Animator animationPlayer;

    [SerializeField] private ParticleSystem getPowerUp;

    [SerializeField] private int numberOfIdleAnimations = 3; // Número total de animações Idle disponíveis
    [SerializeField] private string[] idleAnimationTriggers = { "Idle", "Idle2", "Idle3" }; // Triggers das animações Idle


    private bool hasRandomizedIdle = false; // Flag para verificar se já foi feita a randomização da animação Idle
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
        if (!isIdle && PlayerController.instance.Swimming || PlayerController.instance.isOnWater)
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

    void Update()
    {
        // Randomiza uma nova animação Idle se estiver parado e ainda não tiver sido randomizado
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
            //se estiver no chão pode andar e parar //não está a nadar
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
                if (PlayerController.instance.RB.velocity.x != 0
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
            else if (PlayerController.instance.Swimming || PlayerController.instance.isOnWater)//está a nadar
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
                else //está idle na água
                {
                    animationPlayer.SetBool("IdleSwim", true);
                    animationPlayer.SetBool("Swim", false);

                }
            }
            else if (!PlayerController.instance.Swimming && !PlayerController.instance.isOnWater) //se não estiver no chão
            {
                if (!PlayerController.instance.Swimming && !PlayerController.instance.isOnWater)
                {

                    //Se velocidade for igual a 0 salta normal verticar.
                    if (PlayerController.instance.RB.velocity.x == 0 && !PlayerController.instance.isOnLadder)
                    {
                        animationPlayer.SetBool("Walking", false);

                        if (PlayerController.instance.RB.velocity.y > 0)
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
                        if (PlayerController.instance.RB.velocity.y < 0)
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
                    else if (PlayerController.instance.RB.velocity.x != 0 && !PlayerController.instance.isOnLadder)
                    {
                        if (PlayerController.instance.RB.velocity.y > 0)
                        {
                            isIdle = false;
                            animationPlayer.SetBool("JumpingV", false);
                            animationPlayer.SetBool("FallingV", false);
                            animationPlayer.SetBool("JumpingH", true);
                            animationPlayer.SetBool("FallingH", false);
                            animationPlayer.SetBool("Climbing", false);
                            animationPlayer.SetBool("ClimbingIdle", false);
                        }
                        if (PlayerController.instance.RB.velocity.y < 0)
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
                    else //não está a escalar
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
