using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake CameraShakeinstance;

    [SerializeField] private float globalShakeForce = 1f;
    [SerializeField] private CinemachineImpulseListener impulseListener;

    private CinemachineImpulseDefinition impulseDefinition;


    private void Awake() 
    {
        if(CameraShakeinstance == null)
        {
            CameraShakeinstance = this;
        }
    }

    public void camerashake(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }

    public void ScreenShakeFromProfile(ScreenShakeProfile profile, CinemachineImpulseSource impulseSource) 
    {
        //apply settings
        SetupScreenShakeSettings(profile, impulseSource);
        //screenshake
        impulseSource.GenerateImpulseWithForce(profile.impactForce);
    }

    private void SetupScreenShakeSettings(ScreenShakeProfile profile, CinemachineImpulseSource impulseSource)
    {
        impulseDefinition = impulseSource.ImpulseDefinition;

        //change the impulse source settings
        impulseDefinition.ImpulseDuration = profile.impactTime;
        impulseSource.DefaultVelocity = profile.defaultVelocity;
        impulseDefinition.CustomImpulseShape = profile.impulseCurve;

        //change the impulse listener
        impulseListener.ReactionSettings.AmplitudeGain = profile.listenerAmplitude;
        impulseListener.ReactionSettings.FrequencyGain = profile.listenerFrequency;
        impulseListener.ReactionSettings.Duration = profile.listenerDuration;
    }
}
