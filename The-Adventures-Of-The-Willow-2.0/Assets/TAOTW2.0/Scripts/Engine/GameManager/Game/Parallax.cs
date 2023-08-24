using UnityEngine;

// Version 1.2 - 8 July 2023
// Created by This Isn't Games.
// Free to use in commercial projects.

[HelpURL("https://thisisntgames.itch.io/parallax")]
public class Parallax : MonoBehaviour
{
    [Tooltip("Drag Camera Object")]
    [SerializeField] private Transform stageCamera = null;
    private Vector2 cameraStartPosition;
    private Vector2 cameraCurrentPosition => stageCamera.position;
    private Vector2 deltaCamera;

    [Tooltip("Restriction of the Y-Axis Movement")]
    [SerializeField][Range(0f, 1f)] private float verticalRestriction = 0.5f;

    [System.Serializable]
    public class ParallaxLayer
    {
        public string Name;
        [Tooltip("Put all objects for one layer under a parent object.")]
        public Transform layerObject;
        [Tooltip("Positive is Furthest Away.")]
        [Range(-1, 1)] public float distanceFromCamera;
        public bool infiniteParallax;
        public float spriteWidth;
    }

    [Tooltip("How many layers will this Parallax have? ")]
    public ParallaxLayer[] layer;

    private void Start()
    {
        cameraStartPosition = cameraCurrentPosition;
        for (int i = 0; i < layer.Length; i++)
        {
            layer[i].spriteWidth = GetLayerWidth(layer[i].layerObject);
        }
    }

    private float GetLayerWidth(Transform layerObject)
    {
        Renderer renderer = layerObject.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.x;
        }
        return 0f;
    }

    private void FixedUpdate()
    {
        deltaCamera = cameraCurrentPosition - cameraStartPosition;

        for (int i = 0, n = layer.Length; i < n; i++)
        {
            var currentLayer = layer[i];
            var layerPosit = currentLayer.layerObject.position;
            var multiplyerX = currentLayer.distanceFromCamera;
            var multiplyerY = ((1 - multiplyerX) * verticalRestriction) + multiplyerX;

            if (currentLayer.infiniteParallax)
            {
                foreach (Transform sprite in currentLayer.layerObject)
                {
                    float infiniteX = deltaCamera.x * multiplyerX * currentLayer.spriteWidth * 0.5f;
                    Vector3 spritePosit = sprite.position;
                    spritePosit.x += infiniteX;

                    if (IsSpriteOutsideCamera(sprite, currentLayer.spriteWidth))
                    {
                        float spriteDirection = Mathf.Sign(deltaCamera.x);
                        spritePosit.x -= spriteDirection * currentLayer.spriteWidth;
                    }

                    spritePosit.y = deltaCamera.y * multiplyerY;
                    sprite.position = spritePosit;
                }
            }
            else
            {
                layerPosit.x = deltaCamera.x * multiplyerX;
                layerPosit.y = deltaCamera.y * multiplyerY;
                currentLayer.layerObject.position = layerPosit;
            }
        }
    }

    private bool IsSpriteOutsideCamera(Transform sprite, float spriteWidth)
    {
        Vector3 spriteViewportPos = stageCamera.GetComponent<Camera>().WorldToViewportPoint(sprite.position);
        return spriteViewportPos.x < 0f || spriteViewportPos.x > 1f;
    }
}
