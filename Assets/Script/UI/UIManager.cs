using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private static UIManager inst;
    public static UIManager Inst
    {
        get
        {
            return UIManager.inst;
        }
    }
    public Text LifeText;
    public Text RoundText;
    public Text GoldText;
    public Text TowerTierText;
    public Text TowerTypeText;
    public Text TowerAttackText;
    public Text TowerDelayText;
    public Button AddTowerButton;
    public Button FastButton;
    public Button StartStopButton;
    public Button OptionButton;
    public GameObject TowerExplainPanel;
    public GameObject EmptyPanel;

    public GameObject GameOverPanel;
    public Button RestartButton;
    public Button MenuButton;
    public Text RoundOverText;

    public Sprite StartSpr;
    public Sprite StopSpr;
    

    private void Awake()
    {
        UIManager.inst = this;
    }
    private void Start()
    {
        //버튼 이벤트 할당
        StartStopButton.onClick.AddListener(GameManager.Inst.RoundStart);
        //AddTowerButton.onClick.AddListener(GameManager.Inst.BM.AddRandomTower);
        OptionButton.onClick.AddListener(GameManager.Inst.SetPause);
        MenuButton.onClick.AddListener(ChangeMainScene);
    }

    public void ChangeStartButtonImage(int state = -1)
    {
        if (state == -1) { return; }
        else if (state == 0)
        {
            StartStopButton.GetComponent<Image>().sprite = StopSpr;
        }
        else if (state == 1)
        {
            StartStopButton.GetComponent<Image>().sprite = StartSpr;
        }
    }

    public void OnGameOverPanel(int round)
    {
        GameOverPanel.SetActive(true);
        RoundOverText.text = round.ToString() + " Round";
        RestartButton.onClick.AddListener(GameManager.Inst.Restart);
    }

    void ChangeMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}
