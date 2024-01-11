using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStates : MonoBehaviour
{
    public static PlayerStates instance;
    public Animator animator;

    [HideInInspector] public bool isBig;
    [HideInInspector] public bool isSmall;
    [HideInInspector] public bool isFirePower;
    [HideInInspector] public bool isAirPower;
    [HideInInspector] public bool isBubblePower;

    public event Action<bool> OnBigStateChanged;
    public event Action<bool> OnSmallStateChanged;
    public event Action<bool> OnFirePowerStateChanged;
    public event Action<bool> OnAirPowerStateChanged;
    public event Action<bool> OnBubblePowerStateChanged;

    [SerializeField] private int bigLayer = 0; // Índice da camada Big
    [SerializeField] private int smallLayer = 1; // Índice da camada Small
    [SerializeField] private int firePowerLayer = 2; // Índice da camada FirePower
    [SerializeField] private int airPowerLayer = 3; // Índice da camada AirPower
    [SerializeField] private int bubblePowerLayer = 4; // Índice da camada AirPower

    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        if(SaveGameManager.instance != null)
        {
            isBig = SaveGameManager.instance.isSaveBig;
            isSmall = SaveGameManager.instance.isSaveSmall;
            isFirePower = SaveGameManager.instance.isSaveFirePower;
            isAirPower = SaveGameManager.instance.isSaveAirPower;
            isBubblePower = SaveGameManager.instance.isSaveBubblePower;
            UpdateAnimatorStates();
        }
    }

    void Update()
    {
        
    }
    public void SetBigState(bool value)
    {
        if (value)
        {
            isBig = true;
            isSmall = false;
            isFirePower = false;
            isAirPower = false;

            OnBigStateChanged?.Invoke(isBig);
            UpdateAnimatorStates();
        }
    }

    public void SetSmallState(bool value)
    {
        if (value)
        {
            isBig = false;
            isSmall = true;
            isFirePower = false;
            isAirPower = false;

            OnSmallStateChanged?.Invoke(isSmall);
            UpdateAnimatorStates();
        }
    }

    public void SetFirePowerState(bool value)
    {
        if (value)
        {
            isBig = false;
            isSmall = false;
            isFirePower = true;
            isAirPower = false;

            OnFirePowerStateChanged?.Invoke(isFirePower);
            UpdateAnimatorStates();
        }
    }

    public void SetAirPowerState(bool value)
    {
        if (value)
        {
            isBig = false;
            isSmall = false;
            isFirePower = false;
            isAirPower = true;

            OnAirPowerStateChanged?.Invoke(isAirPower);
            UpdateAnimatorStates();
        }
    }
    public void SetBubblePowerState(bool value)
    {
        if (value)
        {
            isBig = false;
            isSmall = false;
            isFirePower = false;
            isAirPower = false;
            isBubblePower = true;

            OnBubblePowerStateChanged?.Invoke(isBubblePower);
            UpdateAnimatorStates();
        }
    }

    private void UpdateAnimatorStates()
    {
        animator.SetLayerWeight(bigLayer, isBig ? 1f : 0f);
        animator.SetLayerWeight(smallLayer, isSmall ? 1f : 0f);
        animator.SetLayerWeight(firePowerLayer, isFirePower ? 1f : 0f);
        animator.SetLayerWeight(airPowerLayer, isAirPower ? 1f : 0f);
        animator.SetLayerWeight(bubblePowerLayer, isBubblePower ? 1f : 0f);
    }
}
