// UIManager.cs
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
   
    [Header("Score")]
    [SerializeField] private Image scoreImage;
    [SerializeField] public TextMeshProUGUI scoreText;

    [Header("GameOver")]
    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        if (Instance != null) print("UIManager Error");
        Instance = this;
    }

    private void Start()
    {
        gameOverPanel.transform.localScale = new Vector3(0, 0, 0);
        scoreText.text = $"{GameManager.Instance.CurrentScore}";
    }

    public void UpdateTextColor()
    {
        scoreImage.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.5f, 3, 0.4f);
        scoreText.text = $"{GameManager.Instance.CurrentScore}";
    }

    public void SetGameOverPanel()
    {
        gameOverPanel.transform.DOScale(1, 1.5f);
    }
    public void ExitGameOverPanel()
    {
        gameOverPanel.transform.DOScale(0, 1.0f);
    }

    public void ResetMain()
    {
        scoreText.text = $"{0}";
    }
}
