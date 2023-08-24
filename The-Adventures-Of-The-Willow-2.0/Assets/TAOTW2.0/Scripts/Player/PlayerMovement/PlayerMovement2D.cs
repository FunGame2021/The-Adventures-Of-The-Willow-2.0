/*
	Created by @DawnosaurDev at youtube.com/c/DawnosaurStudios
	Thanks so much for checking this out and I hope you find it helpful! 
	If you have any further queries, questions or feedback feel free to reach out on my twitter or leave a comment on youtube :D

	Feel free to use this in your own games, and I'd love to see anything you make!
 */

using FMOD.Studio;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement2D : MonoBehaviour
{
    public static PlayerMovement2D PlayerMovement2Dinstance;
    //Scriptable object which holds all the player's movement parameters. If you don't want to use it
    //just paste in all the parameters, though you will need to manuly change all references in this script
    public PlayerData Data;
    public PlayerAnimations playerAnimations;

    private bool isAirPlayer;
    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
    //Script to handle all player animations, all references can be safely removed if you're importing into your own project.
    public AnimHandler animHandler { get; private set; }
    #endregion

    #region STATE PARAMETERS
    //Variables control the various actions the player can perform at any time.
    //These are fields which can are public allowing for other sctipts to read them
    //but can only be privately written to.
    public bool IsFacingRight { get; private set; }
    public bool facingRight;

    public bool IsJumping { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsSliding { get; private set; }

    //Timers (also all fields, could be private and a method returning a bool could be used)
    public float LastOnGroundTime { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }

    public bool isGrounded;

    //Jump
    private bool _isJumpCut;
    private bool _isJumpFalling;

    //Wall Jump
    private float _wallJumpStartTime;
    private int _lastWallJumpDir;

    //Dash
    private int _dashesLeft;
    private bool _dashRefilling;
    private Vector2 _lastDashDir;
    private bool _isDashAttacking;

    #endregion

    #region INPUT PARAMETERS
    public Vector2 _moveInput;

    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }
    #endregion

    #region CHECK PARAMETERS
    //Set all of these up in the inspector
    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;
    //Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    #region LAYERS & TAGS
    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer;
    #endregion


    [Header("ladder")]
    public float vertical;
    public bool isLadder = false;
    public bool isClimbing;


    //Swimming
    [Header("Swiming System")]
    public bool isOnWater;
    public bool Swimming;
    private Vector2 swimMoveDirection;

    //New input
    private float horizontal;

    //slopes
    [SerializeField] public PhysicsMaterial2D withFriction;
    [SerializeField] public PhysicsMaterial2D noFrictionNormal0;
    private BoxCollider2D BC2D;

    //Particles
    public ParticleSystem Bubble;
	private bool WindParticlesPlayNow;
    [SerializeField] private ParticleSystem WindParticles;
    [SerializeField] private ParticleSystem WaterParticles;
    private Vector2 _lastVelocity;

    [SerializeField] private bool getposition;
    //Pipe
    public bool isOnPipe = false;

    //Platforms
    public Transform _originalParent;
    public Transform PlayerTrans;
    public float zRotation = 0f;

    //Sounds
    private EventInstance playerFootsteps; 
    private bool isPropellerHatDownLoopPlaying = false;
    private bool isPropellerHatUpPlaying = false;
    private bool isPropellerHatDownPlaying = false;


    //Sounds Condition
    private bool isForest;

    //Death
    public bool isDead = false;


    //KnockBack
    public float KnockBack;
    public float KnockBackCount;
    public float KnockBackLength;

    public bool KnockFromRight;

    //Controllers
    //go to principal scene
    public GameObject principalPlayerScene;
    //Open Doors
    public bool openDoor;
    
    //Cutscenes
    [SerializeField] private PlayerInput playerInput;
	
	//Sticky and Icy Platforms
	public bool isOnStickyPlatform;
	public bool isOnIcePlatform;

    //Powers
    private bool isShootPress;
    [HideInInspector] public bool isGrabbingButtonForSkill;
    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        animHandler = GetComponent<AnimHandler>();
        if (PlayerMovement2Dinstance == null)
        {
            PlayerMovement2Dinstance = this;
        }
        playerInput.ActivateInput();
    }
    private void Start()
    {
        isAirPlayer = false;
        IsFacingRight = true;
        BC2D = GetComponent<BoxCollider2D>();
        _originalParent = transform.parent;
        isOnPipe = false;
        //playerFootsteps = AudioManager.instance.CreateInstance(FMODEvents.instance.playerFootsteps);
    }

    public void enemyStompJump()
    {
        RB.velocity = new Vector2(RB.velocity.x, Data.jumpStompEnemy);
        IsJumping = true;
    }

    //New input system Jump
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnJumpInput();
        }
        if (context.canceled)
        {
            OnJumpUpInput();
        }

    }
    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnDashInput();
        }
    }

    public void OpenDoors(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            openDoor = true;
        }
        if(context.canceled)
        {
            openDoor = false;
        }
    }
    public void Shoot(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            isShootPress = true;
        }
        if(context.canceled)
        {
            isShootPress = false;
        }
    }
    public void Grabbing(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            isGrabbingButtonForSkill = true;
        }
        if(context.canceled)
        {
        }
    }

    public void Movement(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
        vertical = context.ReadValue<Vector2>().y;
    }

    private void Update()
    {
		
        _lastVelocity = RB.velocity;
        //Particles
        if (WindParticlesPlayNow)
        {
            WindParticles.Play();
        }
        else
        {
            WindParticles.Stop();
        }

        //fix player rotation on platform rotation
        if (!Swimming)
        {
            Vector3 eulers = PlayerTrans.eulerAngles;
            PlayerTrans.eulerAngles = new Vector3(eulers.x, eulers.y, zRotation);
        }

        #region TIMERS
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;

        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;
        #endregion

        #region INPUT HANDLER
        _moveInput.x = horizontal;
        _moveInput.y = vertical;


        if (_moveInput.x != 0)
            CheckDirectionToFace(_moveInput.x > 0);
        #endregion

        #region COLLISION CHECKS
        if (!IsDashing && !IsJumping)
        {
            //Ground Check
            if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping) //checks if set box overlaps with ground
            {
                if (LastOnGroundTime < -0.1f)
                {
                    animHandler.justLanded = true;
                }
                isGrounded = true;

                LastOnGroundTime = Data.coyoteTime; //if so sets the lastGrounded to coyoteTime
            }
            else
            {
                isGrounded = false;
            }

            //Right Wall Check
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
                    || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
                LastOnWallRightTime = Data.coyoteTime;

            //Right Wall Check
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
                LastOnWallLeftTime = Data.coyoteTime;

            //Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }
        #endregion

        #region JUMP CHECKS
        if (IsJumping && RB.velocity.y < 0)
        {
            IsJumping = false;

            if (!IsWallJumping)
                _isJumpFalling = true;
        }

        if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
        {
            IsWallJumping = false;
        }

        if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
            _isJumpCut = false;

            if (!IsJumping)
                _isJumpFalling = false;
        }

        if (!IsDashing)
        {
            //Jump
            if (CanJump() && LastPressedJumpTime > 0)
            {
                IsJumping = true;
                IsWallJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;
                Jump();

                animHandler.startedJumping = true;
            }
            //WALL JUMP
            else if (CanWallJump() && LastPressedJumpTime > 0 && !isOnWater && !Swimming)
            {
                IsWallJumping = true;
                IsJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;

                _wallJumpStartTime = Time.time;
                _lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

                WallJump(_lastWallJumpDir);
            }
        }
        #endregion

        #region DASH CHECKS
        if (CanDash() && LastPressedDashTime > 0)
        {
            //Freeze game for split second. Adds juiciness and a bit of forgiveness over directional input
            Sleep(Data.dashSleepTime);

            //If not direction pressed, dash forward
            if (_moveInput != Vector2.zero)
                _lastDashDir = _moveInput;
            else
                _lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;



            IsDashing = true;
            IsJumping = false;
            IsWallJumping = false;
            _isJumpCut = false;

            StartCoroutine(nameof(StartDash), _lastDashDir);
        }
        #endregion

        #region SLIDE CHECKS
        if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
            IsSliding = true;
        else
            IsSliding = false;
        #endregion

        #region GRAVITY
        if (!_isDashAttacking && !isOnWater)
        {
            if(isAirPlayer && isShootPress)
            {
                //Higher gravity if we've released the jump input or are falling
                if (IsSliding)
                {
                    SetGravityScale(0);
                }
                else if (RB.velocity.y < 0 && _moveInput.y < 0)
                {
                    //Much higher gravity if holding down
                    SetGravityScale(Data.AirGravityScale * Data.AirFastFallGravityMult);
                    //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
                    RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.AirMaxFastFallSpeed));
                }
                else if (_isJumpCut)
                {
                    //Higher gravity if jump button released
                    SetGravityScale(Data.AirGravityScale * Data.AirJumpCutGravityMult);
                    RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.AirMaxFallSpeed));
                }
                else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.AirJumpHangTimeThreshold)
                {
                    SetGravityScale(Data.AirGravityScale * Data.AirJumpHangGravityMult);
                }
                else if (RB.velocity.y < 0)
                {
                    //Higher gravity if falling
                    SetGravityScale(Data.AirGravityScale * Data.AirFallGravityMult);
                    //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
                    RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.AirMaxFallSpeed));
                }
                else
                {
                    //Default gravity if standing on a platform or moving upwards
                    SetGravityScale(Data.AirGravityScale);
                }
            }
            else
            {
                //Higher gravity if we've released the jump input or are falling
                if (IsSliding)
                {
                    SetGravityScale(0);
                }
                else if (RB.velocity.y < 0 && _moveInput.y < 0)
                {
                    //Much higher gravity if holding down
                    SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
                    //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
                    RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
                }
                else if (_isJumpCut)
                {
                    //Higher gravity if jump button released
                    SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
                    RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
                }
                else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
                {
                    SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
                }
                else if (RB.velocity.y < 0)
                {
                    //Higher gravity if falling
                    SetGravityScale(Data.gravityScale * Data.fallGravityMult);
                    //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
                    RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
                }
                else
                {
                    //Default gravity if standing on a platform or moving upwards
                    SetGravityScale(Data.gravityScale);
                }
            }
           
        }
        else
        {
            if (!isOnWater)
            {
                //No gravity when dashing (returns to normal once initial dashAttack phase over)
                SetGravityScale(0);
            }
        }
        #endregion


        if (Swimming && !isDead)
        {
            swimMoveDirection = new Vector2(_moveInput.x, _moveInput.y);
            float inputMagnitude = Mathf.Clamp01(swimMoveDirection.magnitude);
            swimMoveDirection.Normalize();
            transform.Translate(swimMoveDirection * Data.SwimSpeed * inputMagnitude * Time.deltaTime, Space.World);

            if (swimMoveDirection != Vector2.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, swimMoveDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, Data.RotationSpeed * Time.deltaTime);
            }
        }

        if (_moveInput.x == 0 && isGrounded && !isOnIcePlatform)
        {
            BC2D.sharedMaterial = withFriction;
            RB.sharedMaterial = withFriction;
        }
        else
        {
            BC2D.sharedMaterial = noFrictionNormal0;
            RB.sharedMaterial = noFrictionNormal0;
        }
        if (isLadder && Mathf.Abs(vertical) > 0f)
        {
            isClimbing = true;
        }


        //Animation see PlayerAnimations because climbing not works correctly
        //if (isClimbing)
        //{

        //    playerAnimations.animationPlayer.SetBool("Climbing", true);
        //}
        //else if (isGrounded)
        //{
        //    playerAnimations.animationPlayer.SetBool("Climbing", false);
        //}
        //else
        //{
        //    playerAnimations.animationPlayer.SetBool("Climbing", false);
        //}

        //Sounds
        #region Sounds

        UpdateSound();
        // Verificação do som PropellerHatUpLoop
        if (!isPropellerHatDownLoopPlaying && RB.velocity.y < 0 && isShootPress && !isGrounded && Powers.instance.isAirPlayer)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.PropellerHatDownLoop, transform.position);
            isPropellerHatDownLoopPlaying = true;
        }

        // Verificação do som PropellerHatUp
        if (IsJumping && RB.velocity.y > 0 && !isPropellerHatDownPlaying && isShootPress && Powers.instance.isAirPlayer)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.PropellerHatDown, transform.position);
            isPropellerHatDownPlaying = true;
        }
        if (!IsJumping && isGrounded)
        {
            isPropellerHatUpPlaying = false;
            isPropellerHatDownPlaying = false;
            isPropellerHatDownLoopPlaying = false;
        }
        #endregion

    }

    private void FixedUpdate()
    {
        //Climb
        if (isClimbing && !isDead)
        {
            RB.velocity = new Vector2(RB.velocity.x, _moveInput.y * Data.speedladder);
        }

        //Handle Run
        if (!IsDashing)
        {
            if (IsWallJumping)
                Run(Data.wallJumpRunLerp);
            else
                Run(1);
        }
        else if (_isDashAttacking)
        {
            Run(Data.dashEndRunLerp);
        }

        //Handle Slide
        if (IsSliding)
            Slide();
		
		
        //Handle iceSlide
		if (isOnIcePlatform && Mathf.Abs(_moveInput.normalized.x) < 0.01f)
		{
            // Desacelerar o personagem quando ele não está mais pressionando as teclas de movimento
            RB.velocity = new Vector2(Mathf.MoveTowards(RB.velocity.x, 0f, Data.iceSlideAccelAmount * Time.deltaTime), RB.velocity.y);
		}
    }

    #region INPUT CALLBACKS
    //Methods which whandle input detected in Update()
    public void OnJumpInput()
    {
        if (!isDead)
        {
            isClimbing = false;
            LastPressedJumpTime = Data.jumpInputBufferTime;
            if (Swimming)
            {
				WindParticlesPlayNow = true;
                Data.SwimSpeed = 5f;
                RB.AddForce(transform.rotation * Vector2.up * Data.SwimForce * Data.SwimSpeed, ForceMode2D.Impulse);
                //RB.AddForce(new Vector2(RB.velocity.x, Data.SwimForce), ForceMode2D.Impulse);
            }
            else
            {
                Data.SwimSpeed = 5f;
            }
        }
    }

    public void OnJumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpCut())
            _isJumpCut = true;

        if (Swimming)
        {
            Data.SwimSpeed = 5f;
        }
		WindParticlesPlayNow = false;
    }

    public void OnDashInput()
    {
        if (!isDead && !Swimming && !isOnWater && !isOnPipe)
        {
            LastPressedDashTime = Data.dashInputBufferTime;
            isClimbing = false;
        }
    }
    #endregion

    #region GENERAL METHODS
    public void SetGravityScale(float scale)
    {
        RB.gravityScale = scale;
    }

    private void Sleep(float duration)
    {
        //Method used so we don't need to call StartCoroutine everywhere
        //nameof() notation means we don't need to input a string directly.
        //Removes chance of spelling mistakes and will improve error messages if any
        StartCoroutine(nameof(PerformSleep), duration);
    }

    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration); //Must be Realtime since timeScale with be 0 
        Time.timeScale = 1;
    }
    #endregion

    //MOVEMENT METHODS
    #region RUN METHODS
    private void Run(float lerpAmount)
    {
        if (!Swimming && !isDead && KnockBackCount <= 0)
        {
            float targetSpeed = _moveInput.normalized.x * (isOnIcePlatform ? Data.iceSlideMaxSpeed : Data.runMaxSpeed);

            targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? (isOnIcePlatform ? Data.iceSlideAccelAmount : Data.runAccelAmount) : Data.runDeccelAmount;

            if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
            {
                accelRate *= Data.jumpHangAccelerationMult;
                targetSpeed *= Data.jumpHangMaxSpeedMult;
            }

            if (Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
            {
                accelRate = 0;
            }

            if (isOnStickyPlatform)
            {
                targetSpeed *= Data.stickyPlatformSlowdownFactor;
            }

            if (isOnIcePlatform)
            {
                float targetVelocity = Mathf.Clamp(_moveInput.x * Data.iceSlideMaxSpeed, -Data.iceSlideMaxSpeed, Data.iceSlideMaxSpeed);
                RB.velocity = new Vector2(Mathf.MoveTowards(RB.velocity.x, targetVelocity * 10, Data.iceSlideAccelAmount * Time.deltaTime), RB.velocity.y);
				
                // Reduz a velocidade do jogador enquanto escorrega no gelo
                RB.AddForce(Vector2.left * RB.velocity.x * Data.iceSlipFactor * Time.deltaTime);
				if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(_lastVelocity.x))
                {
					if (IsFacingRight)
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
            else
            {
                float speedDif = targetSpeed - RB.velocity.x;
                float movement = speedDif * accelRate;
                RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
            }
        }
        else if(!Swimming && !isDead && !isOnWater)
        {
            if (KnockFromRight)
            {
                RB.velocity = new Vector2(-KnockBack, KnockBack);
            }
            if (!KnockFromRight)
            {
                RB.velocity = new Vector2(KnockBack, KnockBack);
            }
            KnockBackCount -= Time.deltaTime;
        }
    }

    public void Turn()
    {
        //stores scale and flips the player along the x axis, 
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        facingRight = !facingRight;
        IsFacingRight = !IsFacingRight;
    }
    #endregion

    #region JUMP METHODS
    private void Jump()
    {
        //Audio
        //AudioManager.instance.PlayOneShot(FMODEvents.instance.playerJump, this.transform.position);

        //Ensures we can't call Jump multiple times from one press

        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        #region Perform Jump
        //We increase the force applied if we are falling
        //This means we'll always feel like we jump the same amount 
        //(setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
        if (Powers.instance.isAirPlayer && isShootPress)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.PropellerHatUp, transform.position);
            isPropellerHatUpPlaying = true;

            float force = Data.AirJumpForce;
            if (RB.velocity.y < 0)
                force -= RB.velocity.y;

            RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        }
        else
        {
            float force = Data.jumpForce;
            if (RB.velocity.y < 0)
                force -= RB.velocity.y;

            RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        }
        #endregion
    }


    private void WallJump(int dir)
    {
        //Ensures we can't call Wall Jump multiple times from one press
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        LastOnWallRightTime = 0;
        LastOnWallLeftTime = 0;

        #region Perform Wall Jump
        Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
        force.x *= dir; //apply force in opposite direction of wall

        if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
            force.x -= RB.velocity.x;

        if (RB.velocity.y < 0) //checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
            force.y -= RB.velocity.y;

        //Unlike in the run we want to use the Impulse mode.
        //The default mode will apply are force instantly ignoring masss
        RB.AddForce(force, ForceMode2D.Impulse);

        #endregion
    }
    #endregion

    #region DASH METHODS
    //Dash Coroutine
    private IEnumerator StartDash(Vector2 dir)
    {
        //Overall this method of dashing aims to mimic Celeste, if you're looking for
        // a more physics-based approach try a method similar to that used in the jump
        WindParticlesPlayNow = true;
        LastOnGroundTime = 0;
        LastPressedDashTime = 0;
        if (dir.y != 0)
        {
            // Se houver uma componente y não nula, zere a componente y para permitir apenas dash horizontal
            dir.y = 0;
        }

        float startTime = Time.time;

        _dashesLeft--;
        _isDashAttacking = true;

        SetGravityScale(0);

        //We keep the player's velocity at the dash speed during the "attack" phase (in celeste the first 0.15s)
        while (Time.time - startTime <= Data.dashAttackTime)
        {
            RB.velocity = dir.normalized * Data.dashSpeed;
            //Pauses the loop until the next frame, creating something of a Update loop. 
            //This is a cleaner implementation opposed to multiple timers and this coroutine approach is actually what is used in Celeste :D
            yield return null;
        }

        startTime = Time.time;

        _isDashAttacking = false;

        //Begins the "end" of our dash where we return some control to the player but still limit run acceleration (see Update() and Run())
        SetGravityScale(Data.gravityScale);
        RB.velocity = Data.dashEndSpeed * dir.normalized;

        while (Time.time - startTime <= Data.dashEndTime)
        {
            yield return null;
        }

        //Dash over
        IsDashing = false;
		WindParticlesPlayNow = false;
    }

    //Short period before the player is able to dash again
    private IEnumerator RefillDash(int amount)
    {
        //SHoet cooldown, so we can't constantly dash along the ground, again this is the implementation in Celeste, feel free to change it up
        _dashRefilling = true;
        yield return new WaitForSeconds(Data.dashRefillTime);
        _dashRefilling = false;
        _dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 1);
    }
    #endregion

    #region OTHER MOVEMENT METHODS
    private void Slide()
    {
        //Works the same as the Run but only in the y-axis
        //THis seems to work fine, buit maybe you'll find a better way to implement a slide into this system
        float speedDif = Data.slideSpeed - RB.velocity.y;
        float movement = speedDif * Data.slideAccel;
        //So, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
        //The force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        RB.AddForce(movement * Vector2.up);
    }
    #endregion


    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Turn();
    }

    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }

    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
             (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
    }

    private bool CanJumpCut()
    {
        return IsJumping && RB.velocity.y > 0;
    }

    private bool CanWallJumpCut()
    {
        return IsWallJumping && RB.velocity.y > 0;
    }

    private bool CanDash()
    {
        if (!IsDashing && _dashesLeft < Data.dashAmount && LastOnGroundTime > 0 && !_dashRefilling)
        {
            StartCoroutine(nameof(RefillDash), 1);
        }

        return _dashesLeft > 0;
    }

    public bool CanSlide()
    {
        if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <= 0)
            return true;
        else
            return false;
    }
    #endregion


    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
        Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("water"))
        {
            //Data.SwimSpeed = 5f;
            Swimming = true;
            isOnWater = true;
            RB.drag = 5f;
            SetGravityScale(Data.SwimGravity);
            Bubble.Play();
        }

        if (col.gameObject.CompareTag("Ladder"))
        {
            isLadder = true;
        }

    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("water"))
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
            Bubble.Play();
        }
        
        if (col.gameObject.CompareTag("Ladder"))
        {
            isLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("water"))
        {
            WindParticlesPlayNow = false;
            //cameraZoom.ZoomIn();
            isOnWater = false;
            Swimming = false;
            RB.drag = 0f;
            Bubble.Stop();
	    	SetGravityScale(Data.gravityScale);
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            //animationPlayer.SetBool("Swimming", false);
        }

        if (col.gameObject.CompareTag("Ladder"))
        {
            isLadder = false;
            isClimbing = false;
        }

    }

    public void TrampolineGo()
    {
        RB.velocity = transform.up * Data.TrampolineJumpForce;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {

        if (col.gameObject.tag == "MovingPlatform")
        {
            transform.parent = col.gameObject.transform;
        }

        if (col.gameObject.tag == "EnemyPlatform")
        {
            transform.parent = col.gameObject.transform;
        }


    }
	
    private void OnCollisionStay2D(Collision2D col)
    {

        if (col.gameObject.tag == "StickyPlatform")
        {
			isOnStickyPlatform = true;
        }
        if (col.gameObject.tag == "IcePlatform")
        {
            isOnIcePlatform = true;
        }

    }
    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.tag == "StickyPlatform")
        {
            isOnStickyPlatform = false;
        }
        if (col.gameObject.tag == "IcePlatform")
        {
            isOnIcePlatform = false;
        }
        if (col.gameObject.tag == "MovingPlatform")
        {
            transform.parent = principalPlayerScene.transform;
        }

        if (col.gameObject.tag == "EnemyPlatform")
        {
            transform.parent = principalPlayerScene.transform;
        }

    }

    //Platforms
    public void SetParent(Transform newParent)
    {
        _originalParent = transform.parent;
        transform.parent = newParent;

    }

    public void ResetParent()
    {
        transform.parent = principalPlayerScene.transform;

    }

    private void UpdateSound()
    {
        if (RB.velocity.x != 0 && _moveInput.x != 0 && isGrounded)
        {
            if (isForest)
            {
                PLAYBACK_STATE playbackState;
                playerFootsteps.getPlaybackState(out playbackState);
                if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    playerFootsteps.start();
                }
            }
        }
        else
        {
            playerFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }

    public void CutsceneStateTrue()
    {
        playerInput.DeactivateInput();
    }

    public void CutsceneStateFalse()
    {
        playerInput.ActivateInput();
    }
    public void GetUpSkill(float duration, float height)
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.SkillLevelUpTalk, transform.position);
        StartCoroutine(SmoothLerpUp(duration, height));
    }

    private IEnumerator SmoothLerpUp(float duration, float height)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + new Vector3(0f, height, 0f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
    }

}