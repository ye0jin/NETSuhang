// GameManager.cs
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Transform PlayerTrm;

    [SerializeField] private StageSO stageSO;
    private int stageIdx = -1;
    private bool stageUpgrade = false;
    public bool StageUpgrade => stageUpgrade;

    [HideInInspector] public Camera mainCam;

    [SerializeField] private GameObject mainGame;
    public void LoadMainGame()
    {
        mainGame.SetActive(true);
    }

    private int currentScore = 0;

    private Color mainColor;
    public Color MainColor => mainColor;

    private float colorChangeDuration = 1.0f;

    private bool isGameOver = false;
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        Screen.SetResolution(576, 1024, false);
        Screen.fullScreenMode = FullScreenMode.Windowed;
        if (Instance != null) print("GM Error");
        Instance = this;
        mainCam = Camera.main;
        currentScore = 0;
    }

    private void Start()
    {
        mainGame.SetActive(false);
        StartCoroutine("ChangeColorCoroutine");
    }

    public int StageUpdate()
    {
        return stageSO.stageList[++stageIdx];
    }

    public void GameOver()
    {
        isGameOver = true;
        UIManager.Instance.SetGameOverPanel();
    }

    public int ReturnScore()
    {
        return currentScore;
    }

    public void ScoreUpdate()
    {
        currentScore++;
        UIManager.Instance.UpdateTextColor(); // UIManager의 UpdateTextColor 메서드 호출
        if (currentScore % 10 == 0)
        {
            StartCoroutine(ChangeColorCoroutine());
        }
        if (currentScore % 15 == 0) // 난이도 증가
        {
            //print("dd");
            if(stageIdx < stageSO.stageList.Count - 1)
            {
                stageUpgrade = true;
                MapManager.Instance.WallCntUpdate();
            }
            else
            {
                PlayerTrm.gameObject.GetComponent<Player>().AddSpeed();
            }
        }
    }

    private int check = 0;
    public void CheckUpgrade()
    {
        check++;
        if(check >= 2)
        {
            check = 0;
            stageUpgrade = false;
        }
    }

    private IEnumerator ChangeColorCoroutine()
    {
        mainColor = Random.ColorHSV(0f, 1f, 0.3f, 0.7f, 0.35f, 1.0f);
        float elapsedTime = 0f;
        Color startColor = mainCam.backgroundColor;

        while (elapsedTime < colorChangeDuration)
        {
            mainCam.backgroundColor = Color.Lerp(startColor, mainColor, elapsedTime / colorChangeDuration);
            UIManager.Instance.scoreText.color = Color.Lerp(startColor, mainColor, elapsedTime / colorChangeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCam.backgroundColor = mainColor;
    }

    public void ResetMain()
    {
        stageIdx = -1;
        mainGame.SetActive(false);
        isGameOver = false;
        stageUpgrade = false;
        currentScore = 0;
        check = 0;
    }
}
