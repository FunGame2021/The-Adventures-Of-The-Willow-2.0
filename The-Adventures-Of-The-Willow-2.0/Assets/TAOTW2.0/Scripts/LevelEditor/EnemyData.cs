using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyData", menuName = "LevelEditor/EnemyData")]
public class EnemyData : ScriptableObject
{
    [System.Serializable]
    public class EnemyInfo
    {
        public string enemyName;
        public GameObject prefab;
    }

    [System.Serializable]
    public class EnemyCategory
    {
        public string categoryName;
        public List<EnemyInfo> enemies;
    }

    public List<EnemyCategory> categories;
}
