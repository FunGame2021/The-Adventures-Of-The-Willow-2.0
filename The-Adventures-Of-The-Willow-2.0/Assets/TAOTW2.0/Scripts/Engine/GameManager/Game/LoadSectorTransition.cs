using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSectorTransition : MonoBehaviour
{
    public static LoadSectorTransition instance;
    private GameObject Player;

    private string tempPositionPoint;
    private string tempSectorName;

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
        tempPositionPoint = positionPoint;
        tempSectorName = SectorName;

        ScreenAspectRatio.instance.GetCharacterPosition();
        ScreenAspectRatio.instance.CloseRadiusTransition();
        StartCoroutine(ToEndectorCloseDoorTransition());
    }
    IEnumerator ToEndectorCloseDoorTransition()
    {
        yield return new WaitForSeconds(3f);

        if (LoadPlayLevel.instance != null)
        {
            LoadPlayLevel.instance.ActiveSector(tempSectorName);
        }
        else if (playLevel.instance != null)
        {
            playLevel.instance.ActiveSector(tempSectorName);
        }
        StartCoroutine(ToFindDoorPoint(tempPositionPoint));
    }
    IEnumerator ToFindDoorPoint(string positionPoint)
    {
        yield return new WaitForSeconds(3f);
        // Encontrar todos os objetos com a tag "Door" no setor específico
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");

        Player = GameObject.FindGameObjectWithTag("Player");
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
        tempPositionPoint = positionPoint;
        tempSectorName = SectorName;
        ScreenAspectRatio.instance.GetCharacterPosition();
        ScreenAspectRatio.instance.CloseRadiusTransition();
        StartCoroutine(ToEndsectorCloseTransition());
    }
    IEnumerator ToEndsectorCloseTransition()
    {
        yield return new WaitForSeconds(3f);

        if (LoadPlayLevel.instance != null)
        {
            LoadPlayLevel.instance.ActiveSector(tempSectorName);
        }
        else if (playLevel.instance != null)
        {
            playLevel.instance.ActiveSector(tempSectorName);
        }
        StartCoroutine(ToFindSpawnPoint(tempPositionPoint));
    }
    IEnumerator ToFindSpawnPoint(string positionPoint)
    {
        yield return new WaitForSeconds(3f);
        // Encontrar todos os objetos com a tag "SpawnPoint" no setor específico
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        // Filtrar os objetos para encontrar o que tem o mesmo nome que positionPoint
        GameObject spawnPoint = Array.Find(spawnPoints, point => point.name == positionPoint);

        Player = GameObject.FindGameObjectWithTag("Player");
        if (spawnPoint != null)
        {
            // Verificar se o jogador e spawnPoint não são nulos antes de acessar transform.position
            if (Player != null && spawnPoint != null)
            {
                // Teleportar o jogador para o SpawnPoint encontrado
                Player.transform.position = spawnPoint.transform.position;
                StartCoroutine(ToLoad());
            }
            else
            {
                Debug.LogError("Player or spawnPoint is null."+ positionPoint.ToString() + spawnPoint.ToString());
            }
        }
    }
    IEnumerator ToLoad()
    {
        yield return new WaitForSeconds(3f);
        sectorOpenTransition();

    }
    public void sectorOpenTransition()
    {
        ScreenAspectRatio.instance.GetCharacterPosition();
        ScreenAspectRatio.instance.OpenRadiusTransition();
    }
}
