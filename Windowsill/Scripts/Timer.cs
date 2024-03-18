using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float totalTime = 60.0f; // 总时间（秒）
    private float elapsedTime = 0.0f; // 已过时间
    private int lastDisplayedSecond = 0; // 上次显示的整数秒
    private bool isTimerRunning = false; // 计时器是否在运行
    public int currentTime = 0;

    private void Awake()
    {
        if (!isTimerRunning) {
            StartTimer();
        }
    }

    private void Update()
    {
        if (isTimerRunning) {
            elapsedTime += Time.deltaTime; // 更新已过时间
            
            int currentSecond = Mathf.FloorToInt(elapsedTime);
            currentTime = currentSecond;
            if (currentSecond > lastDisplayedSecond) {
                lastDisplayedSecond = currentSecond;
                //Debug.Log("Time elapsed: " + currentSecond + " seconds");
            }
            
            if (Input.anyKeyDown) {        // 如果任何按键被按下，则重置Timer
                //Debug.Log("Key Down!");
                ResetTimer();
            } else if (elapsedTime >= totalTime) {
                //Debug.Log("Time's up!");
                StopTimer();
                ResetTimer();
            }
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
        elapsedTime = 0.0f;
        lastDisplayedSecond = 0;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0.0f;
        lastDisplayedSecond = 0;
    }

    public bool IsTimerRunning() { return isTimerRunning; }
}
