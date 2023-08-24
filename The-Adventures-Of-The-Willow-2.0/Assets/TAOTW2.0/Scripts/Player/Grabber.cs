using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Grabber : MonoBehaviour
{
    public Transform grabDetect;
    public Transform boxHolder;
    public float rayDist;
    private bool Tograb;

    private void Update()
    {
        RaycastHit2D grabCheck = Physics2D.Raycast(grabDetect.position, Vector2.right * transform.localScale, rayDist);
        if(UserInput.instance.playerMoveAndExtraActions.PlayerActions.Grab.IsPressed())
        {
            Tograb = true;
        }
        if(UserInput.instance.playerMoveAndExtraActions.PlayerActions.Grab.WasReleasedThisFrame())
        {
            Tograb = false;
        }
        if (grabCheck.collider != null && grabCheck.collider.gameObject.layer == LayerMask.NameToLayer("Grabable"))
        {
            if (Tograb)
            {
                grabCheck.collider.gameObject.transform.parent = boxHolder;
                grabCheck.collider.gameObject.transform.position = boxHolder.position;
                grabCheck.collider.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
            }
            else
            {
                grabCheck.collider.gameObject.transform.parent = null;
                grabCheck.collider.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
            }
        }

    }
}
