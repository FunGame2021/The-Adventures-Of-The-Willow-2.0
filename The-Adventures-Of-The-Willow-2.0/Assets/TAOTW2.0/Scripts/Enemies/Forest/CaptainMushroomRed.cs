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

    private void Awake()
    {
        //Stop enemy
        if(GameStates.instance != null)
        {
            if(!GameStates.instance.isLevelStarted)
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
                moveSpeed = moveSpeedTemp;
                rb2d.bodyType = RigidbodyType2D.Dynamic;
                enemyStopped = false;
            }
        }
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
            PlayerManager.instance.IncrementEnemiesKilled();
        }
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
    }

    
}
