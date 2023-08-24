using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DecorData", menuName = "LevelEditor/DecorData")]
public class DecorData : ScriptableObject
{
    [System.Serializable]
    public class DecorInfo
    {
        public string decorName;
        public GameObject prefab;
    }

    [System.Serializable]
    public class DecorCategory
    {
        public string categoryName;
        public List<DecorInfo> decorations;
    }

    public List<DecorCategory> categories;
}
