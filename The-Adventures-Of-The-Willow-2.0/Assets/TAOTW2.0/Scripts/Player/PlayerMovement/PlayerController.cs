using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FMOD.Studio;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    #region Walk
    [Header ("Walk")]
    [HideInInspector] public float moveInput;
    [SerializeField] private float moveSpeed = 3f; 
    [SerializeField] private float speedChangeSmoothTime = 0.2f; // Tempo de interpolação suave para mudança de velocidade
    [SerializeField] private float boostSpeed = 3f;
    private bool speedBoost;

    private float currentMoveSpeed;
    private float targetMoveSpeed;
    private float moveSpeedSmoothVelocity;
    #endregion
    #region Jump
    [Header("Jump")]
    private bool jump;
    [HideInInspector] public bool isGrounded;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 sizeCapsule;
    [SerializeField] private float angleCapsule = -90f;
    [SerializeField] private LayerMask whatIsGround;

    [SerializeField] private float jumpPower = 6f;
    [SerializeField] private float fallMultiplier = 6f;
    [SerializeField] private float jumpMultiplier = 6f;
    Vector2 vecGravity;
    [SerializeField] private float jumpTime = 0.5f;
    [SerializeField] private float normalJumpTime = 0.5f;
    private float jumpTimeCounter;
    private bool jumpBoost;
    [SerializeField] private float boostedJumpTime;

    [SerializeField] private float coyoteTime;
    private float coyoteTimeCounter;

    #endregion

    public Rigidbody2D RB;
    [HideInInspector] public bool facingRight;
    [SerializeField] private float sameGravityPlayer;
    private float normalGravity;
    public bool stopPlayer = false;
    public float autoMoveSpeed = 5f; // Velocidade do movimento automático

    [SerializeField] public PhysicsMaterial2D withFriction2;
    [SerializeField] public PhysicsMaterial2D withFriction;
    [SerializeField] public PhysicsMaterial2D noFriction;

    #region WallJump

    [Header("WallJump")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private float rayDistance = 0.2f;
    public bool isWallSliding;
    [SerializeField] private float wallSlidingSpeed = 2f;
    public bool isWallJumping;
    [SerializeField] private float wallJumpingDirection;
    [SerializeField] private float wallJumpingTime = 0.2f;
    public float wallJumpingCounter;
    [SerializeField] private float wallJumpingDuration = 0.4f;
    [SerializeField] private Vector2 wallJumpingPower = new Vector2(10f, 22f);
    [SerializeField] private Vector2 wallJumpingBoostPower = new Vector2(16f, 26f);
    #endregion

    #region Ladder
    [SerializeField] private float upSpeed;
    [HideInInspector] public float moveInputUp;
    [HideInInspector] public bool isOnLadder;
    [HideInInspector] public bool isClimbing;
    #endregion

    #region Swim
    [SerializeField] private Transform waterCheck;
    [SerializeField] private LayerMask whatIsWater;
    private bool isTouchingWater;

    [HideInInspector] public bool Swimming;
    [HideInInspector] public bool isOnWater;
    private float SwimGravity = 0f;
    private Vector2 swimMoveDirection;
    [SerializeField] private float SwimSpeed;
    [SerializeField] private float RotationSpeed;

    [SerializeField] private float waterJumpForce = 10f;
    private bool canJumpingOutOfWater; 
    [SerializeField] private float SwimBoostSpeed = 6f;
    private float waterExitTime = 0f; // Tempo de saída da água em segundos
    private bool isHeadFirst = false;
    private float rotationSpeed = 5f; // interpolação da rotação
    private float canJumpWaterNow = 0;
    
    [SerializeField] private bool afterWaterOutside;
    [SerializeField] private Transform castOrigin; // O ponto de origem do lançamento da caixa
    [SerializeField] private Vector2 castSize;     // O tamanho da caixa de colisão
    [SerializeField] private LayerMask whatIsHit;  // A camada de objetos que queremos verificar colisões
    private Vector3 originalRotation;
	
    #endregion

    //stomp
    #region enemies
    public float stompForce = 10f; // Ajuste a força do salto conforme necessário.
    #endregion

    #region states
    [SerializeField] private PlayerStates playerstates;
    //small
    [SerializeField] private Collider2D[] SmallColliders;

    //Big
    [SerializeField] private Collider2D[] BigColliders;
    [SerializeField] private CapsuleCollider2D footCollider;
    [SerializeField] private CapsuleCollider2D footCollider2;
    #endregion
    
    #region knockback

    //KnockBack
    public float KnockBack;
    public float KnockBackCount;
    public float KnockBackLength;
    private bool knocked;
    public bool KnockFromRight;
    private float knockbackTimer = 0f;
    #endregion

    #region Dead animation
    private bool isDead = false;
    public float deathAnimationDuration = 2.0f;
    private float deathAnimationTimer = 0.0f;
    #endregion

    #region movingPlatforms
    [SerializeField] private LayerMask whatIsMovingPlatform;
    private bool isOnPlatform;
    private Rigidbody2D platform;
    [SerializeField] private Transform _originalParent; 
    public Transform PlayerTrans;
    public float zRotation = 0f;
    #endregion

    #region GroundTypes
    //Sticky and Icy Platforms
    public bool isOnStickyPlatform;
    public bool isOnIcePlatform;

    public float iceSlideMaxSpeed = 5f; // velocidade máxima quando escorregando
    public float iceSlideAccelAmount = 2.5f; // aceleração quando escorregando
    public float iceSlipFactor = 0.5f;

    public float stickyPlatformSlowdownFactor = 2f;
    #endregion

    //Particles
    public ParticleSystem Bubble;
    private bool WindParticlesPlayNow;
    [SerializeField] private ParticleSystem WindParticles;
    [SerializeField] private ParticleSystem WaterParticles;
    private Vector2 _lastVelocity;

    void Start()
    {
        if (instance == null)
        { 
            instance = this;
        }
        _originalParent = transform.parent;
        // Inicialize a câmera ou realize outras ações, se necessário.
        if (CameraZoom.instance != null)
        {
            CameraZoom.instance.playerRb = RB;
            CameraZoom.instance.playerObject = RB.gameObject;
            // Configura o follow do CinemachineVirtualCamera para seguir o jogador
            CameraZoom.instance._vCam.Follow = CameraZoom.instance.playerObject.transform;
            CameraZoom.instance.AdjustGridColliderSize();
        }
        vecGravity = new Vector2(0, -Physics2D.gravity.y);
        currentMoveSpeed = moveSpeed;
        targetMoveSpeed = moveSpeed;
        normalGravity = sameGravityPlayer;
        originalRotation = transform.eulerAngles;
        stopPlayer = false;

        //Change colliders with player state for big and small
        if (playerstates.isSmall)
        {
            //For ground check
            sizeCapsule = new Vector2(0.37f, 0.14f);

            float newSizeX = 0.33f;

            // Acesse o primeiro CapsuleCollider2D
            footCollider.size = new Vector2(newSizeX, footCollider.size.y);

            float newSize2X = 0.3f;
            // Acesse o segundo CapsuleCollider2D
            footCollider2.size = new Vector2(newSize2X, footCollider2.size.y);

            // Deactivate big colliders
            foreach (Collider2D collider in BigColliders)
            {
                collider.enabled = false;
            }

            // Activate small colliders
            foreach (Collider2D collider in SmallColliders)
            {
                collider.enabled = true;
            }
        }
        else
        {
            //For ground check
            sizeCapsule = new Vector2(0.55f, 0.14f);

            float newSizeX = 0.5945079f;

            // Acesse o primeiro CapsuleCollider2D
            footCollider.size = new Vector2(newSizeX, footCollider.size.y);

            float newSize2X = 0.4789852f;
            // Acesse o segundo CapsuleCollider2D
            footCollider2.size = new Vector2(newSize2X, footCollider2.size.y);

            // Deactivate small colliders
            foreach (Collider2D collider in SmallColliders)
            {
                collider.enabled = false;
            }

            // Activate big colliders
            foreach (Collider2D collider in BigColliders)
            {
                collider.enabled = true;
            }
        }
    }

    void Update()
    {
        if(IsCollidingWithNormalWall())
        {
            AddPlayerFriction2();
        }
        else
        {
            AddPlayerFriction();
        }

        //fix player rotation on platform rotation
        if (!Swimming)
        {
            Vector3 eulers = PlayerTrans.eulerAngles;
            PlayerTrans.eulerAngles = new Vector3(eulers.x, eulers.y, zRotation);
        }

        //Change colliders with player state for big and small
        if (playerstates.isSmall)
        {
            //For ground check
            sizeCapsule = new Vector2(0.37f, 0.14f);

            float newSizeX = 0.33f;

            // Acesse o primeiro CapsuleCollider2D
            footCollider.size = new Vector2(newSizeX, footCollider.size.y);

            float newSize2X = 0.35f;
            // Acesse o segundo CapsuleCollider2D
            footCollider2.size = new Vector2(newSize2X, footCollider2.size.y);

            // Deactivate big colliders
            foreach (Collider2D collider in BigColliders)
            {
                collider.enabled = false;
            }

            // Activate small colliders
            foreach (Collider2D collider in SmallColliders)
            {
                collider.enabled = true;
            }
        }
        else
        {
            //For ground check
            sizeCapsule = new Vector2(0.55f, 0.14f);

            float newSizeX = 0.5945079f;

            // Acesse o primeiro CapsuleCollider2D
            footCollider.size = new Vector2(newSizeX, footCollider.size.y);

            float newSize2X = 0.5929367f;
            // Acesse o segundo CapsuleCollider2D
            footCollider2.size = new Vector2(newSize2X, footCollider2.size.y);
            
            // Deactivate small colliders
            foreach (Collider2D collider in SmallColliders)
            {
                collider.enabled = false;
            }

            // Activate big colliders
            foreach (Collider2D collider in BigColliders)
            {
                collider.enabled = true;
            }
        }

        isGrounded = Physics2D.OverlapCapsule(groundCheck.position, sizeCapsule, CapsuleDirection2D.Horizontal, angleCapsule, whatIsGround);
        isOnPlatform = Physics2D.OverlapCapsule(groundCheck.position, sizeCapsule, CapsuleDirection2D.Horizontal, angleCapsule, whatIsMovingPlatform);
        isTouchingWater = Physics2D.OverlapCircle(waterCheck.position, 0.2f, whatIsWater);
        Collider2D colliders = Physics2D.OverlapCapsule(groundCheck.position, sizeCapsule, CapsuleDirection2D.Horizontal, angleCapsule, whatIsMovingPlatform);
        if (colliders != null)
        {
            if (colliders.tag == "MovingPlatform")
            {
                SetPlayerParent(colliders.transform);
                platform = colliders.gameObject.GetComponent<Rigidbody2D>();
            }
        }
        else
        {
            ResetPlayerParent();
        }
        moveInput = UserInput.instance.moveInput.x;
        moveInputUp = UserInput.instance.moveInput.y;
        // Verifica se o jogador está colidindo com uma parede
        IsCollidingWithWall();
        if (!isWallJumping && !stopPlayer)
        {
            if (moveInput < 0)
            {
                Turn(true); // Vire para a esquerda
            }
            else if (moveInput > 0)
            {
                Turn(false); // Vire para a direita
            }
        }
        
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        //Button was just pushed
        if (UserInput.instance.playerMoveAndExtraActions.PlayerActions.Jump.WasPressedThisFrame())
        {
            if(!Swimming && isGrounded)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.playerJump, this.transform.position);
            }
            if (isGrounded || coyoteTimeCounter > 0 && !Swimming)
            {
                jump = true;
                RB.velocity = new Vector2(RB.velocity.x, jumpPower);
                jumpTimeCounter = 0;
            }
            if (isClimbing && !Swimming)
            {
                jump = true;
                RB.velocity = new Vector2(RB.velocity.x, jumpPower);
                jumpTimeCounter = 0;
                isClimbing = false;
            }
        }
        if (UserInput.instance.playerMoveAndExtraActions.PlayerActions.Jump.WasReleasedThisFrame())
        {
            if (!Swimming)
            {
                jump = false;
                jumpTimeCounter = 0;
                if (RB.velocity.y > 0)
                {
                    RB.velocity = new Vector2(RB.velocity.x, RB.velocity.y * 0.6f);
                }
            }

        }

        if (UserInput.instance.playerMoveAndExtraActions.PlayerActions.Shoot.IsPressed())
        {
            jumpBoost = true;
            speedBoost = true;
            targetMoveSpeed = boostSpeed;
        }
        if (UserInput.instance.playerMoveAndExtraActions.PlayerActions.Shoot.WasReleasedThisFrame())
        {
            jumpBoost = false;
            speedBoost = false;
            targetMoveSpeed = moveSpeed;
        }
        // Interpolação suave da velocidade atual em relação à velocidade alvo
        currentMoveSpeed = Mathf.SmoothDamp(currentMoveSpeed, targetMoveSpeed, ref moveSpeedSmoothVelocity, speedChangeSmoothTime);
        
        WallSlide();
        WallJump();
        
        if(isOnLadder && Mathf.Abs(moveInputUp) > 0)
        {
            isClimbing = true;
        }
        if (Swimming && !stopPlayer)
        {
            

            if (UserInput.instance.playerMoveAndExtraActions.PlayerActions.Jump.IsPressed())
            {
                if (isTouchingWater)
                {
                    swimMoveDirection = new Vector2(moveInput, moveInputUp);
                    float inputMagnitude = Mathf.Clamp01(swimMoveDirection.magnitude);
                    swimMoveDirection.Normalize();
                    transform.Translate(swimMoveDirection * SwimBoostSpeed * inputMagnitude * Time.deltaTime, Space.World);

                    if (swimMoveDirection != Vector2.zero)
                    {
                        Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, swimMoveDirection);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, RotationSpeed * Time.deltaTime);
                    }
                    canJumpingOutOfWater = true;
                }
            }
            else
            {
                swimMoveDirection = new Vector2(moveInput, moveInputUp);
                float inputMagnitude = Mathf.Clamp01(swimMoveDirection.magnitude);
                swimMoveDirection.Normalize();
                transform.Translate(swimMoveDirection * SwimSpeed * inputMagnitude * Time.deltaTime, Space.World);

                if (swimMoveDirection != Vector2.zero)
                {
                    Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, swimMoveDirection);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, RotationSpeed * Time.deltaTime);
                }
                canJumpingOutOfWater = false;
            }
            
        }

        if(!isTouchingWater)
        {
            canJumpingOutOfWater = false;
        }

        // Verifica colisões para cada direção separadamente
        RaycastHit2D groundHit = Physics2D.BoxCast(castOrigin.position, castSize, 0f, Vector2.down, 0.01f, whatIsHit);
        RaycastHit2D ceilingHit = Physics2D.BoxCast(castOrigin.position, castSize, 0f, Vector2.up, 0.01f, whatIsHit);
        RaycastHit2D leftHit = Physics2D.BoxCast(castOrigin.position, castSize, 0f, Vector2.left, 0.01f, whatIsHit);
        RaycastHit2D rightHit = Physics2D.BoxCast(castOrigin.position, castSize, 0f, Vector2.right, 0.01f, whatIsHit);

        if (isHeadFirst && !isGrounded && RB.velocity.y < 0 && !Swimming && !isWallJumping && !isOnLadder && afterWaterOutside)
        {
            
            if (moveInput != 0)
            {
                float targetRotationZ = moveInput < 0 ? -180f : 180f;
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetRotationZ);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            }
            else
            {
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, 180f);
                transform.rotation = targetRotation;

            }

        }
        if ((groundHit.collider != null || ceilingHit.collider != null || leftHit.collider != null || rightHit.collider != null) && afterWaterOutside)
        {
            transform.eulerAngles = originalRotation;
            afterWaterOutside = false;
        }
        if (!afterWaterOutside)
        {
            transform.eulerAngles = originalRotation;
        }

        if (canJumpWaterNow > 0)
        {
            RB.gravityScale = normalGravity;
            canJumpWaterNow -= Time.deltaTime;
        }
        if(canJumpWaterNow <= 0)
        {
            RB.gravityScale = SwimGravity;
            canJumpWaterNow = 0;
        }

        if(waterExitTime > 0)
        {
            isHeadFirst = true;
            waterExitTime -= Time.deltaTime;
        }

        if(waterExitTime <= 0)
        {
            isHeadFirst = false;
            waterExitTime = 0;
        }

        //Death Animation
        if (isDead)
        {
            deathAnimationTimer += Time.deltaTime;

            // Realize a animação de morte aqui, como interpolação da posição do personagem para o centro da tela.

            if (deathAnimationTimer >= deathAnimationDuration)
            {
                // Quando a animação de morte estiver completa
                // Ative uma tela de "Game Over" ou execute outras ações necessárias
                BackToLife();
            }
        }
    }
    private void FixedUpdate()
    {
        if (knockbackTimer > 0)
        {
            // Reduzir o contador de tempo
            knockbackTimer -= Time.fixedDeltaTime;
            knocked = true;
        }
        // Verificar se o knockback terminou
        if (knockbackTimer <= 0)
        {
            knocked = false;
        }

        if (moveInput != 0)
        {
            RemovePlayerFriction();
        }
        else
        {
            AddPlayerFriction();
        }

        #region Jump and walk
        if (!knocked)
        {
            //Walk
            if (!isWallJumping && !Swimming && !stopPlayer && !isDead && !isOnPlatform)
            {
                if (!isOnIcePlatform && !isOnStickyPlatform)
                {
                    RB.velocity = new Vector2(moveInput * currentMoveSpeed, RB.velocity.y);
                }
                else if (isOnIcePlatform)
                {
                    float targetVelocity = Mathf.Clamp(moveInput * iceSlideMaxSpeed, -iceSlideMaxSpeed, iceSlideMaxSpeed);
                    RB.velocity = new Vector2(Mathf.MoveTowards(RB.velocity.x, targetVelocity * 10, iceSlideAccelAmount * Time.deltaTime), RB.velocity.y);

                    // Reduz a velocidade do jogador enquanto escorrega no gelo
                    RB.AddForce(Vector2.left * RB.velocity.x * iceSlipFactor * Time.deltaTime);
                    if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(_lastVelocity.x))
                    {
                        if (facingRight)
                        {
                            Vector3 scale = WaterParticles.transform.localScale;
                            scale.x *= 1;
                            WaterParticles.transform.localScale = scale;
                            WaterParticles.transform.rotation = Quaternion.Euler(0, 178.554f, 38.72f);
                        }
                        else
                        {
                            Vector3 scale = WaterParticles.transform.localScale;
                            scale.x *= 1;
                            WaterParticles.transform.localScale = scale;
                            WaterParticles.transform.rotation = Quaternion.Euler(0, 178.554f, 139.186f);
                        }

                        WaterParticles.Play();
                    }
                }
                else if (isOnStickyPlatform)
                {
                    // Simular aderência na plataforma pegajosa
                    RB.velocity = new Vector2(moveInput * currentMoveSpeed * stickyPlatformSlowdownFactor, RB.velocity.y);
                }
            }
            else
            {
                if(platform != null)
                {
                    //RB.velocity = platform.GetComponent<Rigidbody2D>().velocity;
                    RB.velocity = new Vector2(moveInput * currentMoveSpeed + platform.velocity.x, RB.velocity.y);
                }
                else
                {
                    RB.velocity = new Vector2(moveInput * currentMoveSpeed, RB.velocity.y);
                }
            }
            //FinishPoint
            if (FinishPoint.instance.isFinished && stopPlayer)
            {
                if (FinishPole.instance.isFinishPoleRightEnter)
                {
                    Turn(false); // Vire para a direita
                    RB.velocity = new Vector2(autoMoveSpeed, RB.velocity.y);
                }
                else
                {
                    Turn(true); // Vire para a esquerda
                    RB.velocity = new Vector2(-autoMoveSpeed, RB.velocity.y);
                }
            }
            //Jump
            if (jumpBoost && !stopPlayer && !isDead)
            {
                jumpTime = boostedJumpTime; // Usar duração de salto aumentada
            }
            else
            {
                jumpTime = normalJumpTime;
            }

            if (RB.velocity.y > 0 && jump)
            {
                jumpTimeCounter += Time.deltaTime;
                if (jumpTimeCounter > jumpTime)
                {
                    jump = false;
                }
                float t = jumpTimeCounter / jumpTime;
                float currentJumpM = jumpMultiplier;

                if (t > 0.5f)
                {
                    currentJumpM = jumpMultiplier * (1 - t);
                }
                RB.velocity += vecGravity * currentJumpM * Time.deltaTime;
            }

            if (RB.velocity.y < 0)
            {
                RB.velocity += vecGravity * (fallMultiplier - 1.0f) * Time.deltaTime;
                //RB.velocity -= vecGravity * fallMultiplier * Time.deltaTime;
            }
            #endregion

            if (isClimbing && !Swimming)
            {
                RB.gravityScale = 0;
                RB.velocity = new Vector2(RB.velocity.x, moveInputUp * upSpeed);
            }
            else
            {
                if (!Swimming || !isOnWater)
                {
                    RB.gravityScale = normalGravity;
                }
            }
        }

        

    }

    public void FreezePlayer()
    {
        RB.constraints = RigidbodyConstraints2D.FreezePositionY; 
        RB.freezeRotation = true;
        stopPlayer = true;
        vecGravity = new Vector2(0, -Physics2D.gravity.y);
        RB.gravityScale = 0;
        UserInput.instance.DisableInput();
    }
    public void UnFreezePlayer()
    {
        RB.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        RB.freezeRotation = true;
        stopPlayer = false;
        RB.gravityScale = normalGravity;
        UserInput.instance.EnableInput();
    }
    public void ApplyKnockBack(float knockbackForce, float knockbackDuration, float knockbackUpForce, bool knockFromRight)
    {
        KnockBackCount = knockbackDuration;
        KnockFromRight = knockFromRight;
        // Inicializar o contador de tempo
        knockbackTimer = knockbackDuration;

        // Aplicar a força de knockback horizontal
        Vector2 knockbackDirection = knockFromRight ? Vector2.right : Vector2.left;
        RB.velocity = new Vector2(knockbackDirection.x * knockbackForce, RB.velocity.y);

        // Aplicar a força de knockback vertical
        RB.velocity += Vector2.up * knockbackUpForce;
    }

    public void Turn(bool faceRight)
    {
        if (facingRight != faceRight)
        {
            // Flips the player along the x-axis
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            facingRight = faceRight;
        }
    }

    #region MovingPlatformsParents
    public void SetPlayerParent(Transform newParent)
    {
        transform.parent = newParent;
    }
    public void ResetPlayerParent()
    {
        transform.parent = _originalParent;
    }
    #endregion

    public void AddPlayerFriction2()
    {
        RB.sharedMaterial = withFriction2;
        footCollider.sharedMaterial = withFriction2;
        footCollider2.sharedMaterial = withFriction2;
        foreach (Collider2D collider in BigColliders)
        {
            collider.sharedMaterial = withFriction2;
        }

        foreach (Collider2D collider in SmallColliders)
        {
            collider.sharedMaterial = withFriction2;
        }
    }

    public void AddPlayerFriction()
    {
        RB.sharedMaterial = withFriction;
        footCollider.sharedMaterial = withFriction;
        footCollider2.sharedMaterial = withFriction;
        foreach (Collider2D collider in BigColliders)
        {
            collider.sharedMaterial = withFriction;
        }

        foreach (Collider2D collider in SmallColliders)
        {
            collider.sharedMaterial = withFriction;
        }
    }
    public void RemovePlayerFriction()
    {
        RB.sharedMaterial = noFriction;
        footCollider.sharedMaterial = noFriction;
        footCollider2.sharedMaterial = noFriction;
        foreach (Collider2D collider in BigColliders)
        {
            collider.sharedMaterial = noFriction;
        }

        foreach (Collider2D collider in SmallColliders)
        {
            collider.sharedMaterial = noFriction;
        }
    }

    #region WallJumping
    private void WallSlide()
    {
        if(IsCollidingWithWall() && !isGrounded && moveInput != 0f)
        {
            isWallSliding = true;
            RB.velocity = new Vector2(RB.velocity.x, Mathf.Clamp(RB.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }
    private bool IsCollidingWithWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, rayDistance, whatIsWall);
    }
    private bool IsCollidingWithNormalWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, rayDistance, whatIsGround);
    }
    private void WallJump()
    {
        if (IsCollidingWithWall())
        {
            if (isWallSliding)
            {
                isWallJumping = false;
                wallJumpingDirection = -transform.localScale.x;
                wallJumpingCounter = wallJumpingTime;

                CancelInvoke(nameof(StopWallJumping));
            }
            else
            {
                wallJumpingCounter -= Time.deltaTime;
            }
            if (UserInput.instance.playerMoveAndExtraActions.PlayerActions.Jump.WasPressedThisFrame() && wallJumpingCounter > 0f && !Swimming)
            {
                isWallJumping = true;
                wallJumpingCounter = 0f;

                if (UserInput.instance.playerMoveAndExtraActions.PlayerActions.Shoot.WasPressedThisFrame() && wallJumpingCounter > 0f && !Swimming)
                {
                    RB.velocity = new Vector2(wallJumpingDirection * wallJumpingBoostPower.x, wallJumpingBoostPower.y);
                }
                else
                {
                    RB.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
                }

                if (transform.localScale.x != wallJumpingDirection)
                {
                    // Flips the player along the x-axis
                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                    facingRight = !facingRight;
                }
                Invoke(nameof(StopWallJumping), wallJumpingDuration);
            }
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    public void PlayerDie()
    {
        /*/Quando o personagem morrer, desativa o Rigidbody para evitar que ele seja afetado pela física do jogo.
        Ativa a animação para o personagem está morto.
Alterar os colisores do personagem para Trigger para garantir que ele não colida com outros objetos durante a animação.
Inicie a animação de morte.
Durante a animação de morte,  mover o GameObject do personagem para o centro da tela.
        Usar uma abordagem de interpolação suave, como a função Vector3.Lerp, para animar o movimento.
Quando a animação de morte estiver completa e o personagem estiver no centro da tela, você pode realizar qualquer ação adicional necessária, 
        como exibir uma tela de "Game Over" ou permitir que o jogador reinicie o jogo.
       
        /*/
        if (!isDead)
        {
            isDead = true; 
            PlayerAnimatorController.instance.PlayDeathAnimation();

            // Desativar o Rigidbody para parar a física
            RB.simulated = false;
            RB.velocity = Vector2.zero;

            // Defina todos os colisores como Trigger
            foreach (Collider2D collider in SmallColliders)
            {
                collider.isTrigger = true;
            }
            foreach (Collider2D collider in BigColliders)
            {
                collider.isTrigger = true;
            }
            footCollider.isTrigger = true;
            footCollider2.isTrigger = true;
        }
    }
    public void BackToLife()
    {
        if (isDead)
        {
            isDead = false;
            PlayerAnimatorController.instance.StopDeathAnimation();

            // Reativar o Rigidbody para permitir que o personagem seja afetado pela física novamente
            RB.simulated = true;

            // Defina todos os colisores como Trigger
            foreach (Collider2D collider in SmallColliders)
            {
                collider.isTrigger = false;
            }
            foreach (Collider2D collider in BigColliders)
            {
                collider.isTrigger = false;
            }
            footCollider.isTrigger = false;
            footCollider2.isTrigger = false;
        }
    }


    #endregion

    #region debug
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheck.position, new Vector3(sizeCapsule.x, sizeCapsule.y, 0.1f));

        // Define o tamanho e a posição de origem do raio
        float raycastDistance = 0.5f; // Distância do raio para detectar a colisão com a parede
        Vector2 raycastOrigin = transform.position; // Posição de origem do raio

        // Desenha o raio
        Gizmos.DrawRay(raycastOrigin, transform.right * raycastDistance);

        //Ground check after outside water   
        // Desenha os raycasts adicionais
        Gizmos.color = Color.blue;

        // Raio para baixo
        RaycastHit2D groundHit = Physics2D.BoxCast(castOrigin.position, castSize, 0f, Vector2.down, 0.01f, whatIsHit);
        Gizmos.DrawRay(castOrigin.position, Vector2.down * 0.01f);
        Gizmos.DrawWireCube(castOrigin.position + Vector3.down * 0.01f, new Vector3(castSize.x, 0.01f, castSize.y));

        // Raio para cima
        RaycastHit2D ceilingHit = Physics2D.BoxCast(castOrigin.position, castSize, 0f, Vector2.up, 0.01f, whatIsHit);
        Gizmos.DrawRay(castOrigin.position, Vector2.up * 0.01f);
        Gizmos.DrawWireCube(castOrigin.position + Vector3.up * 0.01f, new Vector3(castSize.x, 0.01f, castSize.y));

        // Raio para a esquerda
        RaycastHit2D leftHit = Physics2D.BoxCast(castOrigin.position, castSize, 0f, Vector2.left, 0.01f, whatIsHit);
        Gizmos.DrawRay(castOrigin.position, Vector2.left * 0.01f);
        Gizmos.DrawWireCube(castOrigin.position + Vector3.left * 0.01f, new Vector3(0.01f, castSize.y, castSize.x));

        // Raio para a direita
        RaycastHit2D rightHit = Physics2D.BoxCast(castOrigin.position, castSize, 0f, Vector2.right, 0.01f, whatIsHit);
        Gizmos.DrawRay(castOrigin.position, Vector2.right * 0.01f);
        Gizmos.DrawWireCube(castOrigin.position + Vector3.right * 0.01f, new Vector3(0.01f, castSize.y, castSize.x));

    }
    #endregion

    #region Triggers
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Ladder"))
        {
            isOnLadder = true;
        }
        if (collision.gameObject.CompareTag("water"))
        {
            afterWaterOutside = true;
            Swimming = true;
            isOnWater = true;
            //evitar ele afundar;
            RB.drag = 20f;
			jump = false;
			// O jogador está na água, então ajuste sua velocidade de queda
            RB.velocity = new Vector2(RB.velocity.x, 0f); // Define a velocidade vertical como zero 		 
			if (canJumpWaterNow <= 0)
            {
                RB.gravityScale = SwimGravity;
            }
           // Bubble.Play();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("water"))
        {
            
            //if (PlayerAbilities.playerAbilitiesInstance.hasSwimming)
            //{
            //    Debug.Log("Water");
            //    //cameraZoom.ZoomOut();
            //    isOnWater = true;
            //    waterTop = false;
            //    Bubble.Play();
            //}
            //cameraZoom.ZoomOut();
            isOnWater = true;
            afterWaterOutside = true;
            //Bubble.Play();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isOnLadder = false;
            isClimbing = false;
        }

        if (collision.gameObject.CompareTag("water"))
        {
            //cameraZoom.ZoomIn();
            isOnWater = false;
            Swimming = false;
            //Bubble.Stop();
            RB.gravityScale = normalGravity;
            RB.drag = 0f;
            if (canJumpingOutOfWater && canJumpWaterNow == 0)
            {
                canJumpWaterNow = 1.5f;
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                canJumpingOutOfWater = false;
                RB.velocity = new Vector2(RB.velocity.x, waterJumpForce);
                waterExitTime = 1f;
            }
        }
    }
    #endregion
    #region Colliders
    private void OnCollisionEnter2D(Collision2D collision)
    {
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "StickyPlatform")
        {
            isOnStickyPlatform = true;
        }
        if (collision.gameObject.tag == "IcePlatform")
        {
            isOnIcePlatform = true;
        }

    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "StickyPlatform")
        {
            isOnStickyPlatform = false;
        }
        if (collision.gameObject.tag == "IcePlatform")
        {
            isOnIcePlatform = false;
        }
    }
    #endregion
}
