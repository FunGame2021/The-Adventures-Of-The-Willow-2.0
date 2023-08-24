using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicData", menuName = "LevelEditor/Music Data", order = 1)]
public class MusicData : ScriptableObject
{
    public List<BiomeCategory> biomeCategories = new List<BiomeCategory>();

    [System.Serializable]
    public class Music
    {
        public string musicName;
        public int musicID;
        public Sprite musicSprite; // Adicionando a variável do sprite da música
        // Outras informações relevantes sobre a música, como o AudioClip, volume, etc.
    }

    [System.Serializable]
    public class BiomeCategory
    {
        public string biomeName;
        public Sprite biomeSprite; // Adicionando a variável do sprite do bioma
        public List<Music> musicList = new List<Music>();
    }
}
