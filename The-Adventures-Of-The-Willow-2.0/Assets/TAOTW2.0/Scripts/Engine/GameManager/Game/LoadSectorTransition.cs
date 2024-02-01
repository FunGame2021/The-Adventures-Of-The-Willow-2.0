using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSectorTransition : MonoBehaviour
{
    public static LoadSectorTransition instance;
    private GameObject Player;

    private void Start()
    {
        if(LoadSectorTransition.instance == null)
        {
            instance = this;
        }
        Player = GameObject.FindGameObjectWithTag("Player");
    }
    public void sectorCloseDoorTransition(string SectorName, string positionPoint)
    {
        ScreenAspectRatio.instance.CloseTransition();
        if (LoadPlayLevel.instance != null)
        {
            LoadPlayLevel.instance.ActiveSector(SectorName);
        }
        else if (playLevel.instance != null)
        {
            playLevel.instance.ActiveSector(SectorName);
        }
        StartCoroutine(ToFindDoorPoint(positionPoint));
    }
    IEnumerator ToFindDoorPoint(string positionPoint)
    {
        yield return new WaitForSeconds(3f);
        // Encontrar todos os objetos com a tag "Door" no setor específico
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");

        // Filtrar os objetos para encontrar aquele que tem o mesmo nome que positionPoint
        foreach (GameObject doorObject in doors)
        {
            Door doorScript = doorObject.GetComponent<Door>();

            if (doorScript != null && doorScript.DoorID == positionPoint)
            {
                // Teleportar o jogador para o ponto encontrado
                Player.transform.position = doorObject.transform.position;
                StartCoroutine(ToLoad());
                break; // Saia do loop assim que encontrar o objeto desejado
            }
        }
    }
    public void sectorCloseTransition(string SectorName, string positionPoint)
    {
        ScreenAspectRatio.instance.CloseTransition();
        if(LoadPlayLevel.instance != null)
        {
            LoadPlayLevel.instance.ActiveSector(SectorName);
        }
        else if(playLevel.instance != null)
        {
            playLevel.instance.ActiveSector(SectorName);
        }
        StartCoroutine(ToFindSpawnPoint(positionPoint));
    }
    IEnumerator ToFindSpawnPoint(string positionPoint)
    {
        yield return new WaitForSeconds(3f);
        // Encontrar todos os objetos com a tag "SpawnPoint" no setor específico
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        // Filtrar os objetos para encontrar o que tem o mesmo nome que positionPoint
        GameObject spawnPoint = Array.Find(spawnPoints, point => point.name == positionPoint);

        if (spawnPoint != null)
        {
            // Teleportar o jogador para o SpawnPoint encontrado
            Player.transform.position = spawnPoint.transform.position;
            StartCoroutine(ToLoad());
        }
    }
    IEnumerator ToLoad()
    {
        yield return new WaitForSeconds(3f);
        sectorOpenTransition();

    }
    public void sectorOpenTransition()
    {
        ScreenAspectRatio.instance.OpenTransition();
    }
}
