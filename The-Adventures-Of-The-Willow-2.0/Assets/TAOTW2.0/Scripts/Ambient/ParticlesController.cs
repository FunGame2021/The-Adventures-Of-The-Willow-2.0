using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesController : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;
    public string particleIDName;
    public bool isToPlay;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Encontre todos os objetos com a tag "Particles"
            GameObject[] objectsWithMatchingTag = GameObject.FindGameObjectsWithTag("Particles");

            foreach (GameObject obj in objectsWithMatchingTag)
            {
                ParticleSystem particleSystem = obj.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    string objectName = obj.name.Replace("(Clone)", "");

                    // Verifique o nome do objeto
                    if (objectName == particleIDName)
                    {
                        if (isToPlay)
                        {
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
