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

    [SerializeField] private bool doorOpen, waitingToOpen;

    [SerializeField] private Animator DoorAnim;

    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private Animator AnimKeyOpen;
    [SerializeField] private float seconds = 3f;
    private bool keyOpened;
    private bool isOnDoor;
    private bool isTeleporting = false;
    private float timeToTeleportAgain = 4f;

    [SerializeField] private GameObject Lock;
    [SerializeField] private Animator LockAnim;
    private bool Locked;
    private bool locksfx;
    private bool Opensfx;
    private float toAgainOpenSfx;
    private float toAgainLockSfx;

    void Start()
    {
        isTeleporting = false;
        if(WithKey && !keyOpened)
        {
            Lock.SetActive(true); 
            Locked = true;
        }
        else
        {
            Lock.SetActive(false);
            Locked = false;
        }
    }
    void Update()
    {
        if(Locked)
        {
            Lock.SetActive(true);
        }
        else
        {
            Lock.SetActive(false);
        }
        if (waitingToOpen && !doorOpen && isOnDoor)
        {
            waitingToOpen = false;

            // Verificar WithKey
            if (WithKey)
            {
                List<Key> keysToRemove = new List<Key>();

                foreach (Key key in thePlayer.followingKeys)
                {
                    // Comparar DoorID com a keyID
                    if (key.keyID == DoorID)
                    {
                        AnimKeyOpen.SetTrigger("OpenDoor");
                        LockAnim.SetTrigger("RemoveLocked");
                        collectEffect.Play();
                        keyOpened = true;
                        key.gameObject.SetActive(false);
                        thePlayer.RemoveKey(key);
                        keysToRemove.Add(key);
                        key.keyDoorOpened = true;
                        Locked = false;
                        doorOpen = true;
                    }
                    else
                    {
                        LockAnim.SetTrigger("Locked");
                        Locked = true;
                        doorOpen = false;
                        if(!locksfx)
                        {
                            AudioManager.instance.PlayOneShot(FMODEvents.instance.Locked, this.transform.position);
                            locksfx = true;
                            toAgainLockSfx = 5f;
                        }
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
                doorOpen = true;
            }
        }

        if (!keyOpened && Locked && !doorOpen && PlayerController.instance != null && PlayerController.instance.moveInputUp > 0.1f && isOnDoor && !isTeleporting)
        {
            LockAnim.SetTrigger("Locked"); 
            if (!locksfx)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.Locked, this.transform.position);
                locksfx = true;
                toAgainLockSfx = 5f;
            }
        }
        if (keyOpened && doorOpen && PlayerController.instance.moveInputUp > 0.1f && isOnDoor && !isTeleporting)
        {
            isTeleporting = true;
            timeToTeleportAgain = 4f;

            DoorAnim.SetTrigger("Open");
            if(!Opensfx)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.OpenDoor, this.transform.position);
                Opensfx = true;
                toAgainOpenSfx = 5f;
            }
            StartCoroutine(Opened());

        }

        if(timeToTeleportAgain >= 0f)
        {
            timeToTeleportAgain -= Time.deltaTime;
        }
        if(timeToTeleportAgain <= 0)
        {
            isTeleporting = false;
        }

        if(toAgainLockSfx >= 0f)
        {
            toAgainLockSfx -= Time.deltaTime;
        }
        if(toAgainLockSfx <= 0f)
        {
            locksfx = false;
        }

        if (toAgainOpenSfx >= 0f)
        {
            toAgainOpenSfx -= Time.deltaTime;
        }
        if (toAgainOpenSfx <= 0f)
        {
            Opensfx = false;
        }
    }

    void MoveToSector(string sectorName, string positionPoint)
    {
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