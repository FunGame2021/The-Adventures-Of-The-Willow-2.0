using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;

    [SerializeField] private float toDie = 1f;
    [HideInInspector] public Vector3 playerPosCheck;
    private Vector3 startPlayerPos;
    private bool isDeadNow;
    public bool isDead;

    private bool isInvulnerable = false; // Flag para controlar se o jogador está invulnerável
    [SerializeField] private float invulnerabilityDuration = 3.0f; // Duração da invulnerabilidade em segundos
    private float invulnerabilityTimer = 0.0f; // Temporizador para rastrear o tempo de invulnerabilidade


    [Header("PlayerStates")]
    public PlayerStates playerStates;
    public bool isInvincible;

    [Header("Player/hurt anim")]
    [SerializeField] private SpriteRenderer playerRenderer; // Referência para o componente SpriteRenderer
    [SerializeField] private Color originalColor;
    [SerializeField] private float minAlpha = 0.3f; // Valor mínimo de alpha
    [SerializeField] private float maxAlpha = 1.0f; // Valor máximo de alpha
    [SerializeField] private float duration = 1.0f; // Duração total do efeito de piscar

    [SerializeField] private float knockbackForce = -5f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private float knockbackUpForce = 1f;

    void Start()
    {
        originalColor = playerRenderer.color;
        playerStates.SetBigState(true);
        if (instance == null)
        {
            instance = this;
        }
        startPlayerPos = this.transform.position;
    }
    private void Update()
    {
        //verifica se o gamestate está em jogo normal ou jogo e editor
        if(GameStates.instance.isNormalGame)
        {
            if (LevelTimeManager.instance.remainingDuration <= 0 && playLevel.instance.StartedLevel)
            {
                TakeDamage();
            }
        }
        else
        {
            if (LevelTimeManager.instance.remainingDuration <= 0 && LoadPlayLevel.instance.StartedLevel)
            {
                TakeDamage();
            }
        }
    }
    private void FixedUpdate()
    {
        // Atualize o temporizador de invulnerabilidade se o jogador estiver invulnerável
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;

            // Verifique se o período de invulnerabilidade acabou
            if (invulnerabilityTimer <= 0.0f)
            {
                isInvulnerable = false;
            }
        }
    }
    private void changeState()
    {
        // Verifica se o jogador está com "Power Up"
        if (playerStates.isFirePower)
        {
            // Inicie o temporizador de invulnerabilidade
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
            // O jogador está com "Power Up"
            playerStates.SetBigState(true);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Dead, this.transform.position);
            // Além disso, inicia a corrotina para fazer o jogador piscar
            StartCoroutine(FlashPlayerColor());
        }
        else if (playerStates.isAirPower)
        {
            // Inicie o temporizador de invulnerabilidade
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
            // O jogador está com "Power Up"
            playerStates.SetBigState(true);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Dead, this.transform.position);
            StartCoroutine(FlashPlayerColor());
        }
        else if (playerStates.isBig)
        {
            // Inicie o temporizador de invulnerabilidade
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
            // O jogador está no estado "Big"
            playerStates.SetSmallState(true);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Dead, this.transform.position);
            StartCoroutine(FlashPlayerColor());
        }
        else if (playerStates.isSmall)
        {
            // O jogador está no estado "Small"
            //Morre
            TakeDamage();
        }
    }

    // Corrotina para fazer o jogador piscar de cor
    private IEnumerator FlashPlayerColor()
    {
        float startTime = Time.time;
        float elapsedTime = 0;


        while (elapsedTime < duration)
        {
            // Escolha aleatoriamente um valor de alpha entre minAlpha e maxAlpha
            float alpha = Random.Range(minAlpha, maxAlpha);

            // Aplique a cor com o alpha calculado
            playerRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            elapsedTime = Time.time - startTime;
            yield return null;
        }

        // Certifique-se de definir a cor de volta para seu valor original quando terminar o efeito
        playerRenderer.color = originalColor;
    }



    public void TakeDamage()
    {
        if (!isDeadNow)
        {
            // Inicie o temporizador de invulnerabilidade
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
            if (PlayerManager.instance != null)
            {
                PlayerManager.instance.IncrementDeaths();
            }
            if (CoinCollect.instance.coin >= 25 && playerPosCheck != null)
            {
                isDead = true;
                isDeadNow = true;
                CoinCollect.instance.ChangeMinusCoin(25);
                PlayerController.instance.stopPlayer = true;
                CheckDie();
            }
            else
            {
                isDead = true;
                isDeadNow = true;
                PlayerController.instance.stopPlayer = true;
                Die();
            }
        }
    }

    //Die and restart to checkpoint pos if have coins >= 25 and remove 25 coins, if 0 restart to initial pos

    void CheckDie()
    {
        PlayerController.instance.PlayerDie();
        AudioManager.instance.PlayOneShot(FMODEvents.instance.Dead, this.transform.position);
        StartCoroutine(RestartCheckpointDie());
    }
    IEnumerator RestartCheckpointDie()
    {
        if (playerPosCheck != null)
        {
            yield return new WaitForSeconds(toDie);
            this.transform.position = playerPosCheck;
            isDeadNow = false;
            PlayerController.instance.stopPlayer = false;
            LevelTimeManager.instance.RestartTimer();
            isDead = false;
        }
        else
        {
            Debug.LogWarning("Checkpoint position is not set!");
        }
    }


    //Die and restart to initial pos
    void Die()
    {
        PlayerController.instance.PlayerDie();
        //PlayerController.instance.isDead = true;
        AudioManager.instance.PlayOneShot(FMODEvents.instance.Dead, this.transform.position);
        StartCoroutine(RestartDie());
    }
    void Restart()
    {
        //playerAnimatiorController.PlayerStart();
        //PlayerController.instance.isDead = false;
        this.transform.position = startPlayerPos;
        isDeadNow = false;
        PlayerController.instance.stopPlayer = false;
        LevelTimeManager.instance.RestartTimer();
        isDead = false;
    }

    IEnumerator RestartDie()
    {
        yield return new WaitForSeconds(toDie);
        Restart();
    }

    #region colliders and triggers

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("immediate_death") && !isDeadNow)
        {
            TakeDamage();
        }
        if (collision.CompareTag("Enemy") && !isDeadNow && !isInvulnerable && !isInvincible)
        {
            // Obter a posição do centro do inimigo
            Vector2 enemyCenter = collision.transform.position;

            // Determinar em qual lado o jogador colidiu comparando as coordenadas X
            bool knockFromRight = transform.position.x < enemyCenter.x;

            // Chamar o método para aplicar o knockback com base no lado de colisão
            PlayerController.instance.ApplyKnockBack(knockbackForce, knockbackDuration, knockbackUpForce, collision.transform.position.x < transform.position.x);
            changeState();
        }
        if (collision.CompareTag("EnemyWithStomp") && !isDeadNow && !isInvulnerable && !isInvincible)
        {
            // Obtenha o colisor do inimigo com o qual o jogador colidiu
            Collider2D enemyCollider = collision.GetComponent<Collider2D>();

            // Verifique se o Collider2D do inimigo não é null
            if (enemyCollider != null)
            {
                // Obtenha o ponto de colisão mais próximo do centro do inimigo
                Vector2 collisionPoint = enemyCollider.ClosestPoint(transform.position);

                // Obtenha a posição do centro do inimigo
                Vector2 enemyCenter = enemyCollider.bounds.center;

                // Obtenha o tamanho do Collider2D do inimigo
                Vector2 enemySize = enemyCollider.bounds.size;

                // Verifique se o jogador colidiu com a borda esquerda do inimigo
                if (collisionPoint.x < enemyCenter.x)
                {
                    changeState();
                }

                // Verifique se o jogador colidiu com a borda direita do inimigo
                else if (collisionPoint.x > enemyCenter.x)
                {
                    changeState();
                }

            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("immediate_death") && !isDeadNow)
        {
            TakeDamage();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("immediate_death") && !isDeadNow)
        {
            TakeDamage();
        }
        if (collision.collider.CompareTag("Enemy") && !isDeadNow && !isInvulnerable && !isInvincible)
        {
            // Obter a posição do centro do inimigo
            Vector2 enemyCenter = collision.transform.position;

            // Determinar em qual lado o jogador colidiu comparando as coordenadas X
            bool knockFromRight = transform.position.x < enemyCenter.x;

            // Chamar o método para aplicar o knockback com base no lado de colisão
            PlayerController.instance.ApplyKnockBack(knockbackForce, knockbackDuration, knockbackUpForce, collision.transform.position.x < transform.position.x);

            changeState();
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("immediate_death") && !isDeadNow)
        {
            TakeDamage();
        }
        if (collision.collider.CompareTag("Enemy") && !isDeadNow && !isInvulnerable && !isInvincible)
        {
            // Obter a posição do centro do inimigo
            Vector2 enemyCenter = collision.transform.position;

            // Determinar em qual lado o jogador colidiu comparando as coordenadas X
            bool knockFromRight = transform.position.x < enemyCenter.x;

            // Chamar o método para aplicar o knockback com base no lado de colisão
            PlayerController.instance.ApplyKnockBack(knockbackForce, knockbackDuration, knockbackUpForce, collision.transform.position.x < transform.position.x);

            changeState();
        }
    }
    #endregion
}
