using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powers : MonoBehaviour
{
	public static Powers instance;

    public bool isNormal;
	public bool isFirePlayer;
	public bool isBubblePlayer;
	public bool isAirPlayer;
	
	public bool isPowered;
	
	
	public Animator PlayerAnimator;

	[SerializeField] private PlayerData playerData;
    void Awake()
    {
		if(instance == null)
		{
			instance = this;
		}
        if (playerData != null)
        {
            playerData.powersInstance = this;
        }
    }
     
    void Update()
    {
        if(isNormal && !isFirePlayer && !isBubblePlayer && !isAirPlayer)
		{
		    PlayerAnimator.SetLayerWeight(0, 1);
            PlayerAnimator.SetLayerWeight(1, 0);
            PlayerAnimator.SetLayerWeight(2, 0);
            PlayerAnimator.SetLayerWeight(3, 0);
		}
		
		if(!isNormal && isFirePlayer && !isBubblePlayer && !isAirPlayer)
		{
		    PlayerAnimator.SetLayerWeight(0, 0);
            PlayerAnimator.SetLayerWeight(1, 1);
            PlayerAnimator.SetLayerWeight(2, 0);
            PlayerAnimator.SetLayerWeight(3, 0);
		}
		
		if(!isNormal && !isFirePlayer && isBubblePlayer && !isAirPlayer)
		{
		    PlayerAnimator.SetLayerWeight(0, 0);
            PlayerAnimator.SetLayerWeight(1, 0);
            PlayerAnimator.SetLayerWeight(2, 1);
            PlayerAnimator.SetLayerWeight(3, 0);
		}
		
		if(!isNormal && !isFirePlayer && !isBubblePlayer && isAirPlayer)
		{
		    PlayerAnimator.SetLayerWeight(0, 0);
            PlayerAnimator.SetLayerWeight(1, 0);
            PlayerAnimator.SetLayerWeight(2, 0);
            PlayerAnimator.SetLayerWeight(3, 1);
		}
    }
	
	public void isNormalState()
	{
		isNormal = true;
		isFirePlayer = false;
		isBubblePlayer = false;
		isAirPlayer = false;
		isPowered = false;
	}
	
	public void isFirePlayerState()
	{
		isNormal = false;
		isFirePlayer = true;
		isBubblePlayer = false;
		isAirPlayer = false;
		isPowered = true;
	}
	
	public void isBubblePlayerState()
	{
		isNormal = false;
		isFirePlayer = false;
		isBubblePlayer = true;
		isAirPlayer = false;
		isPowered = true;
	}
	
	public void isAirPlayerState()
	{
		isNormal = false;
		isFirePlayer = false;
		isBubblePlayer = false;
		isAirPlayer = true;
		isPowered = true;
	}

}
