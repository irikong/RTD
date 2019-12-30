using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    //singeton
    private static GameManager inst;
    public static GameManager Inst
    {
        get
        {
            return GameManager.inst;

        }
    }

    //외부 변수

    public BoardManager BM;
    public static readonly string PATH = "/Json/MapPath0.json";


    //내부 변수
    public float m_TimeScale;
    private GameBoard m_Board;
    private int m_PlayerHP;
    private int m_Round;
    private int m_Gold;
    private int BuiltTowers;   //설치된 타워 수
    private int RemainMonster; //보드에 남은 몬스터
    private int RemainSpan;    //남은 몬스터 스폰 횟수
    private float m_SpanTime = 0f;       //스폰 시간
    private float m_IntervalSpan = 0.3f; //스폰 간격
    private bool m_IsStarted = false;
    private bool m_IsEnded = false;
    private bool m_PauseState = false;
    private List<Vector2Int>[] m_RoundPath;     //이번 라운드의 몬스터 진행 경로

    //외부 속성
    public List<MonsterController> Monsters { get; set; }

    //내부 속성
    private int Gold
    {
        get
        {
            return this.m_Gold;
        }
        set
        {
            this.m_Gold = value;
            UIManager.Inst.GoldText.text = $"GOLD : {this.m_Gold:D8}";
        }
    }
    private int Round
    {
        get
        {
            return this.m_Round;
        }
        set
        {
            this.m_Round = value;
            UIManager.Inst.RoundText.text = $"ROUND {this.m_Round:D3}";
        }
    }
    private int PlayerHP
    {
        get
        {
            return this.m_PlayerHP;
        }
        set
        {
            this.m_PlayerHP = value;
            UIManager.Inst.LifeText.text = $"{this.m_PlayerHP:D2}";
        }
    }

    //유니티 이벤트

    private void Awake()
    {
        GameManager.inst = this;//싱글톤 초기화
    }
    private void Start()
    {
        //내부변수 할당
        Monsters = new List<MonsterController>();
        m_TimeScale = 1.0f;
        //PlayerHP = 50;
        //Round = 1;
        //Gold = 1000;
        LoadMap(PATH);
        BM.CreateBoard(m_Board);
        StartCoroutine(GameLoop());
       
    }
    /*
    public void MakeGameMap()//not use
    {
        var path = new List<Point>[4];
        for (int i = 0; i < 4; ++i)
            path[i] = new List<Point>();
        path[0].Add(new Point(2, 0));
        path[0].Add(new Point(2, 1));
        path[0].Add(new Point(3, 1));
        path[0].Add(new Point(4, 1));
        path[0].Add(new Point(5, 1));
        path[0].Add(new Point(6, 1));
        path[0].Add(new Point(6, 0));

        path[1].Add(new Point(6, 8));
        path[1].Add(new Point(6, 7));
        path[1].Add(new Point(6, 6));
        path[1].Add(new Point(7, 6));
        path[1].Add(new Point(8, 6));

        path[2].Add(new Point(0, 6));
        path[2].Add(new Point(1, 6));
        path[2].Add(new Point(2, 6));
        path[2].Add(new Point(3, 6));
        path[2].Add(new Point(4, 6));
        path[2].Add(new Point(4, 5));
        path[2].Add(new Point(4, 4));
        path[2].Add(new Point(5, 4));
        path[2].Add(new Point(6, 4));
        path[2].Add(new Point(7, 4));
        path[2].Add(new Point(7, 3));
        path[2].Add(new Point(7, 2));
        path[2].Add(new Point(8, 2));

        path[3].Add(new Point(0, 2));
        path[3].Add(new Point(1, 2));
        path[3].Add(new Point(2, 2));
        path[3].Add(new Point(2, 3));
        path[3].Add(new Point(2, 4));
        path[3].Add(new Point(2, 5));
        path[3].Add(new Point(2, 6));
        path[3].Add(new Point(2, 7));
        path[3].Add(new Point(2, 8));
        var info = new MapFileInfo();
        info.path0 = path[0].ToArray();
        info.path1 = path[1].ToArray();
        info.path2 = path[2].ToArray();
        info.path3 = path[3].ToArray();
        var jsonstr = JsonUtility.ToJson(info);
        var file = new FileStream(Application.dataPath + GameManager.PATH, FileMode.OpenOrCreate);
        var sw = new StreamWriter(file);
        sw.Write(jsonstr);
        sw.Close();
        gameMap = new GameBoard(path);
    }*/
    public void LoadMap(string path)
    {
        var file = new FileStream(Application.dataPath + path, FileMode.Open);
        var sr = new StreamReader(file);
        var jsonstr = sr.ReadToEnd();
        sr.Close();
        var info = JsonUtility.FromJson<PathFileInfo>(jsonstr);
        m_Board = new GameBoard(info);
    }
    private IEnumerator GameLoop()
    {
        yield return RoundStarting();
        yield return RoundPlaying();
        yield return RoundEnding();
        if (!m_IsEnded)
            StartCoroutine(GameLoop());
    }
    //라운드가 시작되면 종료
    private IEnumerator RoundStarting() {
        while (!m_IsStarted)  yield return null;
        SetRandomPath();
    }
    private IEnumerator RoundPlaying()
    {
        yield return null;
    }
    private IEnumerator RoundEnding()
    {
        yield return null;
    }
    private void Update()
    {
        //m_IntervalSpan 마다 몬스터 스폰
        if (m_IsStarted && RemainSpan != 0)
        {
            m_SpanTime += Time.deltaTime;
            if (m_SpanTime >= m_IntervalSpan)
            {
                --RemainSpan;
                m_SpanTime -= m_IntervalSpan;
                SpanMonster();
            }
        }
        //
    }
    //void OnApplicationPause(bool pauseStatus)
    //{
    //    if (pauseStatus)
    //    {
    //        isPaused = true;
    //    }
    //    else
    //    {
    //        if (isPaused)
    //        {
    //            isPaused = false;
    //        }
    //    }
    //}
    private void OnGUI()
    {
        //if (!btnTexture)
        //{
        //    Debug.Log("Check btnTexture");
        //    return;
        //}

        // 테스트용
        if (GUI.Button(new Rect(Screen.width / 16 * 12, Screen.height / 30, Screen.width / 10, Screen.height / 15), "Restart"))
        {
            SceneManager.LoadScene("GameScene");
        }
    }
    //내부 함수

    private void SpanMonster()
    {
        //몬스터 생성 후 정보 할당
        var mt = BM.CreateMonster(MONSTER_TYPE.COMMON);
        int hp = 5;
        int speed = 3;
        int attack = 1;
        int reward = 1;
        mt.SetStatus(hp, speed, attack, reward, m_RoundPath);
        Monsters.Add(mt);
        //
    }
    private void SetRandomPath()
    {
        int entryIndex = Random.Range(0, 8);
        //m_RoundPath = m_Board.PathAt(entryIndex);
        //var entry = m_RoundPath[1];
        //var exit = m_RoundPath[m_RoundPath.Count - 2];
        //BM.DisplayEntryMark(entry, exit);
    }

    //외부 함수

    public void SetPause()
    {
        if (!m_PauseState)
        {
            Time.timeScale = 0;
            UIManager.Inst.ChangeStartButtonImage(1);
            //UIManager.Inst.StartButton.GetComponent<Image>().sprite = UIManager.Inst.StartSpr;
            /*Resources.Load<Sprite>("Assets/Sprite/ETC/TestImage") as Sprite;*/
        }
        else
        {
            Time.timeScale = 1.0f;
            UIManager.Inst.ChangeStartButtonImage(0);
            //UIManager.Inst.StartButton.GetComponent<Image>().sprite = UIManager.Inst.StopSpr;
            /*Resources.Load<Sprite>("Assets/Sprite/ETC/TestImage") as Sprite;*/
        }

        m_PauseState ^= true;
    }
    public void MonsterArrive(int attack)
    {
        m_PlayerHP -= attack;

        if (m_PlayerHP == 0)
        {
            UIManager.Inst.OnGameOverPanel(m_Round);
        }

        if (--RemainMonster == 0)
            RoundEnd();
    }
    public void RoundStart()
    {
        if (m_IsStarted) //라운드가 진행중이면 종료
        {
            SetPause();
            return;
        }
        else
        {
            UIManager.Inst.ChangeStartButtonImage(0);
        }

        //라운드 정보 할당
        m_IsStarted = true;
        int count = 10;
        RemainSpan = count;
        RemainMonster = count;
        m_SpanTime = 0;
        //
    }
    public void RoundEnd()
    {
        m_IsStarted = false;
        ++Round;
        SetRandomPath();
        UIManager.Inst.ChangeStartButtonImage(1);
    }
    public void Restart()
    {
        SceneManager.LoadScene("GameScene");
    }
    public bool BoughtTower(int price)
    {
        if (Gold < price)
            return false;
        Gold -= price;
        return true;
    }
    public void AddRewardGold(int reward)
    {
        Gold += reward;
    }
}