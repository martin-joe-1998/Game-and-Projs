using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering.UI;

public class TimeManager : MonoBehaviour
{
    int currentHour;
    DateTime currentTime;
    WeatherData weatherData;

    LightController lightController;
    WeatherInfo weatherInfo;

    [SerializeField] private GameObject normalWindowGlass;
    [SerializeField] private GameObject rainyWindowGlass;
    [SerializeField] private GameObject lampLightSource;

    void Start()
    {
        // Get current time info and set to variable
        UpdateHour();

        string time = currentTime.ToLongTimeString();
        //Debug.Log("Current Time£º" + time);

        lightController = GameObject.FindObjectOfType<LightController>();
        weatherInfo = GameObject.FindObjectOfType<WeatherInfo>();
    }

    void Update()
    {
        UpdateHour();

        //ControlBuildingLight(currentHour);
        //ControlWindow();
    }

    void ControlBuildingLight(int currentHour)
    {
        if (currentHour >= 18 || currentHour <= 3) {
            lightController.lights = true;
        } else {
            lightController.lights = false;
        }
    }

    void ControlWindow()
    {
        if (!weatherInfo.IsWeatherDataNull()) {
            weatherData = weatherInfo.GetWeatherData();
            string id = weatherData.weather[0].id;
            //id = "500";

            if (normalWindowGlass != null && rainyWindowGlass != null) {
                // '5xx' means rainy weather
                if (id[0] == '5') {
                    normalWindowGlass.SetActive(false);
                    rainyWindowGlass.SetActive(true);
                } else {
                    normalWindowGlass.SetActive(true);
                    rainyWindowGlass.SetActive(false);
                }
            }
        } else {
            //Debug.Log("Weather Data Null.");
        }
    }

    private void UpdateHour()
    {
        currentTime = DateTime.Now;
        currentHour = currentTime.Hour;
    }

    public void ToggleWindow()
    {
        if (normalWindowGlass.activeSelf && !rainyWindowGlass.activeSelf) {
            normalWindowGlass.SetActive(false);
            rainyWindowGlass.SetActive(true);
        } else if (!normalWindowGlass.activeSelf && rainyWindowGlass.activeSelf) {
            normalWindowGlass.SetActive(true);
            rainyWindowGlass.SetActive(false);
        }
    }

    public void ToggleLampLightSource()
    {
        if (lampLightSource.activeSelf) {
            lampLightSource.SetActive(false);
        } else {
            lampLightSource.SetActive(true);
        }
    }
}
