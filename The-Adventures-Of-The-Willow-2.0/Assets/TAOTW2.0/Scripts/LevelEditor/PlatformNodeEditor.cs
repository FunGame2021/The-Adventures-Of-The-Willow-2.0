using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class PlatformNodeEditor : MonoBehaviour
{
    public static PlatformNodeEditor instance;

    [System.Serializable]
    public class EditorWaypointData
    {
        public GameObject waypointObject;
        public Vector3 position;
        public TextMeshProUGUI NumberText;
        public float TimeNode = 1f;       
        public float StopTime = 0f;
    }

    public List<EditorWaypointData> waypointsObjects = new List<EditorWaypointData>();
    public GameObject selectedPlatform;
    [SerializeField] private Color selectedColor;
    private Color originalColor;
    [SerializeField] private GameObject waypointPrefab;
    public Transform pointsLineRendererContainer;
    public Transform nodesLineRendererContainer;
    private LineRenderer lineRenderer;
    [SerializeField] private List<Vector3> waypointseditor;
    public PlatformController platformController;
    private GameObject waypoint;
    public Vector3 offset;
    private bool isDragging = false;
    public static Vector3 mousePosition;

    // Variáveis para controle de input
    private bool inputStarted;
    private bool inputHeld;
    private bool inputEnded;
    private bool isTouchInput;
    private bool isLongPressTriggered;

    // Variáveis para controle de clique rápido
    private float lastAddTime = 0f;
    private float addCooldown = 0.5f;
    private Vector3 lastAddedPosition;
    private float minDistanceBetweenNodes = 0.5f;

    private SpriteRenderer platformSpriteRenderer;
    private Color originalPlatformColor;

    // Variáveis para controle do double tap
    [SerializeField] private float doubleTapTimeThreshold = 0.3f; // Tempo máximo entre toques
    [SerializeField] private float maxTapDistance = 50f; // Distância máxima entre os toques para considerar double tap

    private float lastTapTime = 0f;
    private Vector2 lastTapPosition = Vector2.zero;
    private int tapCount = 0;



    private Vector2? GetMouseWorldPosition()
    {
        if (Mouse.current != null && Mouse.current.position != null)
        {
            return Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
        return null;
    }
    void HandleObjectSelection(Vector2 clickPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(clickPosition, Vector2.zero);
        if (hit.collider == null) return;

        if (hit.collider.CompareTag("WayPoint") && !MoveAndSelectTool.instance.AddPlatformNode.isOn
            && !MoveAndSelectTool.instance.RemovePlatformNode.isOn && !MoveAndSelectTool.instance.MovePlatformNode.isOn)
        {
            OpenNodePanelImmediately(hit.collider.gameObject);
        }
    }
    private void OpenNodePanelImmediately(GameObject waypointObj)
    {
        EditorWaypointData data = waypointsObjects.Find(w => w.waypointObject == waypointObj);
        if (data != null)
        {
            NodeEditorInfoText nodeEditor = FindFirstObjectByType<NodeEditorInfoText>();
            if (nodeEditor != null)
            {
                nodeEditor.OpenNodePanel(data);
            }
        }
    }
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        waypointsObjects.Clear();
    }

    private void Update()
    {
        UpdateInput();

        if (!MoveAndSelectTool.instance.isPlatformNodeEditor)
        {
            DeselectPlatform();
            return;
        }

        HandlePlatformSelection();
        HandleWaypointOperations();

        UpdatePlatformPosition();



        CheckMouseRightClick();
        CheckDoubleTapTouch();
    }



    private void CheckMouseRightClick()
    {
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (LevelEditorManager.instance.isActiveSelectPoint && MoveAndSelectTool.instance.isPlatformNodeEditor)
            {
                Vector2? clickPosition = GetMouseWorldPosition();
                if (clickPosition.HasValue)
                {
                    HandleObjectSelection(clickPosition.Value);
                }
            }
        }
    }

    private void CheckDoubleTapTouch()
    {
        if (Touchscreen.current == null)
            return;

        TouchControl touch = Touchscreen.current.primaryTouch;

        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            Vector2 currentTapPosition = touch.position.ReadValue();
            float currentTime = Time.time;

            // Se o tempo entre toques for muito grande, reseta o contador
            if (currentTime - lastTapTime > doubleTapTimeThreshold)
            {
                tapCount = 0;
            }

            if (tapCount == 1 &&
                currentTime - lastTapTime < doubleTapTimeThreshold &&
                Vector2.Distance(currentTapPosition, lastTapPosition) < maxTapDistance)
            {
                // Double tap detectado
                tapCount = 0;

                if (LevelEditorManager.instance.isActiveSelectPoint)
                {
                    Vector2 worldPos = Camera.main.ScreenToWorldPoint(currentTapPosition);
                    HandleObjectSelection(worldPos);
                }
            }
            else
            {
                // Primeiro toque ou inválido, inicia nova contagem
                tapCount = 1;
                lastTapTime = currentTime;
                lastTapPosition = currentTapPosition;
            }
        }
    }
    private void UpdatePlatformPosition()
    {
        if (selectedPlatform != null && waypointsObjects.Count > 0)
        {
            selectedPlatform.transform.position = waypointsObjects[0].position;
        }
    }
    private void UpdateInput()
    {
        if (EventSystem.current != null)
        {
#if UNITY_EDITOR
            if (EventSystem.current.IsPointerOverGameObject())
                return; // Evita seleção ao clicar em UI com o mouse
#else
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
                if (EventSystem.current.IsPointerOverGameObject(touchId))
                    return; // Evita seleção ao tocar em UI no mobile
            }
#endif
        }

        // Reset input states
        inputStarted = false;
        inputHeld = false;
        inputEnded = false;
        isTouchInput = false;

        // Check for touch input first
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            isTouchInput = true;
            mousePosition = LevelEditorManager.instance.mainCamera.ScreenToWorldPoint(
                Touchscreen.current.primaryTouch.position.ReadValue());

            var phase = Touchscreen.current.primaryTouch.phase.ReadValue();
            if (phase == UnityEngine.InputSystem.TouchPhase.Began) inputStarted = true;
            else if (phase == UnityEngine.InputSystem.TouchPhase.Moved || phase == UnityEngine.InputSystem.TouchPhase.Stationary) inputHeld = true;
            else if (phase == UnityEngine.InputSystem.TouchPhase.Ended || phase == UnityEngine.InputSystem.TouchPhase.Canceled) inputEnded = true;
        }
        // Fall back to mouse input if no touch
        else if (Mouse.current != null)
        {
            mousePosition = LevelEditorManager.instance.mainCamera.ScreenToWorldPoint(
                Mouse.current.position.ReadValue());

            inputStarted = Mouse.current.leftButton.wasPressedThisFrame;
            inputHeld = Mouse.current.leftButton.isPressed;
            inputEnded = Mouse.current.leftButton.wasReleasedThisFrame;
        }

        // Ignore input over UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            inputStarted = false;
            inputHeld = false;
        }
    }

    private void HandlePlatformSelection()
    {
        if (inputStarted)
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("MovingPlatform"))
            {
                SelectPlatform(hit.collider.gameObject);
            }
        }
    }


    public void SelectPlatform(GameObject platform)
    {
        // Desseleciona plataforma anterior
        if (selectedPlatform != null)
        {
            platformSpriteRenderer.color = originalPlatformColor;
        }

        selectedPlatform = platform;
        platformController = selectedPlatform.GetComponent<PlatformController>();
        platformSpriteRenderer = selectedPlatform.GetComponent<SpriteRenderer>();
        originalPlatformColor = platformSpriteRenderer.color;
        platformSpriteRenderer.color = selectedColor;

        // Prepara waypoints
        EnsureWaypointsContainerExists();
        waypointsObjects.Clear();

        // Update waypoints loading
        if (platformController.waypointsData != null && platformController.waypointsData.Count > 0)
        {
            foreach (var wpData in platformController.waypointsData)
            {
                GameObject wp = Instantiate(waypointPrefab, wpData.Position, Quaternion.identity, pointsLineRendererContainer);

                EditorWaypointData data = new EditorWaypointData
                {
                    waypointObject = wp,
                    position = wp.transform.position,
                    NumberText = wp.GetComponentInChildren<TextMeshProUGUI>(true),
                    TimeNode = wpData.TimeNode,
                    StopTime = wpData.StopTime
                };

                waypointsObjects.Add(data);
            }
            UpdateWaypointNumbers();
            MovePlatformToFirstWaypoint();
        }
        MovePlatformToFirstWaypoint();



        // Garantir que não estamos arrastando quando selecionamos uma nova plataforma
        isDragging = false;
    }

    private void MovePlatformToFirstWaypoint()
    {
        if (waypointsObjects.Count > 0 && selectedPlatform != null)
        {
            selectedPlatform.transform.position = waypointsObjects[0].position;
        }
    }

    private Vector3 GetCurrentMouseWorldPosition()
    {
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            return Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y,
                Camera.main.transform.position.z - selectedPlatform.transform.position.z));
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
            return Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y,
                Camera.main.transform.position.z - selectedPlatform.transform.position.z));
        }

        return Vector3.zero;
    }

    private void HandleWaypointOperations()
    {
        if (selectedPlatform == null) return;

        // Raycast para detectar objetos
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        // -------------------------
        // Remover Node
        // -------------------------
        if (inputStarted && MoveAndSelectTool.instance.RemovePlatformNode.isOn)
        {
            if (hit.collider != null && hit.collider.CompareTag("WayPoint"))
            {
                HandleWaypointRemoval(hit.collider.gameObject);
                return; // Impede outros inputs no mesmo frame
            }
        }

        // -------------------------
        // Adicionar Node
        // -------------------------
        if (inputStarted && MoveAndSelectTool.instance.AddPlatformNode.isOn)
        {
            AddWaypoint(mousePosition);
            return; // Impede outros inputs no mesmo frame
        }

        // -------------------------
        // Mover Node
        // -------------------------
        if (MoveAndSelectTool.instance.MovePlatformNode.isOn)
        {
            if (inputStarted)
            {

                if (hit.collider != null && hit.collider.CompareTag("WayPoint") && !MoveAndSelectTool.instance.AddPlatformNode.isOn
            && !MoveAndSelectTool.instance.RemovePlatformNode.isOn && MoveAndSelectTool.instance.MovePlatformNode.isOn) 
                {
                    waypoint = hit.collider.gameObject;
                    offset = waypoint.transform.position - mousePosition;

                    isDragging = true; 
                }
            }

            if (inputHeld && isDragging && waypoint != null)
            {
                Vector3 newPosition = mousePosition + offset;
                waypoint.transform.position = new Vector3(newPosition.x, newPosition.y, waypoint.transform.position.z);
                UpdateWaypointPosition(waypoint);
                UpdateWaypointsEditor();
            }

            if (inputEnded)
            {
                isDragging = false;
                waypoint = null;
            }
        }
    }


    private void HandleWaypointRemoval(GameObject waypointToRemove)
    {
        if (waypointToRemove.transform.parent == pointsLineRendererContainer)
        {
            RemoveWaypoint(waypointToRemove);
            UpdateWaypointsEditor();
        }
    }


    private void LateUpdate()
    {
        if (MoveAndSelectTool.instance.MovePlatformNode.isOn && isDragging && waypoint != null && inputHeld)
        {
            Vector3 newPosition = mousePosition + offset;
            waypoint.transform.position = new Vector3(newPosition.x, newPosition.y, waypoint.transform.position.z);
            UpdateWaypointPosition(waypoint);

            // Se estiver movendo o primeiro waypoint, atualiza a plataforma também
            if (waypoint == waypointsObjects[0].waypointObject)
            {
                selectedPlatform.transform.position = waypoint.transform.position;
            }

            UpdateWaypointsEditor();

            if (platformController != null)
            {
                platformController.RenderLine();
            }
        }
    }

    // Função para remover um waypoint
    void RemoveWaypoint(GameObject waypointToRemove)
    {
        // Encontre e remova o WaypointData correspondente
        EditorWaypointData dataToRemove = null;
        foreach (var data in waypointsObjects)
        {
            if (data.waypointObject == waypointToRemove)
            {
                dataToRemove = data;
                break;
            }
        }

        // Remova o WaypointData da lista e destrua o GameObject do waypoint
        if (dataToRemove != null)
        {
            waypointsObjects.Remove(dataToRemove);
            Destroy(waypointToRemove);
            UpdateWaypointNumbers();

        }
    }

    // Função para atualizar a posição de um waypoint na lista waypointsObjects
    public void UpdateWaypointPosition(GameObject waypointToUpdate)
    {
        foreach (var data in waypointsObjects)
        {
            if (data.waypointObject == waypointToUpdate)
            {
                data.position = waypointToUpdate.transform.position;
                break;
            }
        }
    }

    void UpdatePlatform()
    {
        if (platformController != null)
        {
            platformController.SetWaypointsFromEditor(waypointsObjects);
        
            platformController.RenderLine();
        }
    }

    // Função para garantir que o contêiner dos pontos do caminho exista
    void EnsureWaypointsContainerExists()
    {
        string containerName = selectedPlatform.name + "Points";
        Transform container = nodesLineRendererContainer.Find(containerName);

        if (container == null)
        {
            // Verifique se o contêiner de pontos com o mesmo nome já existe
            bool containerExists = false;
            foreach (Transform child in nodesLineRendererContainer)
            {
                if (child.name == containerName)
                {
                    container = child;
                    containerExists = true;
                    break;
                }
            }

            if (!containerExists)
            {
                container = new GameObject(containerName).transform;
                container.SetParent(nodesLineRendererContainer);
            }
        }
        // Apague todos os filhos (pontos) existentes no contêiner
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        // Defina o contêiner de pontos como membro da classe
        pointsLineRendererContainer = container;
    }

    // Função para desselecionar a plataforma anterior
    public void DeselectPlatform()
    {
        if (selectedPlatform != null)
        {
            platformSpriteRenderer.color = originalPlatformColor;
            selectedPlatform = null;
            platformController = null;
            pointsLineRendererContainer = null;
            waypointseditor.Clear();
            waypointsObjects.Clear();
            platformSpriteRenderer = null;
        }
    }

    private void AddWaypoint(Vector2 position)
    {
        // Verifica cooldown e distância do último node adicionado
        if (Time.time - lastAddTime < addCooldown &&
            Vector3.Distance(position, lastAddedPosition) < minDistanceBetweenNodes)
        {
            return;
        }

        // Instancia o novo waypoint
        GameObject newWaypoint = Instantiate(waypointPrefab, position, Quaternion.identity, pointsLineRendererContainer);

        // Obtém o componente TextMeshProUGUI
        TextMeshProUGUI textComponent = newWaypoint.GetComponentInChildren<TextMeshProUGUI>(true);

        if (textComponent == null)
        {
            Debug.LogError("TextMeshProUGUI não encontrado no prefab do waypoint!");
            return;
        }

        // Cria e adiciona o waypoint no FINAL da lista
        EditorWaypointData waypointData = new EditorWaypointData
        {
            waypointObject = newWaypoint,
            position = newWaypoint.transform.position,
            NumberText = textComponent,
            TimeNode = 1f, // Default values
            StopTime = 0f
        };

        waypointsObjects.Add(waypointData);
        UpdateWaypointsEditor();
        UpdateWaypointNumbers();

        // Atualiza os registros de tempo e posição
        lastAddTime = Time.time;
        lastAddedPosition = position;
    }

    public void UpdateWaypointsEditor()
    {
        waypointseditor.Clear();

        // Ordena os waypoints pela ordem de criação (não altera a ordem dos existentes)
        // Mantém a ordem original de adição
        foreach (var waypointData in waypointsObjects)
        {
            waypointseditor.Add(waypointData.position);
        }

        UpdatePlatform();
    }

    private void UpdateWaypointNumbers()
    {
        for (int i = 0; i < waypointsObjects.Count; i++)
        {
            if (waypointsObjects[i].NumberText == null)
            {
                waypointsObjects[i].NumberText = waypointsObjects[i].waypointObject.GetComponentInChildren<TextMeshProUGUI>(true);

                if (waypointsObjects[i].NumberText == null)
                {
                    Debug.LogError("TextMeshProUGUI não encontrado!");
                    continue;
                }

                // Configurações críticas para o tamanho do texto
                waypointsObjects[i].NumberText.fontSize = 0.5f; // Tamanho em unidades world
            }

            waypointsObjects[i].NumberText.text = (i + 1).ToString();
            waypointsObjects[i].NumberText.alignment = TextAlignmentOptions.Center;
            waypointsObjects[i].NumberText.color = Color.white;
            waypointsObjects[i].NumberText.gameObject.SetActive(true);
        }
    }

    public void ObtainCreateAllNodes()
    {
        GameObject[] movingPlatforms = GameObject.FindGameObjectsWithTag("MovingPlatform");

        foreach (GameObject movingPlatform in movingPlatforms)
        {
            PlatformController platformController = movingPlatform.GetComponent<PlatformController>();

            if (platformController != null)
            {
                // Crie os nós para esta plataforma
                CreateNodesForPlatform(platformController);
            }
        }
    }

    private void CreateNodesForPlatform(PlatformController platformController)
    {
        waypointsObjects.Clear();

        if (platformController.waypointsData != null && platformController.waypointsData.Count > 0)
        {
            string platformName = platformController.gameObject.name;
            GameObject pointsContainer = new GameObject(platformName + "Points");
            pointsContainer.transform.SetParent(nodesLineRendererContainer);

            foreach (var waypointInfo in platformController.waypointsData)
            {
                GameObject newWaypointObject = Instantiate(waypointPrefab, waypointInfo.Position, Quaternion.identity, pointsContainer.transform);

                EditorWaypointData waypointData = new EditorWaypointData
                {
                    waypointObject = newWaypointObject,
                    position = newWaypointObject.transform.position,
                    NumberText = newWaypointObject.GetComponentInChildren<TextMeshProUGUI>(true),
                    TimeNode = waypointInfo.TimeNode,
                    StopTime = waypointInfo.StopTime
                };

                waypointsObjects.Add(waypointData);
            }
            UpdateWaypointNumbers();
        }
    }

    public void DeleteThisPlatform(GameObject selectedPlatformDelete)
    {
        if (selectedPlatformDelete != null)
        {
            PlatformController platformController = selectedPlatformDelete.GetComponent<PlatformController>();

            if (platformController != null)
            {
                string containerName = selectedPlatformDelete.name + "Points";
                Transform container = nodesLineRendererContainer.Find(containerName);
                if (nodesLineRendererContainer != null)
                {
                    foreach (Transform child in nodesLineRendererContainer)
                    {
                        if (child.name == containerName)
                        {
                            pointsLineRendererContainer = container;
                            break;
                        }
                    }
                }
                if (pointsLineRendererContainer != null)
                {
                    foreach (Transform child in pointsLineRendererContainer)
                    {
                        Destroy(child.gameObject);
                    }

                    // Destrua o contêiner de pontos
                    Destroy(pointsLineRendererContainer.gameObject);
                }

                // Obtenha o LineRenderer da plataforma
                LineRenderer lineRendererToDelete = platformController.lineRenderer;

                if (lineRendererToDelete != null)
                {
                    // Destrua o LineRenderer
                    Destroy(lineRendererToDelete.gameObject);
                }

                // Limpe a lista waypointseditor
                waypointseditor.Clear();
                Destroy(selectedPlatformDelete.gameObject);
            }
        }
    }
}