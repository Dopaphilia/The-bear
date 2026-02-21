using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleController : MonoBehaviour
{
    [Header("Effects References")]
    public GameObject flameObject; // 여기에 VFX_Candle_Flame_01 연결
    public GameObject smokeObject; // 여기에 VFX_Candle_Smoke_01 연결
    public AudioSource candleSound; // 여기에 촛불 소리 오디오 소스 연결
    public bool isFire;

    void Start()
    {
        flameObject.SetActive(false);
        smokeObject.SetActive(false);
        isFire = false;
    }
            // 소리 켜기
    public void PlayCandleSound()
    {
        if (candleSound != null) 
        {
            candleSound.loop = true; // 루프 켜기
            candleSound.Play();
        }
    }

    // 소리 끄기
    public void StopCandleSound()
    {
        if (candleSound != null) 
        {
            candleSound.Stop();
        }
    }
    public void IgniteCandle()
    {
        flameObject.SetActive(true);
        smokeObject.SetActive(true);
        //PlayCandleSound();
        isFire = true;
    }

    // 촛불 끄는 함수
    public void ExtinguishCandle()
    {
        flameObject.SetActive(false);
        smokeObject.SetActive(false);
        //StopCandleSound();
        isFire = false;
    }

}
