using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Door : MonoBehaviour
{
    public string DoorID;
    public string SecondDoorID;
    public bool WithKey;
    public bool toSector;
    public string SectorName;
    public string PositionPoint;

    private KeyFollow thePlayer;

    [SerializeField] private GameObject doorOpenSprite;

    [SerializeField] private bool doorOpen, waitingToOpen;

    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private Animator AnimKeyOpen;
    [SerializeField] private float seconds = 3f;
    private bool keyOpened;
    private bool isOnDoor;
    private bool isTeleporting = false;

    void Update()
    {
        if (waitingToOpen && !doorOpen && isOnDoor)
        {
            waitingToOpen = false;
            doorOpen = true;
            StartCoroutine(Opened());

            // Verificar WithKey
            if (WithKey)
            {
                List<Key> keysToRemove = new List<Key>();

                AnimKeyOpen.SetTrigger("OpenDoor");
                collectEffect.Play();

                foreach (Key key in thePlayer.followingKeys)
                {
                    // Comparar DoorID com a keyID
                    if (key.keyID == DoorID)
                    {
                        keyOpened = true;
                        key.gameObject.SetActive(false);
                        thePlayer.RemoveKey(key);
                        keysToRemove.Add(key);
                        key.keyDoorOpened = true;
                    }
                }


                foreach (Key key in keysToRemove)
                {
                    thePlayer.followingKeys.Remove(key);
                }
            }
            else
            {
                keyOpened = true;
            }
        }

        if (keyOpened && doorOpen && PlayerController.instance.moveInputUp > 0.1f && isOnDoor && !isTeleporting)
        {
            isTeleporting = true;

            if (!string.IsNullOrEmpty(SecondDoorID))
            {
                    // Verificar toSector
                    if (toSector)
                    {
                        // Se toSector for verdadeiro, mover para a posição SectorName
                        // Aqui você pode implementar a lógica específica para mover para o setor
                        MoveToDoorSector(SectorName, SecondDoorID);

                    }
                    else
                    {
                        // Se toSector for falso, mover para a posição PositionPoint
                        // Aqui você pode implementar a lógica específica para mover para o ponto de posição
                        MoveToDoorPosition(SecondDoorID);

                    }
            }
            else
            {
                // Verificar toSector
                if (toSector)
                {
                    // Se toSector for verdadeiro, mover para a posição SectorName
                    // Aqui você pode implementar a lógica específica para mover para o setor
                    MoveToSector(SectorName, PositionPoint);
                }
                else
                {
                    // Se toSector for falso, mover para a posição PositionPoint
                    // Aqui você pode implementar a lógica específica para mover para o ponto de posição
                    MoveToPosition(PositionPoint);
                }

            }

        }
    }
    void MoveToSector(string sectorName, string positionPoint)
    {
        isTeleporting = false;
        LoadSectorTransition.instance.sectorCloseTransition(sectorName, positionPoint);
    }
   

    void MoveToPosition(string positionPoint)
    {
        // Encontrar todos os objetos com a tag "SpawnPoint"
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        // Filtrar os objetos para encontrar o que tem o mesmo nome que positionPoint
        GameObject spawnPoint = Array.Find(spawnPoints, point => point.name == positionPoint);

        if (spawnPoint != null)
        {
            // Teleportar o jogador para o SpawnPoint encontrado
            thePlayer.transform.position = spawnPoint.transform.position;
            isTeleporting = false;
        }
    }


    void MoveToDoorSector(string sectorName, string positionPoint)
    {
        isTeleporting = false; 
        LoadSectorTransition.instance.sectorCloseDoorTransition(sectorName, positionPoint);
    }


    void MoveToDoorPosition(string positionPoint)
    {
        // Encontrar todos os objetos com a tag "Door"
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");

        // Filtrar os objetos para encontrar aquele que tem o mesmo nome que positionPoint
        foreach (GameObject doorObject in doors)
        {
            Door doorScript = doorObject.GetComponent<Door>();

            if (doorScript != null && doorScript.DoorID == positionPoint)
            {
                // Teleportar o jogador para o ponto encontrado
                thePlayer.transform.position = doorObject.transform.position;
                isTeleporting = false;
                break; // Saia do loop assim que encontrar o objeto desejado
            }
        }
    }



    IEnumerator Opened()
    {
        yield return new WaitForSeconds(seconds);
        doorOpenSprite.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            thePlayer = FindObjectOfType<KeyFollow>();
            waitingToOpen = true;
            isOnDoor = true;
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isOnDoor = true;
            waitingToOpen = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isOnDoor = false;
        }
    }

}