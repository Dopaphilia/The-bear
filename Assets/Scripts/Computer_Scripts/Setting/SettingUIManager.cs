using UnityEngine;

public class SettingUIManager : MonoBehaviour
{
    [Header("Color Filter Objects")]
    public GameObject redFilter;
    public GameObject orangeFilter;
    public GameObject yellowFilter;
    public GameObject blueFilter;

    // 게임 시작 시 모든 필터를 끄고 시작하고 싶다면 추가
    private void Start()
    {
        CloseAllFilters();
    }

    public void CloseAllFilters()
    {
        // 오브젝트가 할당되어 있을 때만 끄도록 체크
        if (redFilter) redFilter.SetActive(false);
        if (orangeFilter) orangeFilter.SetActive(false);
        if (yellowFilter) yellowFilter.SetActive(false);
        if (blueFilter) blueFilter.SetActive(false);
    }

    public void OnClickRedButton()
    {
        CloseAllFilters();
        if (redFilter) redFilter.SetActive(true); // 필터가 연결되어 있을 때만 활성화
    }

    public void OnClickOrangeButton()
    {
        CloseAllFilters();
        if (orangeFilter) orangeFilter.SetActive(true);
    }

    public void OnClickYellowButton()
    {
        CloseAllFilters();
        if (yellowFilter) yellowFilter.SetActive(true);
    }

    public void OnClickBlueButton()
    {
        CloseAllFilters();
        if (blueFilter) blueFilter.SetActive(true);
    }
}