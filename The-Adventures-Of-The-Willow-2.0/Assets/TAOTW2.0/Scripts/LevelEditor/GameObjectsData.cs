using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameObjectsData", menuName = "LevelEditor/GameObjectsData")]
public class GameObjectsData : ScriptableObject
{
    [System.Serializable]
    public class GameObjectsInfo
    {
        public string GameObjectName;
        public GameObject prefab;
    }

    [System.Serializable]
    public class GameObjectCategory
    {
        public string GameObjectsCategoryName;
        public List<GameObjectsInfo> GameObjects;
    }

    public List<GameObjectCategory> categories;
}
