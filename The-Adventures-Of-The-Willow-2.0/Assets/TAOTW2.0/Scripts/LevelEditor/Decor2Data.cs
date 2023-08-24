using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Decor2Data", menuName = "LevelEditor/Decor2Data")]
public class Decor2Data : ScriptableObject
{
    [System.Serializable]
    public class Decor2Info
    {
        public string decor2Name;
        public GameObject prefab;
    }

    [System.Serializable]
    public class Decor2Category
    {
        public string categoryName;
        public List<Decor2Info> decorations;
    }

    public List<Decor2Category> categories;
}
