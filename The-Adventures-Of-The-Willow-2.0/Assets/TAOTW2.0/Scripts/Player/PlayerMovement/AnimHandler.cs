using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimHandler : MonoBehaviour
{
    private PlayerMovement2D mov;
    private SpriteRenderer spriteRend;
    private Quaternion initialRotation;

    [Header("Movement Tilt")]
    [SerializeField] private float maxTilt;
    [SerializeField] private float maxTiltOpposite;
    [SerializeField][Range(0, 1)] private float tiltSpeed;

    [Header("Dash Tilt")]
    [SerializeField] private float maxDashTilt;
    [SerializeField] private float maxDashTiltOpposite;
    [SerializeField][Range(0, 1)] private float dashTiltSpeed;

    [Header("Particle FX")]
    [SerializeField] private GameObject jumpFX;
    [SerializeField] private GameObject landFX;
    private ParticleSystem _jumpParticle;
    private ParticleSystem _landParticle;

    public bool startedJumping { private get; set; }
    public bool justLanded { private get; set; }

    public float currentVelY;

    private void Start()
    {
        mov = GetComponent<PlayerMovement2D>();
        spriteRend = GetComponentInChildren<SpriteRenderer>();
        _jumpParticle = jumpFX.GetComponent<ParticleSystem>();
        _landParticle = landFX.GetComponent<ParticleSystem>();
        initialRotation = spriteRend.transform.localRotation;
    }

    private void LateUpdate()
    {
        UpdateTilt();

        CheckAnimationState();

        ParticleSystem.MainModule jumpPSettings = _jumpParticle.main;
        ParticleSystem.MainModule landPSettings = _landParticle.main;
    }

    private void UpdateTilt()
    {
        float tiltProgress;

        float targetTilt = (mov.IsFacingRight) ? maxTilt : maxTiltOpposite;

        float targetDashTilt = (mov.IsFacingRight) ? maxDashTilt : maxDashTiltOpposite;

        if (mov.IsSliding)
        {
            tiltProgress = 0.25f;
        }
        else
        {
            if (mov.RB.velocity.x > 0 && !mov.IsDashing)
            {
                tiltProgress = Mathf.InverseLerp(0, mov.Data.runMaxSpeed, mov.RB.velocity.x);
            }
            else
            {
                tiltProgress = Mathf.InverseLerp(0, -mov.Data.runMaxSpeed, mov.RB.velocity.x);
            }

            if (mov.RB.velocity.x > 0 && mov.IsDashing)
            {
                tiltProgress = Mathf.InverseLerp(0, mov.Data.runMaxSpeed, mov.RB.velocity.x);
            }
            else
            {
                tiltProgress = Mathf.InverseLerp(0, -mov.Data.runMaxSpeed, mov.RB.velocity.x);
            }
        }
        if (!mov.IsDashing)
        {
            float newRot = Mathf.LerpAngle(spriteRend.transform.localRotation.eulerAngles.z, targetTilt * tiltProgress, tiltSpeed);
            spriteRend.transform.localRotation = Quaternion.Euler(0, 0, newRot);
        }
        if (mov.IsDashing)
        {
            float newDashRot = Mathf.LerpAngle(spriteRend.transform.localRotation.eulerAngles.z, targetDashTilt * tiltProgress, dashTiltSpeed);
            spriteRend.transform.localRotation = Quaternion.Euler(0, 0, newDashRot);
        }
    }

    private void CheckAnimationState()
    {
        if (startedJumping)
        {
            GameObject obj = Instantiate(jumpFX, transform.position - (Vector3.up * transform.localScale.y / 2), Quaternion.Euler(-90, 0, 0));
            Destroy(obj, 1);
            startedJumping = false;
            return;
        }

        if (justLanded)
        {
            GameObject obj = Instantiate(landFX, transform.position - (Vector3.up * transform.localScale.y / 1.5f), Quaternion.Euler(-90, 0, 0));
            Destroy(obj, 1);
            justLanded = false;
            return;
        }
    }

    public void ResetRotation()
    {
        spriteRend.transform.localRotation = initialRotation;
    }
}
