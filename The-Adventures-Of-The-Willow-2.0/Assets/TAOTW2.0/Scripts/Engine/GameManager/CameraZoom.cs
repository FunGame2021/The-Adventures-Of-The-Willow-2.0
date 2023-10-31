using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public static CameraZoom instance;

    [SerializeField] public CinemachineVirtualCamera _vCam;
    [SerializeField] private PolygonCollider2D polygonCollider;
    [SerializeField] private PolygonCollider2D ignoreCollider;
    private CinemachineConfiner2D _confiner;
    public Rigidbody2D playerRb;

    [Range(1, 3)]
    public float waitTime;

    float waitCounter;

    bool zoomIn;
    bool zoomOutJump;
    public bool zoomfinish = false;

    public float zoomSpeed;
    public float zoomSpeedFinish;
    public float smoothTime = 0.2f; // Controle a suavidade do zoom aqui

    private float _currentZoomVelocity;
    [SerializeField] public GameObject playerObject;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        zoomfinish = false;
        _vCam = GetComponent<CinemachineVirtualCamera>();
        _confiner = _vCam.GetComponent<CinemachineConfiner2D>();

        // Atribua o novo CinemachineFreeLook ao m_BoundingShape2D do confiner
        _confiner.m_BoundingShape2D = ignoreCollider;

        float targetZoom = 55;
        _vCam.m_Lens.FieldOfView = Mathf.SmoothDamp(_vCam.m_Lens.FieldOfView, targetZoom, ref _currentZoomVelocity, 1);
    }


    public void AdjustGridColliderSize()
    {
        // Calcula os vértices do retângulo com base no tamanho do Tilemap
        Vector2[] vertices = new Vector2[4];
        vertices[0] = Vector2.zero;
        if (GameStates.instance.isNormalGame)
        {
            vertices[1] = new Vector2(playLevel.instance.GridWidth, 0);
            vertices[2] = new Vector2(playLevel.instance.GridWidth, playLevel.instance.GridHeight);
            vertices[3] = new Vector2(0, playLevel.instance.GridHeight);
        }
        else
        {
            vertices[1] = new Vector2(LoadPlayLevel.instance.GridWidth, 0);
            vertices[2] = new Vector2(LoadPlayLevel.instance.GridWidth, LoadPlayLevel.instance.GridHeight);
            vertices[3] = new Vector2(0, LoadPlayLevel.instance.GridHeight);
        }

        // Define os vértices do PolygonCollider2D para formar o retângulo
        polygonCollider.SetPath(0, vertices);

        // Atualiza o tamanho do container da CinemachineVirtualCamera
        UpdateCinemachineConfiner();
    }

    private void UpdateCinemachineConfiner()
    {
        if (_confiner != null && polygonCollider != null)
        {
            // Obtém os vértices atualizados do retângulo
            Vector2[] vertices = polygonCollider.GetPath(0);

            // Cria uma lista de polígonos para definir a área de confinamento
            List<Vector2> polygon = new List<Vector2>();
            polygon.AddRange(vertices);

            // Define a área de confinamento do CinemachineConfiner2D com o polígono atualizado
            _confiner.m_BoundingShape2D = polygonCollider;
        }
    }

    public void ZoomIn()
    {
        float targetZoom = 55;
        _vCam.m_Lens.FieldOfView = Mathf.SmoothDamp(_vCam.m_Lens.FieldOfView, targetZoom, ref _currentZoomVelocity, smoothTime);
    }

    public void ZoomOut()
    {
        float targetZoom = 60;
        _vCam.m_Lens.FieldOfView = Mathf.SmoothDamp(_vCam.m_Lens.FieldOfView, targetZoom, ref _currentZoomVelocity, smoothTime);
    }

    public void ZoomOutJump()
    {
        float targetZoom = 65;
        _vCam.m_Lens.FieldOfView = Mathf.SmoothDamp(_vCam.m_Lens.FieldOfView, targetZoom, ref _currentZoomVelocity, smoothTime);
    }

    public void ZoomOutFinish()
    {
        zoomfinish = true;
        float targetZoom = 74;
        _vCam.m_Lens.FieldOfView = Mathf.SmoothDamp(_vCam.m_Lens.FieldOfView, targetZoom, ref _currentZoomVelocity, smoothTime);
    }

    public void SwimmingZoomOut()
    {
        float targetZoom = 60;
        _vCam.m_Lens.FieldOfView = Mathf.SmoothDamp(_vCam.m_Lens.FieldOfView, targetZoom, ref _currentZoomVelocity, smoothTime);
    }

    private void LateUpdate()
    {
        if (playerRb != null && !zoomfinish)
        {
            if (Mathf.Abs(playerRb.velocity.magnitude) < 8)
            {
                waitCounter += Time.deltaTime;
                if (waitCounter > waitTime)
                {
                    zoomIn = true;
                }
            }
            else
            {
                zoomIn = false;
                waitCounter = 0;
            }

            if (Mathf.Abs(playerRb.velocity.y) < 8)
            {
                waitCounter += Time.deltaTime;
                if (waitCounter > waitTime)
                {
                    zoomOutJump = true;
                }
            }
            else
            {
                zoomOutJump = false;
                waitCounter = 0;
            }

            if (zoomIn)
            {
                ZoomIn();
            }
            else
            {
                ZoomOut();
            }

            if (zoomOutJump)
            {
                ZoomIn();
            }
            else
            {
                ZoomOutJump();
            }
        }
    }
}
