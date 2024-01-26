using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TimeWeatherData", menuName = "LevelEditor/Time Weather Data")]
public class TimeWeatherData : ScriptableObject
{
    public List<TimeWeatherCategory> timeWeatherCategories = new List<TimeWeatherCategory>();

    [System.Serializable]
    public class TimeWeather
    {
        public string TimeWeatherName;
        public GameObject TimeWeatherPrefab;
    }

    [System.Serializable]
    public class TimeWeatherCategory
    {
        public string timeWeatherNameCategory;
        public List<TimeWeather> TimeWeatherList = new List<TimeWeather>();
    }
}
