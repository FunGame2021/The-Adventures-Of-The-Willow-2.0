using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SectorManager : MonoBehaviour
{
    public static SectorManager instance;
    public Transform sectorList; // Refer�ncia ao objeto que cont�m a lista de setores
    public GameObject sectorButtonPrefab; // Prefab de bot�o de setor
    public int maxSectors = 5; // N�mero m�ximo de setores permitidos
    public List<string> sectorNames = new List<string>(); // Lista de nomes de setores

    private string sectorToDelete; // Vari�vel tempor�ria para armazenar o setor a ser exclu�do

    public GameObject SectorPanel;
    public GameObject deleteSectorPanelWarn;
    public GameObject deleteSectorPanelWarnSector1;
    public string currentSectorName;

    public void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

    }

    //Corrijir isto
    public void CreateSector()
    {
        // Verificar se j� atingiu o limite m�ximo de setores
        if (sectorNames.Count >= maxSectors)
        {
            Debug.LogWarning("Limite m�ximo de setores atingido.");
            return;
        }

        // Encontre o pr�ximo n�mero de setor dispon�vel
        int newSectorNumber = 1;
        string newSectorName = "";

        while (true)
        {
            newSectorName = "Sector" + newSectorNumber;

            // Verifique se o nome do setor j� existe na lista de nomes de setores
            if (!sectorNames.Contains(newSectorName))
            {
                // Nome �nico encontrado, saia do loop while
                break;
            }

            newSectorNumber++; // Se o nome j� existe, tente o pr�ximo n�mero
        }

        // Crie um bot�o apenas para o novo setor e configure-o conforme necess�rio
        GameObject sectorButton = Instantiate(sectorButtonPrefab, sectorList);
        TextMeshProUGUI buttonText = sectorButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = newSectorName;

        // Adicione um ouvinte de clique ao bot�o "SectorButton" para definir currentSectorName com o nome do setor
        Button sectorButtonComponent = sectorButton.transform.Find("SectorButton").GetComponent<Button>();
        sectorButtonComponent.onClick.AddListener(() => SetCurrentSector(newSectorName));

        // Adicione um ouvinte de clique ao bot�o "DeleteSectorButton" para excluir o setor correspondente
        Button deleteButton = sectorButton.transform.Find("DeleteSectorButton").GetComponent<Button>();
        deleteButton.onClick.AddListener(() => DeleteSector(newSectorName));

        // Agora, ap�s criar o bot�o com sucesso, adicione o novo setor � lista
        sectorNames.Add(newSectorName);
        currentSectorName = newSectorName;

        LevelEditorManager.instance.SaveLevel();
            
    }

    public void AddSectorList(string sectorName)
    {
        // Verifique se o setor j� est� na lista
        if (!sectorNames.Contains(sectorName))
        {
            // Adicione o novo setor � lista de nomes de setores
            sectorNames.Add(sectorName);

            // Classifique a lista de nomes de setores em ordem alfab�tica
            sectorNames.Sort();
        }
    }


    public void DeleteSector(string sectorName)
    {
        // Verificar se o setor a ser exclu�do n�o � o setor principal (Sector1)
        if (sectorName != "Sector1")
        {
            // Ativar o painel de confirma��o e definir o setor a ser exclu�do
            sectorToDelete = sectorName; 
            sectorNames.Sort();
            Debug.Log(sectorToDelete);
            deleteSectorPanelWarn.SetActive(true);
        }
        else
        {
            deleteSectorPanelWarnSector1.SetActive(true);
            Debug.LogWarning("N�o � poss�vel excluir o setor principal.");
        }
    }

    public void ConfirmDeleteSector()
    {
        if (!string.IsNullOrEmpty(sectorToDelete))
        {
            // Remover o setor da lista
            sectorNames.Remove(sectorToDelete);
            Debug.Log("apagado");
            LevelEditorManager.instance.DeleteSector(sectorToDelete, WorldManager.instance.currentWorldName, WorldManager.instance.currentLevelName);

            sectorToDelete = null;

            SetDefaultSector("Sector1");


        }
        deleteSectorPanelWarn.SetActive(false);

    }


    public void SetDefaultSector(string sectorName)
    {
        currentSectorName = sectorName; 
        LevelEditorManager.instance.LoadLevel(WorldManager.instance.currentWorldName, WorldManager.instance.currentLevelName, currentSectorName);

    }

    public void ClearSectorButtons()
    {
        if (sectorList != null)
        {
            // Limpa a lista de setores na interface
            foreach (Transform child in sectorList)
            {
                Destroy(child.gameObject);
            }
        }
    }


    public void CreateSectorButton(string sectorName)
    {
        // Crie um bot�o para o setor e configure-o conforme necess�rio
        GameObject sectorButton = Instantiate(sectorButtonPrefab, sectorList);
        TextMeshProUGUI buttonText = sectorButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = sectorName;

        // Adicione um ouvinte de clique ao bot�o "SectorButton" para definir currentSectorName com o nome do setor
        Button sectorButtonComponent = sectorButton.transform.Find("SectorButton").GetComponent<Button>();
        sectorButtonComponent.onClick.AddListener(() => SetCurrentSector(sectorName));

        // Adicione um ouvinte de clique ao bot�o "DeleteSectorButton" para excluir o setor correspondente
        Button deleteButton = sectorButton.transform.Find("DeleteSectorButton").GetComponent<Button>();
        deleteButton.onClick.AddListener(() => DeleteSector(sectorName));

        // Classifique a lista de nomes de setores em ordem alfab�tica
        sectorNames.Sort();
    }

    public void SetCurrentSector(string sectorName)
    {
        currentSectorName = sectorName;
        LevelEditorManager.instance.LoadLevel(WorldManager.instance.currentWorldName, WorldManager.instance.currentLevelName, currentSectorName);
        SectorPanel.SetActive(false);
    }
}
