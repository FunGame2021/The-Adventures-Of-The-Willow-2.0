using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeatherData", menuName = "LevelEditor/Weather Data")]
public class WeatherData : ScriptableObject
{
    public List<WeatherCategory> WeatherCategories = new List<WeatherCategory>();

    [System.Serializable]
    public class Weather
    {
        public string WeatherName;
        public GameObject WeatherPrefab;
    }

    [System.Serializable]
    public class WeatherCategory
    {
        public string WeatherNameCategory;
        public List<Weather> WeatherList = new List<Weather>();
    }
}
