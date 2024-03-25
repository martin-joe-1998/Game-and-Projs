using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

// get current weather from OpenWeatherAPI
public class WeatherInfo : MonoBehaviour
{
    public GetIPAddress getIP;

    // API key of personal account!!
    private string apiKey = "9c79b2e114616ac9fb29e368198e587c";
    // API to get weather info
    private string apiUrl = "https://api.openweathermap.org/data/2.5/weather";

    // A certain position for testing. Will use GPS in the future 
    private float lat;
    private float lon;

    WeatherData weatherData;

    void Start()
    {
        // GetLocationInfo() will be executed at Awake() in getIP
        while (getIP.GetLocationInfo() != (0, 0)) {
            // Get lat and lon info from GetIPAddress.cs
            (lat, lon) = getIP.GetLocationInfo();

            // Construct a complete API url
            string url = $"{apiUrl}?lat={lat}&lon={lon}&appid={apiKey}";

            // Send API request
            StartCoroutine(GetWeatherData(url));
            break;
        }
    }

    // Send request to get weather info from url
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
        Debug.Log("Weather ID: " + weatherData.weather[0].id);
        Debug.Log("Current Weather£º " + weatherData.weather[0].description);
        Debug.Log("Current Temperature£º " + weatherData.main.temp);
    }

    public WeatherData GetWeatherData() { return weatherData; }
    public string GetWeatherID() 
    { 
        if (weatherData == null) {
            return "";
        } else {
            return weatherData.weather[0].id; 
        }
    }
    public bool IsWeatherDataNull() { return weatherData == null; }
}

// Data structure for weather info (based on OpenWeather API document)
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
