using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GalleryManager : MonoBehaviour
{
    public Image galleryDisplay; // 사진이 표시될 이미지
    public List<Sprite> gallerySprites; // 갤러리에 넣을 이미지들
    public Text pageText; // "1 / 5" 표시용 텍스트

    private int currentIndex = 0;

    void Start()
    {
        UpdateUI();
    }

    // 다음 사진 (마지막에서 누르면 첫 번째로 순환)
    public void NextImage()
    {
        if (gallerySprites.Count == 0) return;

        if (currentIndex < gallerySprites.Count - 1)
        {
            currentIndex++;
        }
        else
        {
            currentIndex = 0;
        }
        UpdateUI();
    }

    // 이전 사진 (첫 번째에서 누르면 마지막으로 순환)
    public void PrevImage()
    {
        if (gallerySprites.Count == 0) return;

        if (currentIndex > 0)
        {
            currentIndex--;
        }
        else
        {
            currentIndex = gallerySprites.Count - 1;
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        if (gallerySprites.Count > 0 && galleryDisplay != null)
        {
            galleryDisplay.sprite = gallerySprites[currentIndex];
            if (pageText != null)
            {
                pageText.text = (currentIndex + 1) + " / " + gallerySprites.Count;
            }
        }
    }
}