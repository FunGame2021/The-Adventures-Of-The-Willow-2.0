using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PlatformNodeEditor : MonoBehaviour
{
    public static PlatformNodeEditor instance;
    public bool isNodeEditor;
    private GameObject selectedPlatform;
    [SerializeField] private Color selectedColor;
    private Color originalColor;


    public EraserTool eraserTool;

    //Nodes
    [SerializeField] private GameObject NodeObjectPrefab;
    public List<Transform> nodesList = new List<Transform>(); // Lista para armazenar os nodes da plataforma
    // Dicionário para mapear plataformas aos seus nodes
    public Dictionary<GameObject, List<Transform>> platformNodesDictionary = new Dictionary<GameObject, List<Transform>>();
    public List<GameObject> nodeObjects = new List<GameObject>(); // Lista para armazenar os objetos que representam os nodes da plataforma
    public List<float> nodeTimesList = new List<float>();

    public Transform selectedNodeToMove; // Referência ao node que está sendo movido atualmente

    [SerializeField] private LineRenderer lineRenderer; // Referência ao componente LineRenderer
    [SerializeField] private float lineRendererWidth = 0.1f; // Largura da linha do LineRenderer

    public bool isDragging = false;
    public Vector3 offset;


    //Tempo de cada node
    [SerializeField] private TMP_InputField speedInputField; // Prefab do InputField para a velocidade
    private TMP_InputField tmpSpeedInputField; // Campo de entrada do TextMeshPro para lidar com float inputs
    public GameObject nodeMenuPanel; // Painel de UI para o menu do nó
    public TextMeshProUGUI timeText; // Texto para exibir o tempo atual do nó
    private Transform selectedNode;

    public bool isShiftHeld = false;
    private bool isCtrlHeld = false;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        lineRenderer.positionCount = 0; // Inicialmente, não há pontos no LineRenderer

        // Obtenha o componente TMP_InputField do InputField para trabalhar com números float
        tmpSpeedInputField = speedInputField.GetComponent<TMP_InputField>();

        // Defina o tipo de conteúdo do campo de entrada para aceitar apenas números inteiros ou floats
        tmpSpeedInputField.contentType = TMP_InputField.ContentType.DecimalNumber; // Ou TMP_InputField.ContentType.IntegerNumber para aceitar apenas inteiros

    }
    public void Active()
    {
        isNodeEditor = !isNodeEditor;
    }

    void Update()
    {
        // Detect if Shift or Ctrl keys are held
        isShiftHeld = Keyboard.current.shiftKey.isPressed;
        isCtrlHeld = Keyboard.current.ctrlKey.isPressed;


        //desativa coisas selecionadas de outras funções evitar que sejam instanciados quando estou a usar a ferramenta
        if (isNodeEditor)
        {
            if (DecorButton.instance != null)
            {
                DecorButton.instance.Deselect();
            }
            if (ObjectsButton.instance != null)
            {
                ObjectsButton.instance.Deselect();
            }
            if (GameObjectButton.instance != null)
            {
                GameObjectButton.instance.Deselect();
            }
            if (Decor2Button.instance != null)
            {
                Decor2Button.instance.Deselect();
            }
            if (EnemyButton.instance != null)
            {
                EnemyButton.instance.Deselect();
            }
        }

        // Seleciona a plataforma e muda para a cor personalizada, senão volta para a cor normal
        // Verifica se o clique atingiu um objeto com a tag "ObjectObject" e define a plataforma selecionada
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null && hit.collider.CompareTag("ObjectObject") && Mouse.current.leftButton.wasPressedThisFrame)
        {

            // Limpa a seleção anterior da plataforma
            if (selectedPlatform != null)
            {
                selectedPlatform.GetComponent<SpriteRenderer>().color = originalColor;
            }

            // Seleciona a nova plataforma
            selectedPlatform = hit.collider.gameObject;

            // Muda para a cor personalizada
            originalColor = selectedPlatform.GetComponent<SpriteRenderer>().color;
            selectedPlatform.GetComponent<SpriteRenderer>().color = selectedColor;

            // Verifica se a plataforma já tem nodes no dicionário, se não tiver, cria uma nova lista de nodes
            //if (!platformNodesDictionary.ContainsKey(selectedPlatform))
            //{
            //    platformNodesDictionary[selectedPlatform] = new List<Transform>();
            //}

            // Atualiza a lista de nodes na plataforma selecionada
            PlatformMovement platformMovement = selectedPlatform.GetComponent<PlatformMovement>();
            if (platformMovement != null)
            {
                nodesList = platformMovement.nodes.ToList();
                // Atualiza a lista de tempos de nó para a plataforma selecionada
                nodeTimesList = platformMovement.nodeTransitionTimes.ToList();
            }

            // Atualiza a lista de nodes da plataforma selecionada
            //nodesList = platformNodesDictionary[selectedPlatform];

            // Atualiza a linha do Line Renderer para refletir os novos nodes da plataforma selecionada
            RenderLine();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            //chamar AddNode() e adicionar node no local do clique, ele adiciona o node à platforma selecionada

            //Se eraser tool estiver ativo retorna
            if (eraserTool.isActiveEraserTile || eraserTool.isActiveEraserEnemy)
            {
                return;
            }
            // Verifica se o clique foi realizado em um elemento do UI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return; // Sai da função se o clique foi no UI
            }
            if (isNodeEditor && selectedPlatform != null)
            {
                if (isCtrlHeld)
                {
                    // Se Ctrl está pressionado, adicionar um novo node na posição do clique
                    AddNode();
                }

                // Atualiza a lista de nodes na plataforma selecionada
                PlatformMovement platformMovement = selectedPlatform.GetComponent<PlatformMovement>();
                if (platformMovement != null)
                {
                    platformMovement.nodes = nodesList.ToArray();
                }

            }
        }
        // Verifica se o botão esquerdo do mouse foi solto e finaliza o movimento do node
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            selectedNodeToMove = null;
            isDragging = false;
            if (selectedPlatform != null)
            {
                // Renderiza o caminho atualizado da plataforma após soltar o nó
                RenderLine();
            }
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && isShiftHeld)
        {
            // Verifica se o mouse está sobre um nó
            RaycastHit2D nodeHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);
            if (nodeHit.collider != null)
            {
                GameObject HitNode = nodeHit.collider.gameObject; 
                Debug.Log(HitNode);
                if (HitNode.CompareTag("Node") && nodesList.Contains(HitNode.transform))
                {
                    selectedNodeToMove = HitNode.transform;
                    offset = selectedNodeToMove.position - Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                    isDragging = true;
                }
            }
        }

        if(Mouse.current.rightButton.wasPressedThisFrame)
        {
            // Verifica se o mouse está sobre um nó
            RaycastHit2D nodeHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);

            if (isCtrlHeld)
            {
                if (nodeHit.collider != null)
                {
                    GameObject hitNode = nodeHit.collider.gameObject;

                    // Verifica se o node clicado pertence à plataforma selecionada
                    if (nodesList.Contains(hitNode.transform))
                    {
                        // Remover o node da lista de nodes da plataforma
                        nodesList.Remove(hitNode.transform);

                        // Remover o GameObject associado ao node
                        Destroy(hitNode);

                        // Atualiza a lista de nodes na plataforma selecionada no dicionário
                        UpdateNodesListForPlatform(selectedPlatform, nodesList);

                        // Renderiza o caminho atualizado da plataforma após remover o node
                        RenderLine();
                    }
                }
            }
            if(isShiftHeld)
            {
                if (nodeHit.collider != null)
                {
                    GameObject hitNode = nodeHit.collider.gameObject;

                    // Verifica se o node clicado pertence à plataforma selecionada
                    if (nodesList.Contains(hitNode.transform))
                    {
                        OpenNodeMenu(hitNode.transform);
                    }
                }
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
        if (isDragging && selectedNodeToMove != null && isShiftHeld)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(mousePosition) + offset;
            selectedNodeToMove.position = new Vector3(newPosition.x, newPosition.y, selectedNodeToMove.position.z);

            // Atualiza a lista de nodes na plataforma selecionada no dicionário
            UpdateNodesListForPlatform(selectedPlatform, nodesList);
        }

        // Se o movimento do node foi concluído (mouse solto ou movimento interrompido pelo editor do Unity)
        if (selectedNodeToMove != null && !isDragging && selectedPlatform != null)
        {
            // Atualize a posição do node na lista nodesList
            int nodeIndex = nodesList.IndexOf(selectedNodeToMove);
            if (nodeIndex >= 0)
            {
                nodesList[nodeIndex] = selectedNodeToMove;
            }

            // Atualiza a lista de nodes na plataforma selecionada no dicionário
            UpdateNodesListForPlatform(selectedPlatform, nodesList);

            // Renderiza o caminho atualizado da plataforma após mover um node
            RenderLine();

            // Redefine a referência ao node selecionado para evitar a reatualização da posição após o movimento
            selectedNodeToMove = null;
        }
    }

     private void AddNode()
    {
        // Adicionar nodes por clique com mouse novo input clica na cena adiciona um node (NodeObjectPrefab)
        // Cada node posso clicar com o botão esquerdo em cima dele ele abre um menu onde posso adicionar a sua velocidade (ou tempo em segundos) em que a plataforma se move até ao próximo node

        // Converte a posição do mouse para a posição no mundo
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0f;

        // Se não houver nenhum node na lista da plataforma, adiciona a posição da plataforma como o primeiro node
        if (nodesList.Count == 0)
        {
            nodesList.Add(selectedPlatform.transform);
        }

        GameObject newNode = Instantiate(NodeObjectPrefab, mousePosition, Quaternion.identity);

        nodesList.Add(newNode.transform);

        UpdateNodesListForPlatform(selectedPlatform, nodesList);

        RenderLine();
    }


    private void OpenNodeMenu(Transform nodeTransform)
    {
        // Abre o menu do nó para edição

        // Atualiza o nó associado ao painel de menu
        selectedNode = nodeTransform;

        // Verifica se já existe um menu aberto para este nó
        if (!nodeMenuPanel.activeSelf)
        {
            // Atualiza o texto do tempo do nó no painel
            PlatformMovement platformMovement = selectedPlatform.GetComponent<PlatformMovement>();
            if (platformMovement != null && platformMovement.nodeTransitionTimes.Length > nodesList.IndexOf(selectedNode))
            {
                float time = platformMovement.nodeTransitionTimes[nodesList.IndexOf(selectedNode)];
                timeText.text = time.ToString();
                // Defina o valor do campo de entrada para o tempo atual do nó
                tmpSpeedInputField.text = time.ToString();
            }

            // Ativa o painel de menu do nó
            nodeMenuPanel.SetActive(true);
        }
    }

    public void SaveNodeTime()
    {
        // Salva o tempo inserido pelo usuário no array nodeTransitionTimes do componente PlatformMovement
        if (selectedNode != null && tmpSpeedInputField != null)
        {
            PlatformMovement platformMovement = selectedPlatform.GetComponent<PlatformMovement>();
            if (platformMovement != null)
            {
                int nodeIndex = platformMovement.nodes.ToList().IndexOf(selectedNode);
                if (nodeIndex >= 0)
                {
                    float time;
                    if (float.TryParse(tmpSpeedInputField.text, out time))
                    {
                        // Certifique-se de que o tempo seja maior ou igual a 0
                        time = Mathf.Max(0f, time);

                        // Atualiza o array de tempos de transição para o nó atual
                        if (platformMovement.nodeTransitionTimes.Length > nodeIndex)
                        {
                            platformMovement.nodeTransitionTimes[nodeIndex] = time;
                        }
                        else
                        {
                            // Caso o array não tenha espaço suficiente, redimensiona-o e adiciona o tempo do nó
                            float[] newTimesArray = new float[nodeIndex + 1];
                            platformMovement.nodeTransitionTimes.CopyTo(newTimesArray, 0);
                            newTimesArray[nodeIndex] = time;
                            platformMovement.nodeTransitionTimes = newTimesArray;
                        }
                    }
                }
            }
        }

        // Desativa o painel de menu do nó após salvar o tempo
        nodeMenuPanel.SetActive(false);
    }


    private void RenderLine()
    {
        // Atualiza o Line Renderer para mostrar o caminho da plataforma
        lineRenderer.positionCount = nodesList.Count; // Define o número de pontos no Line Renderer

        // Define as posições dos pontos no Line Renderer com base nos nodes da plataforma
        for (int i = 0; i < nodesList.Count; i++)
        {
            lineRenderer.SetPosition(i, nodesList[i].position);
        }

        // Configurações visuais do Line Renderer
        lineRenderer.startWidth = lineRendererWidth;
        lineRenderer.endWidth = lineRendererWidth;
    }

    private void UpdateNodesListForPlatform(GameObject platform, List<Transform> updatedNodes)
    {
        if (platformNodesDictionary.ContainsKey(platform))
        {
            platformNodesDictionary[platform] = updatedNodes;
        }
        else
        {
            platformNodesDictionary.Add(platform, updatedNodes);
        }
    }
}