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

    private int currentNodeIndex = 0; // �ndice do node atual
    private int direction = 1; // Dire��o do movimento (1 para frente, -1 para tr�s)
    private Vector3 targetPosition; // Posi��o alvo do pr�ximo node

    // Vari�vel para armazenar a posi��o inicial da plataforma
    private Vector3 initialPlatformPosition;

    // Array para armazenar as posi��es locais dos n�s em rela��o � posi��o da plataforma no momento da adi��o
    private Vector3[] nodeRelativePositions;

    // Array de tempos de transi��o entre os n�s. O tamanho desse array deve ser o mesmo que o array de nodes.
    public float[] nodeTransitionTimes;
    private float currentTransitionTime; // Tempo de transi��o atual entre os n�s


    private void Start()
    {
        // Armazena a posi��o inicial da plataforma
        initialPlatformPosition = transform.position;

        // Se a lista de nodes estiver vazia, adiciona o pr�prio transform da plataforma como �nico node
        if (nodes.Length == 0)
        {
            nodes = new Transform[] { transform };
        }

        // Inicializa o array de posi��es locais dos n�s
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
        // Move a plataforma em dire��o � posi��o alvo apenas se o editor de n�veis n�o estiver ativo
        if (!GameStates.Instance.isLevelEditor)
        {
            // Calcula a velocidade atual da plataforma usando o tempo de transi��o atual entre os n�s
            float currentSpeed = speed / currentTransitionTime;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

            // Verifica se a plataforma alcan�ou a posi��o alvo
            if (transform.position == targetPosition)
            {
                // Define o pr�ximo node como o pr�ximo alvo
                SetNextNode();
            }
        }
    }
    // Atualiza as posi��es dos n�s em rela��o � posi��o da plataforma
    private void UpdateNodePositions()
    {
        Vector3 platformOffset = transform.position - initialPlatformPosition;

        // Atualiza a posi��o de cada n� em rela��o � posi��o da plataforma
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i].position = nodes[i].position + platformOffset;
        }

        // Atualiza a posi��o inicial da plataforma para a nova posi��o
        initialPlatformPosition = transform.position;
    }

    private void SetNextNode()
    {
        // Avan�a para o pr�ximo node no array, considerando a dire��o do movimento
        currentNodeIndex += direction;

        // Verifica se alcan�ou o fim do array de nodes
        if (isPingPong)
        {
            // Se estiver no modo ping-pong, inverte a dire��o ao atingir os extremos do array
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
            // Se estiver no modo circular, volta ao primeiro node ap�s atingir o �ltimo
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
            // Se n�o estiver em nenhum dos modos especiais, volta ao primeiro node ap�s atingir o �ltimo
            if (currentNodeIndex >= nodes.Length)
            {
                currentNodeIndex = 0;
            }
        }

        SetTargetPosition();
    }

    private void SetTargetPosition()
    {
        // Define a posi��o alvo como a posi��o do pr�ximo node
        targetPosition = nodes[currentNodeIndex].position;

        // Define o tempo de transi��o atual usando o array de tempos de transi��o
        currentTransitionTime = nodeTransitionTimes[currentNodeIndex];
    }

    // Fun��o para adicionar novos nodes a partir de uma lista
    public void AddNodesFromList(List<Transform> nodeList)
    {
        nodes = nodeList.ToArray();
        SetTargetPosition();

        // Atualiza as posi��es relativas dos n�s em rela��o � posi��o atual da plataforma
        for (int i = 0; i < nodes.Length; i++)
        {
            nodeRelativePositions[i] = nodes[i].position - transform.position;
        }

        // Chama a fun��o para atualizar as posi��es dos n�s em rela��o � posi��o da plataforma
        UpdateNodePositions();
    }
}
