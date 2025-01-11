using UnityEngine;

public class DestroyByTime : MonoBehaviour
{
    [SerializeField]private float lifetime = 5f; // Tempo em segundos antes de destruir o objeto

    // Start é chamado antes do primeiro frame update
    void Start()
    {
        // Destroi o objeto após o tempo especificado
        Destroy(gameObject, lifetime);
    }
}
