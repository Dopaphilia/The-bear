using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] ParticleSystem waterParticle;

    private bool isWaterRunning = false;
    public void ToggleWater()
    {
        isWaterRunning = !isWaterRunning;

        if (isWaterRunning)
        {
            waterParticle.Play(); // 물 틀기
        }
        else
        {
            waterParticle.Stop(); // 물 끄기 (이미 나온 물은 자연스럽게 떨어짐)
        }
    }
}