using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class PanelController : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public GameObject[] panels;

    private Dictionary<int, GameObject> panelDictionary = new Dictionary<int, GameObject>();

    private void Start()
    {
        dropdown.ClearOptions();
        List<string> dropdownOptions = new List<string>();

        for (int i = 0; i < panels.Length; i++)
        {
            string optionName = GetOptionName(i); // Obtenha o nome da opção com base no índice
            dropdownOptions.Add(optionName);
            panelDictionary.Add(i, panels[i]);
        }

        dropdown.AddOptions(dropdownOptions);
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnDropdownValueChanged(int index)
    {
        // Desativa todos os painéis
        DeactivateAllPanels();

        // Verifica se o índice selecionado está dentro do intervalo válido
        if (panelDictionary.ContainsKey(index))
        {
            // Obtém o painel correspondente ao índice selecionado
            GameObject selectedPanel = panelDictionary[index];
            selectedPanel.SetActive(true);
        }
    }

    private void DeactivateAllPanels()
    {
        // Desativa todos os painéis
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
    }

    private string GetOptionName(int index)
    {
        // Retorne o nome da opção com base no índice
        // Personalize esta função de acordo com seus nomes desejados
        switch (index)
        {
            case 0:
                return "Enemies";
            case 1:
                return "Objects";
            case 2:
                return "Platforms";
            case 3:
                return "Decorations";
            case 4:
                return "Interactive";
            case 5:
                return "GameObjects";
            default:
                return "Opção Desconhecida";
        }
    }
}
