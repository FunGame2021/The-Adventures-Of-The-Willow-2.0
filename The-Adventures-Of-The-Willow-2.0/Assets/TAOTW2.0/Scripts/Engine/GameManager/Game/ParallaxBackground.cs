using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private float lengthX, lengthY;
    private float startposX, startposY;
    private float startYOffset;
    public GameObject cam;
    public float parallaxEffectX;
    public float parallaxEffectY;
    public bool alwaysVisible;

    private void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        lengthX = spriteRenderer.bounds.size.x;
        lengthY = spriteRenderer.bounds.size.y;

        startposX = transform.position.x;
        startposY = transform.position.y;

        startYOffset = cam.transform.position.y - startposY;
    }

    private void FixedUpdate()
    {
        float tempX = (cam.transform.position.x * (1 - parallaxEffectX));
        float distX = (cam.transform.position.x * parallaxEffectX);

        // Movimento no eixo X
        transform.position = new Vector3(startposX + distX, transform.position.y, transform.position.z);

        if (tempX > startposX + lengthX / 2)
        {
            startposX += lengthX;
        }
        else if (tempX < startposX - lengthX / 2)
        {
            startposX -= lengthX;
        }

        // Movimento no eixo Y
        float newY;

        if (alwaysVisible)
        {
            float distY = (cam.transform.position.y * parallaxEffectY);
            float lowerYLimit = startposY - lengthY / 2;
            float upperYLimit = startposY + lengthY / 2;
            float cameraY = cam.transform.position.y;
            newY = Mathf.Clamp(startposY + distY, lowerYLimit + cameraY, upperYLimit + cameraY);
        }
        else
        {
            float cameraY = cam.transform.position.y;
            float distY = (cameraY - (startposY + startYOffset)) * parallaxEffectY;
            newY = startposY + distY;
        }

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
