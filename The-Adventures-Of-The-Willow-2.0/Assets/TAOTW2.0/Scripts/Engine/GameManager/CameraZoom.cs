using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public static CameraZoom instance;

    private CinemachineVirtualCamera _vCam;
    [SerializeField] private PolygonCollider2D polygonCollider;
    [SerializeField] private PolygonCollider2D ignoreCollider;
    private CinemachineConfiner2D _confiner;
    public Rigidbody2D playerRb;

    [Range(1, 3)]
    public float waitTime;

    float waitCounter;

    bool zoomIn;
    bool zoomOutJump;
    public bool zoomfinish;

    public float zoomSpeed;
    public float zoomSpeedFinish;
    public float smoothTime = 0.2f; // Controle a suavidade do zoom aqui

    private float _currentZoomVelocity;

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
    }

    public void Initialize()
    {
        playerRb = PlayerController.instance.GetComponent<Rigidbody2D>();
        // Obt�m o GameObject do jogador (PlayerPrefab) associado ao Rigidbody2D
        GameObject playerObject = playerRb.gameObject;

        // Configura o follow do CinemachineVirtualCamera para seguir o jogador
        _vCam.Follow = playerObject.transform;

        AdjustGridColliderSize();
    }

    private void AdjustGridColliderSize()
    {
        // Calcula os v�rtices do ret�ngulo com base no tamanho do Tilemap
        Vector2[] vertices = new Vector2[4];
        vertices[0] = Vector2.zero;
        vertices[1] = new Vector2(LoadPlayLevel.instance.GridWidth, 0);
        vertices[2] = new Vector2(LoadPlayLevel.instance.GridWidth, LoadPlayLevel.instance.GridHeight);
        vertices[3] = new Vector2(0, LoadPlayLevel.instance.GridHeight);

        // Define os v�rtices do PolygonCollider2D para formar o ret�ngulo
        polygonCollider.SetPath(0, vertices);

        // Atualiza o tamanho do container da CinemachineVirtualCamera
        UpdateCinemachineConfiner();
    }

    private void UpdateCinemachineConfiner()
    {
        if (_confiner != null && polygonCollider != null)
        {
            // Obt�m os v�rtices atualizados do ret�ngulo
            Vector2[] vertices = polygonCollider.GetPath(0);

            // Cria uma lista de pol�gonos para definir a �rea de confinamento
            List<Vector2> polygon = new List<Vector2>();
            polygon.AddRange(vertices);

            // Define a �rea de confinamento do CinemachineConfiner2D com o pol�gono atualizado
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
        float targetZoom = 80;
        _vCam.m_Lens.FieldOfView = Mathf.SmoothDamp(_vCam.m_Lens.FieldOfView, targetZoom, ref _currentZoomVelocity, smoothTime);
    }

    public void SwimmingZoomOut()
    {
        float targetZoom = 60;
        _vCam.m_Lens.FieldOfView = Mathf.SmoothDamp(_vCam.m_Lens.FieldOfView, targetZoom, ref _currentZoomVelocity, smoothTime);
    }

    private void LateUpdate()
    {
        if (playerRb != null)
        {
            if (Mathf.Abs(playerRb.velocity.magnitude) < 8 && !zoomfinish)
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

            if (Mathf.Abs(playerRb.velocity.y) < 8 && !zoomfinish)
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
