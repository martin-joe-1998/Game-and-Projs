using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.Networking;
using System;

public class GetIPAddress : MonoBehaviour
{
    private float latitude = 0;
    private float longitude = 0;

    void Awake()
    {
        // Get Latitude and Longitude info by ipinfo API
        StartCoroutine(GetLocationFromIP());
    }

    IEnumerator GetLocationFromIP()
    {
        // IP info API
        string url = "https://ipinfo.io/json";

        // send HTTP GET request
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success) {
            // parse JSON response
            string jsonResponse = www.downloadHandler.text;
            LocationData locationData = JsonUtility.FromJson<LocationData>(jsonResponse);

            // extract latitude and longitude info
            string[] coords = locationData.loc.Split(',');
            latitude = Convert.ToSingle(coords[0]);
            longitude = Convert.ToSingle(coords[1]);

            // print info
            //Debug.Log("Latitude: " + latitude + ", Longitude: " + longitude);
        } else {
            Debug.LogError("Failed to get location data: " + www.error);
        }
    }

    // return latitude and longitude info
    public (float, float) GetLocationInfo()
    {
        return (latitude, longitude);
    }
}

// Data structure used to deserialize JSON responses
[Serializable]
public class LocationData {
    public string loc;
}

