using System;
using UnityEngine;

public enum MONSTER_TYPE
{
    COMMON
}
public enum TOWER_TYPE
{
    NONE,
    FIRE,
    ICE,
    POISON,
    IRON,
    WIND
}
public enum TILE_TYPE
{
    NONE,
    FIRE,
    ICE,
    POISON,
    IRON,
    WIND,
    WALL,
    STRAIGHT,
    TURN,
    CROSS

}

[Serializable]
public struct Point//(x,y)구조체
{
    public int x;
    public int y;
    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public Point(Point p)
    {
        this.x = p.x;
        this.y = p.y;
    }
    public static Point operator +(Point p1, Point p2)
    {
        return new Point(p1.x + p2.x, p1.y + p2.y);

    }
    public static Point operator -(Point p)
    {
        return new Point(-p.x, -p.y);

    }
    public static Point operator -(Point p1, Point p2)
    {
        return p1 + (-p2);

    }
    public static Point operator *(Point p, float f)
    {
        Point tmp;
        tmp.x = (int)(p.x * f);
        tmp.y = (int)(p.y * f);
        return tmp;

    }
    public static Point operator /(Point p, float f)
    {
        Point tmp;
        tmp.x = (int)(p.x / f);
        tmp.y = (int)(p.y / f);
        return tmp;

    }
    public bool Equals(Point p)
    {
        return this.x == p.x && this.y == p.y;
    }
    public override String ToString()
    {
        return $"({x},{y})";
    }
    public Vector2 ToVector()
    {
        return new Vector2(this.x, this.y);
    }
    public Vector3 ToVector3()
    {
        return new Vector3(this.x, this.y, 0);
    }
    public bool Inside(Point LeftTop, Point RightBottom)
    {
        return LeftTop.x <= x && x <= RightBottom.x &&
               LeftTop.y <= y && y <= RightBottom.y;
    }
}