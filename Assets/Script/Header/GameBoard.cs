using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
[Serializable]
public struct PathFileInfo
{
    public Vector2Int[] path0;
    public Vector2Int[] path1;
    public Vector2Int[] path2;
    public Vector2Int[] path3;
}
public class PathInfo
{
    public List<Vector2Int> m_Entrys;
    public List<Vector2Int>[] m_Paths;      //큐브단위 경로
    public List<Vector2Int>[] m_PathLines;  //선 단위 경로
    public PathInfo()
    {
        m_Entrys = new List<Vector2Int>();
        m_Paths = new List<Vector2Int>[4];
        m_PathLines = new List<Vector2Int>[4];
        for (int i = 0; i < 4; ++i)
        {
            m_Paths[i] = new List<Vector2Int>();
            m_PathLines[i] = new List<Vector2Int>();
        }
    }

    //경로를 pathfile에서 복사
    public void CopyFrom(PathFileInfo file)
    {
        m_Entrys.Clear();

        m_Paths[0].Clear();
        var path0 = file.path0.ToList();
        m_Entrys.Add(path0[0]);
        m_Entrys.Add(path0[path0.Count - 1]);
        this.m_Paths[0].AddRange(path0);

        m_Paths[1].Clear();
        var path1 = file.path1.ToList();
        m_Entrys.Add(path1[0]);
        m_Entrys.Add(path1[path1.Count - 1]);
        this.m_Paths[1].AddRange(path1);

        m_Paths[2].Clear();
        var path2 = file.path2.ToList();
        m_Entrys.Add(path2[0]);
        m_Entrys.Add(path2[path2.Count - 1]);
        this.m_Paths[2].AddRange(path2);

        m_Paths[3].Clear();
        var path3 = file.path3.ToList();
        m_Entrys.Add(path3[0]);
        m_Entrys.Add(path3[path3.Count - 1]);
        this.m_Paths[3].AddRange(path3);
        MakePathLines();
    }
    //완성된 경로에서 선형 경로를 생성, 복사후 자동 호출
    public void MakePathLines()
    {
        foreach (int i in Enumerable.Range(0, 4))
        {
            m_PathLines[i].Clear();
            var prev = m_Paths[i][0];
            m_PathLines[i].Add(prev);
            foreach (int n in Enumerable.Range(1, m_Paths[i].Count - 2))
            {
                var curr = m_Paths[i][n];
                var next = m_Paths[i][n + 1];
                if (prev.x != next.x && prev.y != next.y)
                    m_PathLines[i].Add(curr);
                prev = curr;
            }
            m_PathLines[i].Add(m_Paths[i][m_Paths[i].Count - 1]);
        }
    }
    //m_Paths를 하나의 리스트로 반환
    public List<Vector2Int> ToList()
    {
        var tmp = new List<Vector2Int>();
        foreach (var l in m_Paths) tmp.AddRange(l);
        return tmp;
    }
    public PathFileInfo ToFileInfo()
    {
        PathFileInfo tmp;
        tmp.path0 = m_Paths[0].ToArray();
        tmp.path1 = m_Paths[1].ToArray();
        tmp.path2 = m_Paths[2].ToArray();
        tmp.path3 = m_Paths[3].ToArray();
        return tmp;
    }

}

//보드에 대한 정보가 담긴 클래스
public class GameBoard : MonoBehaviour
{
    public CubeManager[,] m_Tiles = new CubeManager[9, 9];
    public PathInfo m_Path = new PathInfo();
    public CubeManager m_SelectCube = CubeManager.None;
    public GameBoard()
    {
        var mat9 = from x in Enumerable.Range(0, 9)
                   from y in Enumerable.Range(0, 9)
                   select new Vector2Int(x, y);
        foreach (var v in mat9)
            m_Tiles[v.x, v.y] = new CubeManager();

    }
    public GameBoard(PathFileInfo file) : this()
    {
        m_Path.CopyFrom(file);
        PathInit();
        ShuffleTile();
    }
    public CubeManager this[int x, int y]
    {
        get { return m_Tiles[x, y]; }
    }
    //경로타일을 저장한다.
    private void PathInit()
    {
        //전체 경로
        var mapPath = m_Path.ToList();
        //첫번째 경로큐브
        var type = TILE_TYPE.STRAIGHT;
        var pos = mapPath[0];
        float rotate = pos.x == mapPath[1].x ? 0f : 90f;
        m_Tiles[pos.x, pos.y].Init(type, pos, rotate);
        //2~n-1 경로큐브
        for (int i = 1; i + 1 < mapPath.Count; ++i)
        {
            var prev = mapPath[i - 1];
            var cur = mapPath[i];
            var next = mapPath[i + 1];
            bool straightY = (next - prev).x == 0;
            bool straightX = (next - prev).y == 0;
            type = straightX || straightY ? TILE_TYPE.STRAIGHT : TILE_TYPE.TURN;
            rotate = (type == TILE_TYPE.STRAIGHT) ?
                     straightX ? 90f : 0f
                   : GetTurnRotate(prev, cur, next);
            var cm = m_Tiles[cur.x, cur.y];
            if (cm.m_Type == TILE_TYPE.STRAIGHT)
                cm.Init(TILE_TYPE.CROSS, cur);
            else
                cm.Init(type, cur, rotate);
        }
        //
        //마지막 경로 큐브
        type = TILE_TYPE.STRAIGHT;
        pos = mapPath[mapPath.Count - 1];
        rotate = pos.x == mapPath[mapPath.Count - 2].x ? 0 : 90f;
        m_Tiles[pos.x, pos.y].Init(type, pos, rotate);
    }
    //회전 타일의 각도를 반환한다.
    private float GetTurnRotate(Vector2Int prev, Vector2Int cur, Vector2Int next)
    {
        var step1 = cur - prev;
        var step2 = next - cur;
        return step1.y == 0 ?
               step1.x < 0 ?
                   step2.y < 0 ? 0f : 270f
                 : step2.y < 0 ? 90f : 180f
             : step1.y < 0 ?
                   step2.x < 0 ? 18f : 270f
                 : step2.x < 0 ? 90f : 0f;
    }
    //타일을 무작위로 재배치한다.
    public void ShuffleTile()
    {
        foreach (var p in from x in Enumerable.Range(1, 7)
                          from y in Enumerable.Range(1, 7)
                          where  m_Tiles[x, y].m_Type <  TILE_TYPE.WALL
                          select new Vector2Int(x, y))
            m_Tiles[p.x, p.y].Init((TILE_TYPE)Random.Range(1, 6), p);

    }
    //n(0<=n<8)번째 entry를 반환한다.
    public Vector2Int EntryAt(int n)
    {
        if (n < 0 || n > 7)
            throw new ArgumentOutOfRangeException($"ERROR :entry 범위를 벗어났습니다 : {n}");
        return m_Path.m_Entrys[n];
    }
    //n번째 fullpath를 반환한다.
    public List<Vector2>[] PathAt(int n)
    {
        if (n < 0 || n > 7)
            throw new ArgumentOutOfRangeException($"ERROR :path 범위를 벗어났습니다 : {n}");

        bool reverse = n % 2 == 1;
        var indexs = Enumerable.Range(n / 2, 4);
        var path = (from i in reverse ? indexs : indexs.Reverse()
                    let pathline = ToVector2(m_Path.m_PathLines[i % 4])
                    select reverse ? pathline : Reversed(pathline)).ToArray();
        foreach (var i in Enumerable.Range(0, 4))
        {
            var v1 = path[i][0];
            var v2 = path[i][path[i].Count - 1];

            if (v1.x == 0) { v1.x -= 0.5f; v2.x += 0.5f; }
            else if (v1.x == 8) { v1.x += 0.5f; v2.x -= 0.5f; }
            else if (v1.y == 0) { v1.y -= 0.5f; v2.y += 0.5f; }
            else if (v1.y == 8) { v1.y += 0.5f; v2.y -= 0.5f; }
            path[i][0] = v1;
            path[i][path[i].Count - 1] = v2;
        }
        return path;
    }

    private static List<Vector2> Reversed(List<Vector2> list)
    {
        return ((IEnumerable<Vector2>)list).Reverse().ToList();
    }
    private static List<Vector2> ToVector2(IEnumerable<Vector2Int> list)
    {
        var tmp = new List<Vector2>();
        foreach (var v in list) tmp.Add(v);
        return tmp;

    }
}
