using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParticleTypesData", menuName = "LevelEditor/ParticleTypesData")]
public class ParticleTypes : ScriptableObject
{
    [System.Serializable]
    public class ParticleTypesInfo
    {
        public string ParticleTypesName;
        public GameObject prefab;
    }

    [System.Serializable]
    public class ParticleTypesCategory
    {
        public string particleTypesCategoryName;
        public List<ParticleTypesInfo> ParticleTypes;
    }

    public List<ParticleTypesCategory> categories;
}