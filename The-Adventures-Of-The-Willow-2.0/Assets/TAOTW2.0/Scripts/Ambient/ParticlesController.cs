using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesController : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;
    public string particleIDName;
    public bool isToPlay;
    public float timeToPlay;
    private float timeToPlayAgain;
    private bool playied;
    public bool wasWaitTime;

    private void Update()
    {
        if (wasWaitTime)
        {
            if (timeToPlayAgain <= 0)
            {
                playied = false;
            }
            else
            {
                playied = true;
            }
            if (timeToPlayAgain >= 0)
            {
                timeToPlayAgain -= Time.deltaTime;
            }
            if (timeToPlayAgain <= 0)
            {
                timeToPlayAgain = 0;
            }
        }
        else
        {
            playied = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !playied)
        {
            // Encontre todos os objetos com a tag "Particles"
            GameObject[] objectsWithMatchingTag = GameObject.FindGameObjectsWithTag("Particles");

            foreach (GameObject obj in objectsWithMatchingTag)
            {
                ParticleSystem particleSystem = obj.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    // Obtenha as informações do sistema de partículas
                    ParticlesInfo particlesInfo = obj.GetComponent<ParticlesInfo>();

                    // Verifique o nome do objeto
                    if (particlesInfo != null && particlesInfo.thisParticleName == particleIDName)
                    {
                        if (isToPlay)
                        {
                            if (wasWaitTime)
                            {
                                timeToPlayAgain = timeToPlay;
                            }
                            // Reproduza o sistema de partículas
                            particleSystem.Play();
                        }
                        else
                        {
                            // Pare o sistema de partículas
                            particleSystem.Stop();
                        }
                    }
                }
            }
        }
    }

}
