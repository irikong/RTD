using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{

    public TowerManager[] m_TowerPrefabs;
    public GameObject[] m_CubePrefabs;
    public GameObject[] m_MonsterPrefabs;
    public Material m_SelectMask;

    private GameBoard m_Board;
    private Transform m_TileHolder;
    private Transform m_WallHolder;
    private Transform m_PathHolder;
    private Transform m_TowerHolder;
    private Transform m_MonsterHolder;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnMouseLeftClick();
    }
    //마우스 클릭 이벤트
    private void OnMouseLeftClick()
    {
        var selected = CubeDeselect();
        RaycastHit hit;
        if (GetScreenHit(out hit))
        {
            var obj = hit.collider.gameObject;
            if (obj.CompareTag("Tile"))
            {
                var cube = m_Board[(int)obj.transform.position.x, (int)obj.transform.position.z];
                if (cube != selected)   // 다른 큐브가 클릭됐으면 선택
                    CubeSelect(cube);
            }
        }
    }
    //cube mask제거 후 board에서 선택 취소후 선택된 타일 반환
    public CubeManager CubeDeselect()
    {
        var selected = m_Board.m_SelectCube;
        if (selected.m_Instance != null)
        {
            selected.m_Instance.GetComponent<MeshRenderer>().material = null;
            m_Board.m_SelectCube = CubeManager.None;
        }
        return selected;

    }
    //cube mask생성 후 board에서 선택
    public void CubeSelect(CubeManager cube)
    {
        cube.m_Instance.GetComponent<MeshRenderer>().material = m_SelectMask;
        m_Board.m_SelectCube = cube;

    }
    //클릭된 곳의 충돌 여부, 충돌 정보 반환
    private bool GetScreenHit(out RaycastHit hit)
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray.origin, ray.direction, out hit);
    }
    //보드 생성
    public void CreateBoard(GameBoard board)
    {
        m_Board = board;
        InitObjectHolder();
        InstanciatePathTile();
        InstanciateWall();
    }
    //holder 객체 초기화
    private void InitObjectHolder()
    {

        var TileMap = new GameObject() { name = "Map" }.transform;
        TileMap.parent = transform;
        m_TileHolder = new GameObject() { name = "Tiles" }.transform;
        m_TileHolder.parent = TileMap;
        m_WallHolder = new GameObject() { name = "Walls" }.transform;
        m_WallHolder.parent = TileMap;
        m_PathHolder = new GameObject() { name = "Paths" }.transform;
        m_PathHolder.parent = TileMap;
        m_TowerHolder = new GameObject() { name = "Towers" }.transform;
        m_MonsterHolder = new GameObject() { name = "Monsters" }.transform;

    }
    //path, tile 생성
    private void InstanciatePathTile()
    {
        foreach (var p in from x in Enumerable.Range(0, 9)
                          from y in Enumerable.Range(0, 9)
                          select new Vector2Int(x, y))
        {
            var cm = m_Board[p.x, p.y];
            switch (cm.m_Type)
            {
                case TILE_TYPE.STRAIGHT:
                case TILE_TYPE.TURN:
                case TILE_TYPE.CROSS:
                    cm.m_Instance = Instantiate(m_CubePrefabs[(int)cm.m_Type], cm.m_Position, cm.m_Rotation, m_PathHolder);
                    break;
                case TILE_TYPE.NONE:
                case TILE_TYPE.WALL:
                    break;
                default:
                    cm.m_Instance = Instantiate(m_CubePrefabs[(int)cm.m_Type], cm.m_Position, cm.m_Rotation, m_TileHolder);
                    break;
            }
        }
    }
    //wall 생성
    private void InstanciateWall()
    {
        foreach (var p in from x in Enumerable.Range(0, 9)
                          from y in Enumerable.Range(0, 9)
                          where x == 0 || y == 0 || x == 8 || y == 8
                          select new Vector2Int(x, y))
        {
            if (m_Board[p.x, p.y].m_Type == TILE_TYPE.NONE)
            {
                Instantiate(m_CubePrefabs[(int)TILE_TYPE.WALL], new Vector3(p.x, -0.5f, p.y), Quaternion.Euler(0f, 0f, 0f), m_WallHolder);
                Instantiate(m_CubePrefabs[(int)TILE_TYPE.WALL], new Vector3(p.x, 0.5f, p.y), Quaternion.Euler(0f, 0f, 0f), m_WallHolder);
            }
            Instantiate(m_CubePrefabs[(int)TILE_TYPE.WALL], new Vector3(p.x, 1.5f, p.y), Quaternion.Euler(0f, 0f, 0f), m_WallHolder);
        }
    }
    // type형 몬스터를 보드에 생성한 후 controller 반환
    public MonsterController CreateMonster(MONSTER_TYPE type)
    {
        return Instantiate(m_MonsterPrefabs[(int)type], m_MonsterHolder.transform)
        .GetComponent<MonsterController>();
    }
    // type형 타워를 보드에 생성한 후 manager를 반환
    public TowerManager CreateTower(int type)
    {
        return Instantiate(m_TowerPrefabs[type], m_TowerHolder.transform) as TowerManager;
    }

    //entry mark 표시
    //public void DisplayEntryMark(Point entry, Point exit)
    //{

    //    var entryTile = Tiles[entry.x, entry.y];
    //    var exitTile = Tiles[exit.x, exit.y];
    //    float angle = (entry.y == 0) ? 0f :
    //                  (entry.y == 8) ? 180f :
    //                  (entry.x == 0) ? 270f :
    //                                   90f;
    //    if (this.entry != null)
    //        Destroy(this.entry);
    //    this.entry = Instantiate(EntryMark, entryTile.transform.position, Quaternion.Euler(0, 0, angle), entryTile.transform);
    //    if (this.exit != null)
    //        Destroy(this.exit);
    //    this.exit = Instantiate(EntryMark, exitTile.transform.position, Quaternion.Euler(0, 0, angle), exitTile.transform);
    //}
    //타워정보 표시
    //private void DisplayOn(TowerManager tm)
    //{
    //    var ui = UIManager.Inst;
    //    ui.TowerExplainPanel.SetActive(true);
    //    ui.EmptyPanel.SetActive(false);
    //    if (rangeMask != null)
    //        Destroy(rangeMask);
    //    rangeMask = Instantiate(RangeMask, tm.BaseTile.transform);
    //    rangeMask.transform.Translate(0, 1, 0);
    //    int scale = tm.Range * 2 + 1;
    //    rangeMask.transform.localScale = new Vector3(scale, scale);
    //    rangeMask.transform.Rotate(270, 0, 0);
    //    ui.TowerTierText.text = $"Tier : {tm.Tier}";
    //    ui.TowerTypeText.text = $"Element : {tm.Type}";
    //    ui.TowerAttackText.text = $"Attack : {tm.Attack}";
    //    ui.TowerDelayText.text = $"Delay : {tm.Delay}";
    //}
    //타워정보 감춤
    //private void DisplayOff()
    //{
    //    UIManager.Inst.TowerExplainPanel.SetActive(false);
    //    UIManager.Inst.EmptyPanel.SetActive(true);
    //    if (rangeMask != null)
    //        Destroy(rangeMask);
    //}
    //public void AddRandomTower()
    //{
    //    if (SelectedTile == null)
    //        return;
    //    if (SelectedTile.BuiltTower != null)
    //        return;
    //    if (!GameManager.Inst.BoughtTower(10))
    //        return;
    //    int type = Random.Range(1, 6);
    //    TowerManager tw = CreateTower(type);
    //    SelectedTile.BuiltTower = tw;
    //    tw.SetStatus(SelectedTile.transform.position, (TOWER_TYPE)type, 1f, 1, 1, 0.5f, SelectedTile);
    //    DisplayOn(tw);
    //}
}
