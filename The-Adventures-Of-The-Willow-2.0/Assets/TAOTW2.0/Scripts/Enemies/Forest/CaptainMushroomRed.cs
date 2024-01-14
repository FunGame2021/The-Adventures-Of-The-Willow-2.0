using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptainMushroomRed : MonoBehaviour
{
    const string LEFT = "left";
    const string RIGHT = "right";

    [SerializeField]
    Transform castPos;

    [SerializeField]
    Transform castWallPos;

    [SerializeField]
    float edgeCastDist = 1f;
    [SerializeField]
    float wallCastDist = 1f;

    string facingDirection;
    Vector3 baseScale;

    Rigidbody2D rb2d;

    public float moveSpeedTemp = 5f;//same moveSpeed
    public float moveSpeed = 5f;
    public float scaleSpeed = 5f;
    private bool audioPlayed = false;

    private StompEnemy stompEnemy;

    private Animator animator;
    private bool enemyStopped;

    private bool incremented;

    [SerializeField] private LayerMask groundLayer; // Configure no Inspector para incluir a layer do chão.
    [SerializeField] private float overlapRadius = 0.2f; // Raio de sobreposição para verificar colisões.


    [SerializeField] private Transform leftObjectCheck;
    [SerializeField] private Transform rightObjectCheck;
    [SerializeField] private Transform castObjectPos;
    [SerializeField] private LayerMask colliderObjectsLayer; // Configure no Inspector para incluir a layer do chão.

    private void Awake()
    {
        //Stop enemy
        if (GameStates.instance != null)
        {
            if (!GameStates.instance.isLevelStarted)
            {
                moveSpeed = 0f;
                rb2d.bodyType = RigidbodyType2D.Static;
                enemyStopped = true;
            }
        }
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;
        facingDirection = LEFT;
        audioPlayed = false;
        stompEnemy = GetComponentInChildren<StompEnemy>();

        //Stop enemy
        if (GameStates.instance != null)
        {
            if (!GameStates.instance.isLevelStarted)
            {
                moveSpeed = 0f;
                rb2d.bodyType = RigidbodyType2D.Static;
                enemyStopped = true;
            }
        }
    }
    private void Update()
    {
        if (GameStates.instance != null)
        {
            if (!GameStates.instance.isLevelStarted)
            {
                moveSpeed = 0f;
                rb2d.bodyType = RigidbodyType2D.Static;
                enemyStopped = true;
            }
            else
            {
                StartCoroutine(ToStartEnemy());
            }
        }
        // Verificar se o ponto central do jogador está dentro de algum colisor de chão
        if (IsInsideGround() || IsBetweenObjects() && !enemyStopped)
        {
            DestroyGameObject();
            if (PlayerManager.instance != null)
            {
                if (!incremented)
                {
                    PlayerManager.instance.IncrementEnemiesKilled();
                    incremented = true;
                }
            }
        }
    }
    IEnumerator ToStartEnemy()
    {
        yield return new WaitForSeconds(1);
        moveSpeed = moveSpeedTemp;
        rb2d.bodyType = RigidbodyType2D.Dynamic;
        enemyStopped = false;
        rb2d.mass = 10f;
        rb2d.gravityScale = 1f;
    }
    private void StompNow()
    {
        moveSpeed = 0f;
        if (!audioPlayed)
        {
            audioPlayed = true;
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Stomp, this.transform.position);
        }
        if(PlayerManager.instance != null)
        {
            if (!incremented)
            {
                PlayerManager.instance.IncrementEnemiesKilled();
                incremented = true;
            }
        }
    }
    bool IsInsideGround()
    {
        // Verificar se há algum colisor de chão dentro do raio de sobreposição
        Collider2D overlap = Physics2D.OverlapCircle(transform.position, overlapRadius, groundLayer);

        return overlap != null;
    }
    bool IsBetweenObjects()
    {
        // Verificar se está atingindo um objeto à esquerda
        bool hittingLeftObject = Physics2D.Linecast(leftObjectCheck.position, castObjectPos.position, colliderObjectsLayer);

        // Verificar se está atingindo um objeto à direita
        bool hittingRightObject = Physics2D.Linecast(rightObjectCheck.position, castObjectPos.position, colliderObjectsLayer);

        return hittingLeftObject && hittingRightObject;
    }
    public void DestroyGameObject()
    {
        Destroy(this.gameObject);
    }

    private void FixedUpdate()
    {
        if (!enemyStopped)
        {
            rb2d.velocity += Physics2D.gravity * Time.fixedDeltaTime;
            float rigidbodyDrag = Mathf.Clamp01(1.0f - (rb2d.drag * Time.fixedDeltaTime));
            rb2d.velocity *= rigidbodyDrag;

            float vX = moveSpeed * (facingDirection == LEFT ? -1 : 1);

            rb2d.velocity = new Vector2(vX, rb2d.velocity.y);
        }

        if (IsHittingWall() || IsNearEdge() || IsHittingEnemy() || IsHittingBox())
        {
            ChangeFacingDirection(facingDirection == LEFT ? RIGHT : LEFT);
        }

        if (stompEnemy.isStomped)
        {
            animator.SetBool("Die", true);
            StompNow();
        }
    }

    void ChangeFacingDirection(string newDirection)
    {
        facingDirection = newDirection;
        Vector3 newScale = baseScale;
        newScale.x = newDirection == LEFT ? -Mathf.Abs(newScale.x) : Mathf.Abs(newScale.x);

        StartCoroutine(ScaleObjectSmoothly(newScale));

        transform.localScale = newScale;
    }

    IEnumerator ScaleObjectSmoothly(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < scaleSpeed)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / scaleSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

    bool IsHittingWall()
    {
        float castDist = (facingDirection == LEFT) ? -wallCastDist : wallCastDist;
        Vector3 targetPos = castWallPos.position + new Vector3(castDist, 0, 0);
        return Physics2D.Linecast(castWallPos.position, targetPos, 1 << LayerMask.NameToLayer("Ground"));
    }

    bool IsNearEdge()
    {
        float castDist = edgeCastDist;
        Vector3 targetPos = castPos.position - new Vector3(0, castDist, 0);
        return !Physics2D.Linecast(castPos.position, targetPos, 1 << LayerMask.NameToLayer("Ground"));
    }

    bool IsHittingEnemy()
    {
        float castDist = (facingDirection == LEFT) ? -wallCastDist : wallCastDist;
        Vector3 targetPos = castWallPos.position + new Vector3(castDist, 0, 0);
        return Physics2D.Linecast(castWallPos.position, targetPos, 1 << LayerMask.NameToLayer("enemy"));
    }

    bool IsHittingBox()
    {
        float castDist = (facingDirection == LEFT) ? -wallCastDist : wallCastDist;
        Vector3 targetPos = castPos.position + new Vector3(castDist, 0, 0);
        return Physics2D.Linecast(castPos.position, targetPos, 1 << LayerMask.NameToLayer("Grabable"));
    }
    private void OnDrawGizmos()
    {
        float castDist = edgeCastDist;
        Vector3 targetPos = castPos.position - new Vector3(0, castDist, 0);
        Debug.DrawLine(castPos.position, targetPos, Color.red);


        Debug.DrawLine(castPos.position, targetPos, Color.blue);


        // Desenhar uma esfera gizmo para representar a área de sobreposição
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, overlapRadius);

        // Desenhar as linhas de verificação para objetos à esquerda e à direita
        Gizmos.color = Color.red;
        Gizmos.DrawLine(leftObjectCheck.position, castObjectPos.position);
        Gizmos.DrawLine(rightObjectCheck.position, castObjectPos.position);

    }


}
