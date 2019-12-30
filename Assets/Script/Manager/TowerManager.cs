using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TowerManager : MonoBehaviour
{

    public MonsterController target;
    //private string monsterTag = "Monster";

    public Sprite[] Texture;
    public GameObject hitEffect;
    private SpriteRenderer spr;
    private TOWER_TYPE type;
    private float attackPoint; // 타워 공격력
    private float attackRate = 1f;
    private int range;
    private float rangeRate = 1f;
    private float delay; // 지연 시간
    private float delayRate = 1f;
    private float delayRemain = 0.0f; // 남은 지연 시간(deltatime)
    private int tier;
    public CubeManager BaseTile { get; set; }

    public TOWER_TYPE Type { get { return type; } }
    public float Attack { get { return attackPoint * attackRate; } }
    public int Range { get { return (int)(range * rangeRate); } }
    public float Delay { get { return delay * delayRate; } }
    public int Tier
    {
        get { return this.tier; }
        set
        {
            GetComponent<SpriteRenderer>().sprite = Texture[value - 1];
            this.tier = value;
        }
    }
    void Start()
    {
        transform.Translate(Vector3.back);
        spr = GetComponent<SpriteRenderer>();
    }

    private void OnSameTile()
    {
        this.attackRate = 1.5f;
        this.delayRate = 0.8f;
    }
    public void SetStatus(Vector3 pos,TOWER_TYPE type, float attack, int range, int tier, float delay, CubeManager basetile)
    {
        this.transform.position = pos;
        this.type = type;
        this.attackPoint = attack;
        this.range = range;
        Tier = tier;
        this.delay = delay;
        if (type == TOWER_TYPE.WIND)
            this.delay *= 0.5f;
        if ((int)type == (int)basetile.m_Type)
            OnSameTile();
        this.BaseTile = basetile;
    }
    public void UpgradeTower() 
    {
        int type = Random.Range(1, 6);
        var tw = GameManager.Inst.BM.CreateTower(type);
        tw.SetStatus(BaseTile.m_Position,(TOWER_TYPE)type, attackPoint + 1f, range + 1, tier + 1, 0.5f, BaseTile);
        //BaseTile.BuiltTower = tw;
        //GameManager.Inst.BM.TileSelect(tw.BaseTile);
        DestroyObj();
    }
    public void DestroyObj()
    {
        //target.TargetedTowers.Remove(this);
        Destroy(gameObject);
    }
    void FixedUpdate()
    {
        if (CheckTarget())
            AttackTarget();
    }

    private Vector3 firstPosition;

    private void OnMouseDown()
    {
        //Debug.Log("OnMouseDown");

        firstPosition = this.transform.position;
        //GameManager.Inst.BM.TileSelect(BaseTile);
    }

    private void OnMouseUp()
    {
        //Debug.Log("OnMouseUp");

        OnOffCollider();

        Vector2 pos = this.transform.position;
        RaycastHit2D rayHit = Physics2D.Raycast(pos, Vector2.zero);

        OnOffCollider();

        if (rayHit.collider != null && rayHit.collider.CompareTag("Tower"))
        {
            var tw = rayHit.collider.gameObject.GetComponent<TowerManager>();
            if (Tier != tw.Tier || Tier > 2)
            {
                this.transform.position = firstPosition;
                return;
            }
            //Debug.Log(rayHit.collider.gameObject.name);
            //BaseTile.BuiltTower = null;
            DestroyObj();
            tw.UpgradeTower();
        }
        else
        {
            this.transform.position = firstPosition;
        }
    }

    private void OnMouseDrag()
    {
        //Debug.Log("OnMouseDrag");

        float dist = 10.0f;

        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, dist);
        this.transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
    }

    private void OnOffCollider()
    {
        this.gameObject.GetComponent<BoxCollider2D>().enabled ^= true;
    }
    private bool Targeting(MonsterController target)
    {
        if (target != null)
        {
            target.TargetedTowers.Add(this); // 간헐적으로 에러 발생
            this.target = target;
            return true;
        }
        return false;
    }
    private void DisTartgeting()
    {
        target.TargetedTowers.Remove(this);
        target = null;
    }
    private bool CheckTarget()
    {
        //if (target != null)
            //if (!target.Position.Inside(lt, rb))
            //    DisTartgeting();
        //if (target == null)
            //return Targeting(GameManager.Inst.Monsters.Find(mc => mc.Position.Inside(lt, rb)));
        return true;
    }

    void AttackTarget()
    {
        delayRemain -= Time.deltaTime;
        if (delayRemain > 0.0f) return;
        //Debug.Log(delayTimeRemain);
        Debug.DrawLine((Vector2)this.transform.position, (Vector2)target.transform.position, Color.red);
        DrawHitEffect();

        switch (type)
        {
            case TOWER_TYPE.FIRE:
                Collider2D[] MonsterColl = Physics2D.OverlapBoxAll(target.transform.position, new Vector2(tier * 1.5f, tier * 1.5f), 0);

                foreach (Collider2D i in MonsterColl)
                {
                    if (i.gameObject.tag == "Monster")
                    {
                        i.gameObject.GetComponent<MonsterController>().AttackedByTower(Attack);
                    }
                }
                break;
            case TOWER_TYPE.ICE:
                target.GetComponent<MonsterController>().Iced(tier);
                target.GetComponent<MonsterController>().AttackedByTower(Attack);
                break;
            case TOWER_TYPE.POISON:
                target.GetComponent<MonsterController>().Poisoned(tier);
                break;
            case TOWER_TYPE.IRON:
                target.GetComponent<MonsterController>().IronStack(tier);
                target.GetComponent<MonsterController>().AttackedByTower(Attack);
                break;
            case TOWER_TYPE.WIND:
                target.GetComponent<MonsterController>().AttackedByTower(Attack);
                break;
            default:
                target.GetComponent<MonsterController>().AttackedByTower(Attack);
                break;
        }
        delayRemain = Delay;
    }
    private void DrawHitEffect()
    {
        if (!target) return;

        hitEffect.GetComponent<SpriteRenderer>().color = spr.color;

        Destroy(Instantiate(hitEffect.gameObject, target.transform.position, target.transform.rotation), 0.5f);
    }
}