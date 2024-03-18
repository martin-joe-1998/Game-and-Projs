using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float totalTime = 60.0f; // ��ʱ�䣨�룩
    private float elapsedTime = 0.0f; // �ѹ�ʱ��
    private int lastDisplayedSecond = 0; // �ϴ���ʾ��������
    private bool isTimerRunning = false; // ��ʱ���Ƿ�������
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
            elapsedTime += Time.deltaTime; // �����ѹ�ʱ��
            
            int currentSecond = Mathf.FloorToInt(elapsedTime);
            currentTime = currentSecond;
            if (currentSecond > lastDisplayedSecond) {
                lastDisplayedSecond = currentSecond;
                //Debug.Log("Time elapsed: " + currentSecond + " seconds");
            }
            
            if (Input.anyKeyDown) {        // ����κΰ��������£�������Timer
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
