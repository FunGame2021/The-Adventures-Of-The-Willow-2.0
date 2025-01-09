using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class MenuCamera : MonoBehaviour
{
    public CinemachineCamera currentCamera;
	
    void Start()
    {
        currentCamera.Priority++;
    }

    public void UpdateCamera(CinemachineCamera target)
	{
		currentCamera.Priority--;
		currentCamera = target;
		currentCamera.Priority++;
	}
}
