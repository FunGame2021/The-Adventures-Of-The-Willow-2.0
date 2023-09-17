using UnityEngine;
using UnityEngine.InputSystem;

public class Grabber : MonoBehaviour
{
    [SerializeField] private Transform grabDetect;
    [SerializeField] private Transform boxHolder;
    [SerializeField] private float rayDist;
    [SerializeField] private LayerMask raycastLayerMask;

    private GameObject currentHeldObject; // Variável para rastrear a caixa atualmente segurada
    private bool isGrabbing; // Variável para rastrear se o jogador está atualmente segurando uma caixa

    private void FixedUpdate()
    {
        RaycastHit2D grabCheck = Physics2D.Raycast(grabDetect.position, Vector2.right * transform.localScale, rayDist, raycastLayerMask);

        if (grabCheck.collider != null)
        {
            bool isGrabPressed = UserInput.instance.playerMoveAndExtraActions.PlayerActions.Grab.IsPressed();

            if (isGrabPressed && !isGrabbing && !PlayerHealth.instance.isDead)
            {
                HandleGrab(grabCheck.collider.gameObject);
            }
            else if (!isGrabPressed && isGrabbing)
            {
                HandleRelease();
            }
        }

        if (PlayerHealth.instance != null)
        {
            if (PlayerHealth.instance.isDead)
            {
                HandleRelease();
            }
        }
    }

    private void HandleGrab(GameObject grabbedObject)
    {
        currentHeldObject = grabbedObject;
        currentHeldObject.transform.parent = boxHolder;
        currentHeldObject.transform.position = boxHolder.position;
        Rigidbody2D rb = currentHeldObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Defina a variável isBeingGrabbed como verdadeira na caixa
        RestartObjects restartObjects = currentHeldObject.GetComponent<RestartObjects>();
        if (restartObjects != null)
        {
            restartObjects.isBeingGrabbed = true;
        }

        isGrabbing = true; // Marque que o jogador está segurando uma caixa
    }

    private void HandleRelease()
    {
        if (currentHeldObject != null)
        {
            currentHeldObject.transform.parent = null;
            Rigidbody2D rb = currentHeldObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            // Defina a variável isBeingGrabbed como verdadeira na caixa
            RestartObjects restartObjects = currentHeldObject.GetComponent<RestartObjects>();
            if (restartObjects != null)
            {
                restartObjects.isBeingGrabbed = false;
            }

            currentHeldObject = null; // Limpe a referência para a caixa atualmente segurada
            isGrabbing = false; // Marque que o jogador não está mais segurando uma caixa
        }
    }
}
