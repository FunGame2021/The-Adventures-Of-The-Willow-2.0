using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;

    [SerializeField] private float toDie = 1f;
    [HideInInspector] public Vector3 playerPosCheck;
    private Vector3 startPlayerPos;
    public bool isDeadNow;
    public bool isDead;
    public bool restartDie;

    private bool isInvulnerable = false; // Flag para controlar se o jogador est� invulner�vel
    [SerializeField] private float invulnerabilityDuration = 3.0f; // Dura��o da invulnerabilidade em segundos
    private float invulnerabilityTimer = 0.0f; // Temporizador para rastrear o tempo de invulnerabilidade


    [Header("PlayerStates")]
    public PlayerStates playerStates;
    public bool isInvincible;

    [Header("Player/hurt anim")]
    [SerializeField] private SpriteRenderer playerRenderer; // Refer�ncia para o componente SpriteRenderer
    [SerializeField] private Color originalColor;
    [SerializeField] private float minAlpha = 0.3f; // Valor m�nimo de alpha
    [SerializeField] private float maxAlpha = 1.0f; // Valor m�ximo de alpha
    [SerializeField] private float duration = 1.0f; // Dura��o total do efeito de piscar

    [SerializeField] private float knockbackForce = -5f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private float knockbackUpForce = 1f;

    public string LastSectorCheck;

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
        //verifica se o gamestate est� em jogo normal ou jogo e editor
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
        // Atualize o temporizador de invulnerabilidade se o jogador estiver invulner�vel
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;

            // Verifique se o per�odo de invulnerabilidade acabou
            if (invulnerabilityTimer <= 0.0f)
            {
                isInvulnerable = false;
            }
        }
    }
    private void changeState()
    {
        Debug.Log($"Current States - FirePower: {playerStates.isFirePower}, AirPower: {playerStates.isAirPower}, Big: {playerStates.isBig}, Small: {playerStates.isSmall}");

        // Verifica se o jogador est� com "Power Up"
        if (playerStates.isFirePower)
        {
            // Inicie o temporizador de invulnerabilidade
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
            // O jogador est� com "Power Up"
            playerStates.SetBigState(true);
            PlayDeathSound();
            // Al�m disso, inicia a corrotina para fazer o jogador piscar
            StartCoroutine(FlashPlayerColor());
        }
        else if (playerStates.isAirPower)
        {
            // Inicie o temporizador de invulnerabilidade
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
            // O jogador est� com "Power Up"
            playerStates.SetBigState(true);
            PlayDeathSound();
            StartCoroutine(FlashPlayerColor());
        }
        else if (playerStates.isBig)
        {
            // Inicie o temporizador de invulnerabilidade
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
            // O jogador est� no estado "Big"
            playerStates.SetSmallState(true);
            PlayDeathSound();
            Debug.Log("isbig");
            StartCoroutine(FlashPlayerColor());
        }
        else if (playerStates.isSmall)
        {
            // O jogador est� no estado "Small"
            //Morre
            TakeDamage();
        }
    }
    private void PlayDeathSound()
    {
        if (!isDeadNow)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Dead, this.transform.position);
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
        if (!isDeadNow) // Garante que o dano seja aplicado apenas uma vez
        {
            isDeadNow = true; // Evita que outros m�todos chamem TakeDamage novamente
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;

            if (PlayerManager.instance != null)
            {
                PlayerManager.instance.IncrementDeaths();
            }

            if (CoinCollect.instance.coin >= 25 && playerPosCheck != null)
            {
                isDead = true;
                CoinCollect.instance.ChangeMinusCoin(25);
                PlayerController.instance.stopPlayer = true;
                CheckDie();
            }
            else
            {
                isDead = true;
                PlayerController.instance.stopPlayer = true;
                Die();
            }
        }
    }


    //Die and restart to checkpoint pos if have coins >= 25 and remove 25 coins, if 0 restart to initial pos

    void CheckDie()
    {
        PlayerController.instance.PlayerDie();
        PlayDeathSound();
        StartCoroutine(RestartCheckpointDie());
    }
    IEnumerator RestartCheckpointDie()
    {
        if (playerPosCheck != null)
        {
            yield return new WaitForSeconds(toDie);
            if (LoadSectorTransition.instance != null)
            {
                if (!string.IsNullOrEmpty(LastSectorCheck))
                {
                    LoadSectorTransition.instance.sectorSimpleCloseTransition(LastSectorCheck, playerPosCheck);
                }
                else
                {
                    LoadSectorTransition.instance.sectorSimpleCloseTransition("Sector1", playerPosCheck);
                }
            }
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
        PlayDeathSound();
        StartCoroutine(RestartDie());
    }
    void Restart()
    {
        if (LoadSectorTransition.instance != null)
        {
            LoadSectorTransition.instance.sectorSimpleCloseTransition("Sector1", startPlayerPos);
        }
        StartCoroutine(RestartValues());
    }
    IEnumerator RestartValues()
    {
        yield return new WaitForSeconds(2.0f);
        LevelTimeManager.instance.RestartTimer();
        PlayerController.instance.stopPlayer = false;
        //playerAnimatiorController.PlayerStart();
        //PlayerController.instance.isDead = false;
        isDeadNow = false;
        isDead = false;
        restartDie = false;
    }

    IEnumerator RestartDie()
    {
        if (!restartDie)
        {
            restartDie = true;
            yield return new WaitForSeconds(toDie);
            Restart();
        }
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
            // Obter a posi��o do centro do inimigo
            Vector2 enemyCenter = collision.transform.position;

            // Determinar em qual lado o jogador colidiu comparando as coordenadas X
            bool knockFromRight = transform.position.x < enemyCenter.x;

            // Chamar o m�todo para aplicar o knockback com base no lado de colis�o
            PlayerController.instance.ApplyKnockBack(knockbackForce, knockbackDuration, knockbackUpForce, collision.transform.position.x < transform.position.x);
            changeState();
        }
        if (collision.CompareTag("EnemyWithStomp") && !isDeadNow && !isInvulnerable && !isInvincible)
        {
            // Obtenha o colisor do inimigo com o qual o jogador colidiu
            Collider2D enemyCollider = collision.GetComponent<Collider2D>();

            // Verifique se o Collider2D do inimigo n�o � null
            if (enemyCollider != null)
            {
                // Obtenha o ponto de colis�o mais pr�ximo do centro do inimigo
                Vector2 collisionPoint = enemyCollider.ClosestPoint(transform.position);

                // Obtenha a posi��o do centro do inimigo
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
            // Obter a posi��o do centro do inimigo
            Vector2 enemyCenter = collision.transform.position;

            // Determinar em qual lado o jogador colidiu comparando as coordenadas X
            bool knockFromRight = transform.position.x < enemyCenter.x;

            // Chamar o m�todo para aplicar o knockback com base no lado de colis�o
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
            // Obter a posi��o do centro do inimigo
            Vector2 enemyCenter = collision.transform.position;

            // Determinar em qual lado o jogador colidiu comparando as coordenadas X
            bool knockFromRight = transform.position.x < enemyCenter.x;

            // Chamar o m�todo para aplicar o knockback com base no lado de colis�o
            PlayerController.instance.ApplyKnockBack(knockbackForce, knockbackDuration, knockbackUpForce, collision.transform.position.x < transform.position.x);

            changeState();
        }
    }
    #endregion

    
}
