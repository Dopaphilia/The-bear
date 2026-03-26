using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SNSLoginManager : MonoBehaviour
{
    [Header("Accounts")]
    public SNSAccountData[] accounts;

    [Header("Login UI")]
    public TMP_InputField idInput;
    public TMP_InputField pwInput;
    public TMP_Text errorText;

    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject homePanel;
    public GameObject profilePanel;
    public GameObject settingPanel;
    public GameObject commonButtons;

    [Header("Optional Home UI")]
    public TMP_Text homeUserNameText;
    public Image homeProfileImage;

    [Header("Optional Profile UI")]
    public TMP_Text profileNameText;
    public Image profileImage;
    public TMP_Text bioText;

    [Header("Profile Post Roots")]
    public GameObject miffyPostsRoot;
    public GameObject borrisPostsRoot;

    [Header("Optional Private Post Groups")]
    public GameObject miffyPrivatePosts;
    public GameObject borrisPrivatePosts;

    private SNSAccountData currentAccount;

    private void Start()
    {
        ShowLoginPanel();
        ClearError();
        HideAllPostGroups();
    }

    public void TryLogin()
    {
        string enteredId = idInput.text.Trim();
        string enteredPw = pwInput.text.Trim();

        if (string.IsNullOrEmpty(enteredId))
        {
            ShowError("아이디를 입력하세요.");
            return;
        }

        if (string.IsNullOrEmpty(enteredPw))
        {
            ShowError("비밀번호를 입력하세요.");
            return;
        }

        foreach (SNSAccountData account in accounts)
        {
            if (account.userId == enteredId && account.password == enteredPw)
            {
                currentAccount = account;
                OnLoginSuccess();
                return;
            }
        }

        ShowError("아이디 또는 비밀번호가 올바르지 않습니다.");
    }

    private void OnLoginSuccess()
    {
        ClearError();
        UpdateUIForCurrentAccount();

        if (loginPanel != null)
            loginPanel.SetActive(false);

        if (homePanel != null)
            homePanel.SetActive(true);

        if (profilePanel != null)
            profilePanel.SetActive(false);

        if (settingPanel != null)
            settingPanel.SetActive(false);

        if (commonButtons != null)
            commonButtons.SetActive(true);
    }

    public void Logout()
    {
        currentAccount = null;

        if (idInput != null)
            idInput.text = "";

        if (pwInput != null)
            pwInput.text = "";

        ClearError();
        HideAllPostGroups();
        ShowLoginPanel();
    }

    public void OpenHome()
    {
        if (homePanel != null)
            homePanel.SetActive(true);

        if (profilePanel != null)
            profilePanel.SetActive(false);

        if (settingPanel != null)
            settingPanel.SetActive(false);
    }

    public void OpenProfile()
    {
        if (currentAccount == null)
            return;

        UpdateUIForCurrentAccount();

        if (homePanel != null)
            homePanel.SetActive(false);

        if (profilePanel != null)
            profilePanel.SetActive(true);

        if (settingPanel != null)
            settingPanel.SetActive(false);
    }

    public void OpenSetting()
    {
        if (currentAccount == null)
            return;

        if (homePanel != null)
            homePanel.SetActive(false);

        if (profilePanel != null)
            profilePanel.SetActive(false);

        if (settingPanel != null)
            settingPanel.SetActive(true);
    }

    private void ShowLoginPanel()
    {
        if (loginPanel != null)
            loginPanel.SetActive(true);

        if (homePanel != null)
            homePanel.SetActive(false);

        if (profilePanel != null)
            profilePanel.SetActive(false);

        if (settingPanel != null)
            settingPanel.SetActive(false);

        if (commonButtons != null)
            commonButtons.SetActive(false);
    }

    private void UpdateUIForCurrentAccount()
    {
        if (currentAccount == null)
            return;

        if (homeUserNameText != null)
            homeUserNameText.text = currentAccount.displayName;

        if (homeProfileImage != null)
            homeProfileImage.sprite = currentAccount.profileImage;

        if (profileNameText != null)
            profileNameText.text = currentAccount.displayName;

        if (profileImage != null)
            profileImage.sprite = currentAccount.profileImage;

        if (bioText != null)
            bioText.text = currentAccount.bio;

        UpdatePostGroups();
    }

    private void UpdatePostGroups()
    {
        HideAllPostGroups();

        if (currentAccount == null)
            return;

        if (currentAccount.userId == "miffy")
        {
            if (miffyPostsRoot != null)
                miffyPostsRoot.SetActive(true);

            if (miffyPrivatePosts != null)
                miffyPrivatePosts.SetActive(true);
        }
        else if (currentAccount.userId == "borris")
        {
            if (borrisPostsRoot != null)
                borrisPostsRoot.SetActive(true);

            if (borrisPrivatePosts != null)
                borrisPrivatePosts.SetActive(true);
        }
    }

    private void HideAllPostGroups()
    {
        if (miffyPostsRoot != null)
            miffyPostsRoot.SetActive(false);

        if (borrisPostsRoot != null)
            borrisPostsRoot.SetActive(false);

        if (miffyPrivatePosts != null)
            miffyPrivatePosts.SetActive(false);

        if (borrisPrivatePosts != null)
            borrisPrivatePosts.SetActive(false);
    }

    private void ShowError(string message)
    {
        if (errorText != null)
            errorText.text = message;
    }

    private void ClearError()
    {
        if (errorText != null)
            errorText.text = "";
    }

    public SNSAccountData GetCurrentAccount()
    {
        return currentAccount;
    }
}