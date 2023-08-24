using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ObjectsData", menuName = "LevelEditor/ObjectsData")]
public class ObjectsData : ScriptableObject
{
    [System.Serializable]
    public class ObjectsInfo
    {
        public string ObjectName;
        public ObjectType objectType;
        public GameObject prefab;
    }

    [System.Serializable]
    public class ObjectCategory
    {
        public string categoryName;
        public List<ObjectsInfo> Objects;
    }

    public List<ObjectCategory> categories;
}
public enum ObjectType
{
    Normal,
    Moving,
}
