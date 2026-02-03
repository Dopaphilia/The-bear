using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NewsManager : MonoBehaviour
{
    public Image articleDisplay; // 기사 이미지가 표시될 곳
    public List<Sprite> newsSprites; // 뉴스 이미지 리스트 (인스펙터에서 등록)
    public Text pageText; // "1 / 10" 처럼 표시할 텍스트

    private int currentIndex = 0;

    void Start()
    {
        UpdateUI();
    }

    // 다음 페이지로 (마지막 페이지에서 누르면 첫 번째 사진으로 순환)
    public void NextPage()
    {
        if (currentIndex < newsSprites.Count - 1)
        {
            currentIndex++;
        }
        else
        {
            currentIndex = 0; // 마지막이면 0번(첫 번째)으로
        }
        UpdateUI();
    }

    // 이전 페이지로 (첫 번째 페이지에서 누르면 마지막 사진으로 순환)
    public void PrevPage()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
        }
        else
        {
            currentIndex = newsSprites.Count - 1; // 첫 번째면 마지막 번호로
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        if (newsSprites.Count > 0 && articleDisplay != null)
        {
            articleDisplay.sprite = newsSprites[currentIndex];
            if (pageText != null)
            {
                pageText.text = (currentIndex + 1) + " / " + newsSprites.Count;
            }
        }
    }
}