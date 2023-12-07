using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class SubUIManager : MonoBehaviour
{
    public static SubUIManager Instance;

    private bool gameStarted = false;

    [Header("MainPanel")]
    [SerializeField] private GameObject gameStartBtn;
    [SerializeField] private GameObject gameStartPanel;
    [Header("Login")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TextMeshProUGUI loginText;
    [SerializeField] private GameObject loginBtn;
    [SerializeField] private GameObject logoutBtn;
    [Header("Change")]
    [SerializeField] private GameObject changePanel;
    [SerializeField] private GameObject changePasswordBtn;
    [Header("Register")]
    [SerializeField] private GameObject registerPanel;
    [Header("ETC")]
    [SerializeField] private GameObject exitBtn;
    [SerializeField] private Image emojiImage;
    [SerializeField] private Transform titleText;
    [SerializeField] private GameObject checkingObj;

    private Color mainColor;
    private float colorChangeDuration = 3.0f;
    private string emojiString = "";

    private void Awake()
    {
        if (Instance != null) print("SubuiManager Error");
        Instance = this;
    }
    private void Start()
    {
        // 초기ㅘ
        loginPanel.transform.localPosition = new Vector3(0, 3200, 0);
        changePanel.transform.localPosition = new Vector3(0, 3200, 0);
        registerPanel.transform.localPosition = new Vector3(2000, 0, 0);
        titleText.transform.localPosition = new Vector3(0, 135, 0);

        loginText.text = "";
        
        loginBtn.SetActive(true);
        exitBtn.SetActive(true);
        gameStartBtn.SetActive(false);
        changePasswordBtn.SetActive(false);
        logoutBtn.SetActive(false);

        StartCoroutine(ColorChange());
    }

    public void IsLogout()
    {
        // 로그아웃
        exitBtn.SetActive(true);
        exitBtn.gameObject.transform.localScale = Vector3.one;
        loginBtn.SetActive(true);
        loginBtn.gameObject.transform.localScale = Vector3.one;
        logoutBtn.SetActive(false);
        logoutBtn.gameObject.transform.localScale = Vector3.one;
        gameStartBtn.SetActive(false);
        gameStartBtn.gameObject.transform.localScale = Vector3.one;
        changePasswordBtn.SetActive(false); 
        changePasswordBtn.gameObject.transform.localScale = Vector3.one;

        titleText.DOLocalMoveY(135, 1.2f).SetEase(Ease.InOutExpo);
    }
    public void IsLogin(string email)
    {
        // 로그인했을 경우
        exitBtn.SetActive(false);
        exitBtn.gameObject.transform.localScale = Vector3.one;
        loginBtn.SetActive(false);
        loginBtn.gameObject.transform.localScale = Vector3.one;
        logoutBtn.SetActive(true);
        logoutBtn.gameObject.transform.localScale = Vector3.one;
        gameStartBtn.SetActive(true);
        gameStartBtn.gameObject.transform.localScale = Vector3.one;
        changePasswordBtn.SetActive(true);
        changePasswordBtn.gameObject.transform.localScale = Vector3.one;

        titleText.DOLocalMoveY(620, 1.2f).SetEase(Ease.InOutExpo);

        //loginText.gameObject.SetActive(true);
        //loginText.text = $"{email}, Attending for {1} Days!";
    }

    public void MessageBox(string result)
    {
        checkingObj.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = $"{result}";
        checkingObj.transform.DOScale(1, 0.7f);
        StartCoroutine(Waiting());
    }
    public IEnumerator Waiting()
    {
        yield return new WaitForSeconds(1.5f);
        checkingObj.transform.DOScale(0, 0.7f);
    }

    private IEnumerator ColorChange()
    {
        mainColor = Random.ColorHSV(0f, 1f, 0.3f, 0.7f, 0.35f, 1.0f);
        mainColor.a = 0.98f;

        float elapsedTime = 0f;
        Color startColor = gameStartPanel.GetComponent<Image>().color;

        while (elapsedTime < colorChangeDuration)
        {
            gameStartPanel.GetComponent<Image>().color = Color.Lerp(startColor, mainColor, elapsedTime / colorChangeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        gameStartPanel.GetComponent<Image>().color = mainColor;
        StartCoroutine("ColorChange");
    }

    public void SetEmoji(string s)
    {
        emojiString = s;
        emojiImage.GetComponent<Animator>().SetBool(emojiString, true);
    }
    public void OutEmoji()  
    {
        emojiImage.GetComponent<Animator>().SetBool(emojiString, false);
        emojiString = ""; // 초기화
    }

    #region 씬로드
    public void LoadGameScene()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        changePanel.SetActive(false);
        gameStartPanel.transform.DOLocalMoveY(-3400, 1.3f).OnComplete(() => StopCoroutine("ColorChange"));
        GameManager.Instance.LoadMainGame();
    }
    public void LoadStartScene()
    {
        StartCoroutine("ColorChange");
        gameStartPanel.transform.DOLocalMoveY(0, 1.3f).OnComplete(()=>
        {
            loginPanel.SetActive(true);
            registerPanel.SetActive(true);
            changePanel.SetActive(true);
        });
    }
    #endregion

    #region 버튼
    public void LoadChangePanel()
    {
        changePanel.transform.DOLocalMoveY(0, 1.5f);
    }
    public void LoadLoginPanel()
    {
        loginPanel.transform.DOLocalMoveY(0, 1.5f);
    }
    public void LoadRegisterPanel()
    {
        loginPanel.transform.DOLocalMoveX(-2000f, 1.5f);
        registerPanel.transform.DOLocalMoveX(0, 1.5f);
    }
    public void ExitRegisterPanel()
    {
        loginPanel.transform.DOLocalMoveX(0f, 1.5f);
        registerPanel.transform.DOLocalMoveX(2000f, 1.5f);
    }
    public void ExitChangePanel()
    {
        changePanel.transform.DOLocalMoveY(3200, 1.5f);
    }
    public void ExitLoginPanel()
    {
        loginPanel.transform.DOLocalMoveY(3200, 1.5f);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    #endregion
}
