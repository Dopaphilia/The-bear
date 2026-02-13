using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 어디서든 GameManager.Instance로 접근할 수 있게 만드는 '싱글톤' 패턴
    public static GameManager Instance;

    [Header("Game Settings")]
    public int currentDay = 1; // 현재 날짜 (1일차부터 시작)

    void Awake()
    {
        // 게임 시작 시 이 스크립트가 단 하나만 존재하도록 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음 (옵션)
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 날짜를 하루 증가시키고, 화면에 띄울 텍스트를 반환하는 함수
    public string GetNextDayText()
    {
        currentDay++; // 날짜 1 증가 (예: 1 -> 2)
        return "DAY " + currentDay; // "DAY 2" 리턴
    }
}