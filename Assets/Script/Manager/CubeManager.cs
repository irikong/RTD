using UnityEngine;

public class CubeManager
{
    public static readonly CubeManager None = new CubeManager();
    public GameObject m_Instance = null;
    public TILE_TYPE m_Type = TILE_TYPE.NONE;
    public Vector3 m_Position;
    public Quaternion m_Rotation;
    public void Init(TILE_TYPE type, Vector2Int pos, float angle = 0f)
    {
        m_Type = type;
        m_Position = new Vector3(pos.x, type < TILE_TYPE.WALL ? 0f : -0.5f, pos.y);
        m_Rotation = Quaternion.Euler(0, angle, 0);
    }
    public override string ToString()
    {
        return $"[{m_Type}, {m_Position}, {m_Rotation.eulerAngles.y}]";
    }
}
