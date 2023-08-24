using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    public string id;
    public Transform[] nodes; // Array de nodes que define o percurso da plataforma
    public float nodeTime;
    public float speed = 2f; // Velocidade de movimento da plataforma
    public bool isPingPong = false; // Define se a plataforma deve fazer um movimento ping-pong
    public bool isCircular = false; // Define se a plataforma deve fazer um movimento circular

    private int currentNodeIndex = 0; // Índice do node atual
    private int direction = 1; // Direção do movimento (1 para frente, -1 para trás)
    private Vector3 targetPosition; // Posição alvo do próximo node

    // Variável para armazenar a posição inicial da plataforma
    private Vector3 initialPlatformPosition;

    // Array para armazenar as posições locais dos nós em relação à posição da plataforma no momento da adição
    private Vector3[] nodeRelativePositions;

    // Array de tempos de transição entre os nós. O tamanho desse array deve ser o mesmo que o array de nodes.
    public float[] nodeTransitionTimes;
    private float currentTransitionTime; // Tempo de transição atual entre os nós


    private void Start()
    {
        // Armazena a posição inicial da plataforma
        initialPlatformPosition = transform.position;

        // Se a lista de nodes estiver vazia, adiciona o próprio transform da plataforma como único node
        if (nodes.Length == 0)
        {
            nodes = new Transform[] { transform };
        }

        // Inicializa o array de posições locais dos nós
        nodeRelativePositions = new Vector3[nodes.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            nodeRelativePositions[i] = nodes[i].position - initialPlatformPosition;
        }

        id = System.Guid.NewGuid().ToString();
        SetTargetPosition();
    }

    private void Update()
    {
        // Move a plataforma em direção à posição alvo apenas se o editor de níveis não estiver ativo
        if (!GameStates.Instance.isLevelEditor)
        {
            // Calcula a velocidade atual da plataforma usando o tempo de transição atual entre os nós
            float currentSpeed = speed / currentTransitionTime;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

            // Verifica se a plataforma alcançou a posição alvo
            if (transform.position == targetPosition)
            {
                // Define o próximo node como o próximo alvo
                SetNextNode();
            }
        }
    }
    // Atualiza as posições dos nós em relação à posição da plataforma
    private void UpdateNodePositions()
    {
        Vector3 platformOffset = transform.position - initialPlatformPosition;

        // Atualiza a posição de cada nó em relação à posição da plataforma
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i].position = nodes[i].position + platformOffset;
        }

        // Atualiza a posição inicial da plataforma para a nova posição
        initialPlatformPosition = transform.position;
    }

    private void SetNextNode()
    {
        // Avança para o próximo node no array, considerando a direção do movimento
        currentNodeIndex += direction;

        // Verifica se alcançou o fim do array de nodes
        if (isPingPong)
        {
            // Se estiver no modo ping-pong, inverte a direção ao atingir os extremos do array
            if (currentNodeIndex >= nodes.Length || currentNodeIndex < 0)
            {
                direction *= -1;

                // Corrige o currentNodeIndex caso tenha ultrapassado os limites do array
                currentNodeIndex = Mathf.Clamp(currentNodeIndex, 0, nodes.Length - 1);

                SetTargetPosition();
            }
        }
        else if (isCircular)
        {
            // Se estiver no modo circular, volta ao primeiro node após atingir o último
            if (currentNodeIndex >= nodes.Length)
            {
                currentNodeIndex = 0;
            }
            else if (currentNodeIndex < 0)
            {
                currentNodeIndex = nodes.Length - 1;
            }
        }
        else
        {
            // Se não estiver em nenhum dos modos especiais, volta ao primeiro node após atingir o último
            if (currentNodeIndex >= nodes.Length)
            {
                currentNodeIndex = 0;
            }
        }

        SetTargetPosition();
    }

    private void SetTargetPosition()
    {
        // Define a posição alvo como a posição do próximo node
        targetPosition = nodes[currentNodeIndex].position;

        // Define o tempo de transição atual usando o array de tempos de transição
        currentTransitionTime = nodeTransitionTimes[currentNodeIndex];
    }

    // Função para adicionar novos nodes a partir de uma lista
    public void AddNodesFromList(List<Transform> nodeList)
    {
        nodes = nodeList.ToArray();
        SetTargetPosition();

        // Atualiza as posições relativas dos nós em relação à posição atual da plataforma
        for (int i = 0; i < nodes.Length; i++)
        {
            nodeRelativePositions[i] = nodes[i].position - transform.position;
        }

        // Chama a função para atualizar as posições dos nós em relação à posição da plataforma
        UpdateNodePositions();
    }
}
