using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GemCollect : MonoBehaviour
{
    public static GemCollect instance;
    public TextMeshProUGUI TXTGems;
    public int Gem;
	
    void Start()
    {
		//DataPersistenceManager.instance.LoadGame();
        TXTGems.text = Gem.ToString();
        if (instance == null)
        {
            instance = this;
        }

    }

    public void ChangeGem(int GemValue)
    {
        Gem += GemValue;
        TXTGems.text = Gem.ToString();
		//DataPersistenceManager.instance.SaveGame();
    }

    public void ChangeMinusGem(int GemValue)
    {
        Gem -= GemValue;
        TXTGems.text = Gem.ToString();
		//DataPersistenceManager.instance.SaveGame();
    }


}
