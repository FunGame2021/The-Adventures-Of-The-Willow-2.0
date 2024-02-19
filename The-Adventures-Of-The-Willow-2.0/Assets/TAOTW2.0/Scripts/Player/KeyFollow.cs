using Bundos.WaterSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeyFollow : MonoBehaviour
{
    public Transform KeyFollowPoint;

    public List<Key> followingKeys = new List<Key>();

    private Vector3 targetPosition;
    private Vector3 currentVelocity;


    public float spacing = 0.3f; // Espaçamento entre as chaves na fila
    public float smoothTime = 0.3f;
    public float delay = 1.0f;  // Atraso em segundos

    void Update()
    {
        for (int i = 0; i < followingKeys.Count; i++)
        {
            if (followingKeys[i] != null)
            {
                // Posição atrasada em relação à posição do jogador
                Vector3 delayedPosition = KeyFollowPoint.position - KeyFollowPoint.forward * delay;

                // Obtém a direção do jogador
                Vector3 playerDirection = PlayerController.instance.facingRight ? KeyFollowPoint.right : -KeyFollowPoint.right;

                // Aplica o espaçamento horizontal com base na direção do jogador
                delayedPosition += playerDirection * i * spacing;

                // Bloqueia o componente Z para evitar movimento no eixo Z
                delayedPosition.z = followingKeys[i].transform.position.z;

                // Aplica a suavização exponencial à posição da chave
                Vector3 smoothedPosition = Vector3.SmoothDamp(followingKeys[i].transform.position, delayedPosition, ref currentVelocity, smoothTime);

                // Atualiza a posição da chave
                followingKeys[i].transform.position = smoothedPosition;
            }
        }
    }

    public void AddKey(Key key)
    {
        followingKeys.Add(key);
    }
    public void RemoveKey(Key key)
    {
        followingKeys.Remove(key);
    }
}

