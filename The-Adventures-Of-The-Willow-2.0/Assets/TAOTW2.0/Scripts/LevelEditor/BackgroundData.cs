using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundData", menuName = "LevelEditor/Background Data", order = 1)]
public class BackgroundData : ScriptableObject
{
    public List<BiomeCategory> biomeCategories = new List<BiomeCategory>();

    [System.Serializable]
    public class Background
    {
        public string backgroundName;
        public GameObject backgroundPrefab;
        public Sprite backgroundSprite; // Adicionando a variável do sprite da música
        // Outras informações relevantes sobre a música, como o AudioClip, volume, etc.
    }

    [System.Serializable]
    public class BiomeCategory
    {
        public string biomeName;
        public Sprite biomeSprite; // Adicionando a variável do sprite do bioma
        public List<Background> BackgroundList = new List<Background>();
    }
}
