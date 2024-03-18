using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

// get current weather from OpenWeatherAPI
public class WeatherInfo : MonoBehaviour
{
    private string apiKey = "9c79b2e114616ac9fb29e368198e587c";
    // API to get weather info
    private string apiUrl = "https://api.openweathermap.org/data/2.5/weather";

    // A certain position for testing. Will use GPS in the future 
    private float lat = 35.627578f;
    private float lon = 140.104024f;

    WeatherData weatherData;

    void Start()
    {
        // Construct a complete API url
        string url = $"{apiUrl}?lat={lat}&lon={lon}&appid={apiKey}";

        // Send API request
        StartCoroutine(GetWeatherData(url));
    }

    IEnumerator GetWeatherData(string url)
    {
        // Send Get request
        using (UnityWebRequest request = UnityWebRequest.Get(url)) {
            // Wait reply of request
            yield return request.SendWebRequest();

            // Check if something wrong happen
            if (request.result != UnityWebRequest.Result.Success) {
                Debug.LogError("Error: " + request.error);
            } else {
                // Parse JSON's response data
                string jsonResult = request.downloadHandler.text;
                weatherData = JsonUtility.FromJson<WeatherData>(jsonResult);

                // Show Weather Data
                //DisplayWeather(weatherData);
            }
        }
    }

    void DisplayWeather(WeatherData weatherData)
    {
        // 在这里你可以从WeatherData对象中提取所需的天气信息并以文字形式显示在游戏中
        Debug.Log("Weather ID: " + weatherData.weather[0].id);
        Debug.Log("Current Weather： " + weatherData.weather[0].description);
        Debug.Log("Current Temperature： " + weatherData.main.temp);
    }

    public WeatherData GetWeatherData() { return weatherData; }
    public bool IsWeatherDataNull() { return weatherData == null; }
}

// 天气数据模型（根据OpenWeatherMap的API响应格式定义）
[Serializable]
public class WeatherData {
    public Weather[] weather;
    public MainData main;
}

[Serializable]
public class Weather {
    public string description;
    public string id;
}

[Serializable]
public class MainData {
    public float temp;
}
