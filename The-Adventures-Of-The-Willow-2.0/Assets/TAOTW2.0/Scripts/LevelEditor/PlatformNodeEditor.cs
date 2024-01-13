using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
public class PlatformNodeEditor : MonoBehaviour
{
    public static PlatformNodeEditor instance;

    [System.Serializable]
    public class WaypointData
    {
        public GameObject waypointObject;
        public Vector3 position;
    }

    public List<WaypointData> waypointsObjects = new List<WaypointData>();

    public bool isNodeEditor;
    public GameObject selectedPlatform;
    [SerializeField] private Color selectedColor;
    private Color originalColor;
    [SerializeField] private GameObject waypointPrefab;
    public Transform pointsLineRendererContainer;
    public Transform nodesLineRendererContainer; //All objects local
    [SerializeField] private List<Vector3> waypointseditor; // Declare waypoints na classe

    public PlatformController platformController; // Referência ao controlador da plataforma selecionada
    private GameObject waypoint;
    private GameObject previouslySelectedPlatform; // Variável para manter controle da plataforma anteriormente selecionada

    public Vector3 offset;
    private bool isDragging = false;

    public Button nodeButton;  // Arraste o botão para este campo no Inspector
    // Sprites para o botão ativo e desativado
    public Sprite activeSprite;    // Defina esta sprite no Inspector
    public Sprite inactiveSprite;  // Defina esta sprite no Inspector

    private LineRenderer lineRenderer;
    [SerializeField] private GameObject lineRenderPrefab;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        // Inicialmente, defina a sprite do botão com base no estado de isNodeEditor
        UpdateButtonSprite();
    }

    private void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);

        if (isNodeEditor)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("MovingPlatform"))
                    {
                        // Restaure a cor da plataforma anterior
                        if (previouslySelectedPlatform != null)
                        {
                            previouslySelectedPlatform.GetComponent<Renderer>().material.color = originalColor;
                        }

                        selectedPlatform = hit.collider.gameObject;
                        platformController = selectedPlatform.GetComponent<PlatformController>();
                        originalColor = selectedPlatform.GetComponent<Renderer>().material.color;
                        selectedPlatform.GetComponent<Renderer>().material.color = selectedColor;

                        EnsureWaypointsContainerExists();
                        // Limpe a lista waypointsObjects
                        waypointsObjects.Clear();

                        if (platformController.waypoints != null && platformController.waypoints.Count > 0)
                        {
                            // Instancie objetos com base nos waypoints da plataforma
                            foreach (Vector3 waypointPosition in platformController.waypoints)
                            {
                                GameObject newWaypointObject = Instantiate(waypointPrefab, waypointPosition, Quaternion.identity, pointsLineRendererContainer);

                                WaypointData waypointData = new WaypointData();
                                waypointData.waypointObject = newWaypointObject;
                                waypointData.position = newWaypointObject.transform.position;

                                waypointsObjects.Add(waypointData);
                            }

                            // Atualize a lista waypointseditor
                            UpdateWaypointsEditor();

                        }
                        // Defina a plataforma atualmente selecionada como a plataforma anteriormente selecionada
                        previouslySelectedPlatform = selectedPlatform;
                    }
                }

            }
            if (Mouse.current.leftButton.wasPressedThisFrame && Keyboard.current.shiftKey.isPressed && pointsLineRendererContainer != null)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                AddWaypoint(mousePosition);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame && Keyboard.current.ctrlKey.isPressed)
            {
                if (hit.collider != null && hit.collider.CompareTag("WayPoint"))
                {
                    waypoint = hit.collider.gameObject;

                    // Verifique se o waypoint pertence ao container correto
                    if (waypoint.transform.parent == pointsLineRendererContainer)
                    {
                        // Exclua o ponto do caminho
                        RemoveWaypoint(waypoint);
                        UpdateWaypointsEditor();
                    }
                }
            }
            if (Mouse.current.leftButton.isPressed)
            {
                if (hit.collider != null && hit.collider.CompareTag("WayPoint"))
                {
                    // Mover ponto do caminho

                    waypoint = hit.collider.gameObject;

                    // Verifique se o waypoint pertence ao container correto
                    if (waypoint.transform.parent == pointsLineRendererContainer)
                    {
                        // Mova o ponto do caminho
                        offset = waypoint.transform.position - Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

                        isDragging = true;

                        // Atualize a posição do waypoint na lista waypointsObjects enquanto ele é movido
                        UpdateWaypointPosition(waypoint);
                    }
                }
            }
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                isDragging = false;
            }
        }
        else
        {
            // Desselecione a plataforma se edit for falso
            DeselectPlatform();
        }
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            // Desselecione a plataforma se outra coisa for clicada
            DeselectPlatform();
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Verifica se o clique foi realizado em um elemento do UI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                isNodeEditor = false;
                UpdateButtonSprite();
                return; // Sai da função se o clique foi no UI
            }
        }
    }
    private void LateUpdate()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();

        // Verifica se o clique foi realizado em um elemento do UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return; // Sai da função se o clique foi no UI
        }
        if (isDragging && waypoint != null)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(mousePosition) + offset;
            waypoint.transform.position = new Vector3(newPosition.x, newPosition.y, waypoint.transform.position.z);
            UpdateWaypointPosition(waypoint);
            UpdateWaypointsEditor();
        }

    }
    // Função para remover um waypoint
    void RemoveWaypoint(GameObject waypointToRemove)
    {
        // Encontre e remova o WaypointData correspondente
        WaypointData dataToRemove = null;
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
        }
    }
    // Função para atualizar a posição de um waypoint na lista waypointsObjects
    void UpdateWaypointPosition(GameObject waypointToUpdate)
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
            platformController.waypoints = new List<Vector3>(waypointseditor);
            platformController.RenderLine();
        }
    }


    // Certifique-se de que o container de waypoints exista, se não, crie-o
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
    void DeselectPlatform()
    {
        if (selectedPlatform != null)
        {
            selectedPlatform.GetComponent<Renderer>().material.color = originalColor;
            selectedPlatform = null;
            platformController = null;
            pointsLineRendererContainer = null;
            waypointseditor.Clear();
            waypointsObjects.Clear();
        }
    }
    // Adicione um waypoint ao controlador da plataforma
    void AddWaypoint(Vector2 position)
    {
        // Instancie o ponto do caminho e adicione à cena na posição arredondada
        GameObject newWaypoint = Instantiate(waypointPrefab, position, Quaternion.identity, pointsLineRendererContainer);

        WaypointData waypointData = new WaypointData();
        waypointData.waypointObject = newWaypoint;
        waypointData.position = newWaypoint.transform.position;

        waypointsObjects.Add(waypointData);

        UpdateWaypointsEditor();
    }
    void UpdateWaypointsEditor()
    {
        waypointseditor.Clear(); // Limpa a lista para recriá-la na ordem correta.

        // Ordena a lista de waypointsObjects pela ordem de criação.
        waypointsObjects.Sort((a, b) => a.waypointObject.GetInstanceID().CompareTo(b.waypointObject.GetInstanceID()));

        // Adiciona os Vector3 à lista waypointseditor.
        foreach (var waypointData in waypointsObjects)
        {
            waypointseditor.Add(waypointData.position);
        }
        UpdatePlatform();
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
        // Limpe a lista waypointsObjects
        waypointsObjects.Clear();

        if (platformController.waypoints != null && platformController.waypoints.Count > 0)
        {
            // Obtém o nome da plataforma
            string platformName = platformController.gameObject.name;

            // Crie um novo objeto para o contêiner de pontos
            GameObject pointsContainer = new GameObject(platformName + "Points");

            // Configure o novo contêiner como filho do contêiner de nós
            pointsContainer.transform.SetParent(nodesLineRendererContainer);

            // Instancie objetos com base nos waypoints da plataforma dentro do contêiner de pontos
            foreach (Vector3 waypointPosition in platformController.waypoints)
            {
                GameObject newWaypointObject = Instantiate(waypointPrefab, waypointPosition, Quaternion.identity, pointsContainer.transform);

                WaypointData waypointData = new WaypointData();
                waypointData.waypointObject = newWaypointObject;
                waypointData.position = newWaypointObject.transform.position;

                waypointsObjects.Add(waypointData);
            }

        }
    }
    public void ToggleNodeEditor()
    {
        // Alterne o estado de isNodeEditor
        isNodeEditor = !isNodeEditor;

        // Atualize a sprite do botão com base no novo estado
        UpdateButtonSprite();

    }

    private void UpdateButtonSprite()
    {
        // Altere a sprite do botão com base no estado de isNodeEditor
        if (nodeButton != null)
        {
            if (isNodeEditor)
            {
                nodeButton.image.sprite = activeSprite;
            }
            else
            {
                nodeButton.image.sprite = inactiveSprite;
            }
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
                if(nodesLineRendererContainer != null)
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