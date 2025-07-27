using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPoint : MonoBehaviour
{
    public static FinishPoint instance;

    [SerializeField] private float jumpForce = 10f;


    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

}
