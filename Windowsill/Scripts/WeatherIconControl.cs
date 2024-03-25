using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class WeatherIconControl : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private WeatherInfo weatherInfo;
    private Image imageComponent;
    
    //private int SpriteIndex = 0;
    private WeatherData weatherData;
    private string weatherID = "0";
    // Defualt UI background color
    static float backCol = 190 / 255;
    private Color backgroundColor = new Color(backCol, backCol, backCol, 54 / 255);

    DateTime currentTime = DateTime.Now;

    void Awake()
    {
        imageComponent = GetComponent<Image>();

        imageComponent.sprite = null;
        
        if (sprites.Length > 0) { // sprite list is loaded normally
            imageComponent.color = backgroundColor;
        } else { // sprite list is not loaded normally
            imageComponent.color = Color.white;
        }
    }

    // used to detected weather id's changes
    private string previousID;

    void Update()
    {
        // update weather data
        weatherData = weatherInfo.GetWeatherData();

        if (weatherData != null) {
            weatherID = weatherData.weather[0].id;           
        }
        // if weather id is changed, update icon
        if (previousID != weatherID) {
            //Debug.Log("weatherID is " + weatherID);
            UpdateWeatherIcon();
        }

        previousID = weatherID;
    }

    void UpdateWeatherIcon()
    {
        imageComponent.color = Color.white;
        // update hour in 24h format
        int hour = currentTime.Hour;

        // change sprite icon by id code and current hour(day or night)
        switch (weatherID) {
            case "200":
            case "201":
            case "202":
            case "210":
            case "211":
            case "212":
            case "221":
            case "230":
            case "231":
            case "232":
                if (hour >= 19 || hour <= 5) {
                    // 11n
                    imageComponent.sprite = sprites[13];
                } else {
                    // 11d
                    imageComponent.sprite = sprites[12];
                }   
                break;
            case "300":
            case "301":
            case "302":
            case "310":
            case "311":
            case "312":
            case "313":
            case "314":
            case "321":
            case "520":
            case "521":
            case "522":
            case "531":
                if (hour >= 19 || hour <= 5) {
                    // 09n
                    imageComponent.sprite = sprites[9];
                } else {
                    // 09d
                    imageComponent.sprite = sprites[8];
                }    
                break;
            case "500":
            case "501":
            case "502":
            case "503":
            case "504":
                if (hour >= 19 || hour <= 5) {
                    // 10n
                    imageComponent.sprite = sprites[11];
                } else {
                    // 10d
                    imageComponent.sprite = sprites[10];
                }
                break;
            case "511":
            case "600":
            case "601":
            case "602":
            case "611":
            case "612":
            case "613":
            case "615":
            case "616":
            case "620":
            case "621":
            case "622":
                if (hour >= 19 || hour <= 5) {
                    // 13n
                    imageComponent.sprite = sprites[15];
                } else {
                    // 13d
                    imageComponent.sprite = sprites[14];
                }     
                break;
            case "701":
            case "711":
            case "721":
            case "731":
            case "741":
            case "751":
            case "761":
            case "762":
            case "771":
            case "781":
                if (hour >= 19 || hour <= 5) {
                    // 50n
                    imageComponent.sprite = sprites[17];
                } else {
                    // 50d
                    imageComponent.sprite = sprites[16];
                }
                break;
            case "800":
                if (hour >= 19 || hour <= 5) {
                    // 01n
                    imageComponent.sprite = sprites[1];
                } else {
                    // 01d
                    imageComponent.sprite = sprites[0];
                }
                break;
            case "801":
                if (hour >= 19 || hour <= 5) {
                    // 02n
                    imageComponent.sprite = sprites[3];
                } else {
                    // 02d
                    imageComponent.sprite = sprites[2];
                }
                break;
            case "802":
                if (hour >= 19 || hour <= 5) {
                    // 03n
                    imageComponent.sprite = sprites[5];
                } else {
                    // 03d
                    imageComponent.sprite = sprites[4];
                }
                break;
            case "803":
            case "804":
                if (hour >= 19 || hour <= 5) {
                    // 04n
                    imageComponent.sprite = sprites[7];
                } else {
                    // 04d
                    imageComponent.sprite = sprites[6];
                }
                break;
        }
    }
}
