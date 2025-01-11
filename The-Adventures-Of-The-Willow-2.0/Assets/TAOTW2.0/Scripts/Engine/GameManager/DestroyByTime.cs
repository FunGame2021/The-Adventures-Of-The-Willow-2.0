using UnityEngine;

public class DestroyByTime : MonoBehaviour
{
    [SerializeField]private float lifetime = 5f; // Tempo em segundos antes de destruir o objeto

    // Start � chamado antes do primeiro frame update
    void Start()
    {
        // Destroi o objeto ap�s o tempo especificado
        Destroy(gameObject, lifetime);
    }
}
