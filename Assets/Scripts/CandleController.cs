using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleController : MonoBehaviour
{
    [Header("Effects References")]
    public GameObject flameObject; // 여기에 VFX_Candle_Flame_01 연결
    public GameObject smokeObject; // 여기에 VFX_Candle_Smoke_01 연결
    public bool isFire;

    void Start()
    {
        flameObject.SetActive(false);
        smokeObject.SetActive(false);
        isFire = false;
    }
    
    public void IgniteCandle()
    {
        flameObject.SetActive(true);
        smokeObject.SetActive(true);
        isFire = true;
    }

    // 촛불 끄는 함수 예시
    public void ExtinguishCandle()
    {
        flameObject.SetActive(false);
        smokeObject.SetActive(false);
        isFire = false;
    }
}
