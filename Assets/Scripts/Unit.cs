using System;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitData Data { get; private set; }
    public int CurrentHP { get; private set; }
    public int CurrentMP { get; private set; }
    public float ATB { get; set; }                 // 0..1
    public Vector2Int Cell { get; set; }
    public bool IsDefending { get; private set; }
    public bool IsReadyForTurn { get; set; }       // ATB lleno (en cola o actuando)
    public bool HasMoved { get; set; }
    public float yOffset;                          // 1 para cápsulas primitivas
    public GridMovement Movement { get; private set; }

    public bool IsDead => CurrentHP <= 0;
    public bool IsPlayer => Data.isPlayerControlled;
    public int AttackPower => Mathf.RoundToInt(Data.attack * atkMult);
    public int MagicPower => Data.magic;

    public event Action<Unit> StatsChanged;
    public event Action<Unit> Died;

    float atkMult = 1f, buffTime;

    public Vector3 WorldPos(Vector2Int c) => GridManager.Instance.CellToWorld(c) + Vector3.up * yOffset;

    public void Init(UnitData data, Vector2Int cell)
    {
        Data = data;
        CurrentHP = data.maxHP;
        CurrentMP = data.maxMP;
        Movement = GetComponent<GridMovement>();
        if (Movement == null) Movement = gameObject.AddComponent<GridMovement>();
        GridManager.Instance.Place(this, cell);
        transform.position = WorldPos(cell);
    }

    public void BeginTurn() { IsDefending = false; HasMoved = false; }
    public void Defend() { IsDefending = true; StatsChanged?.Invoke(this); }

    public int TakeDamage(int raw)
    {
        if (IsDead) return 0;
        int dmg = IsDefending ? Mathf.CeilToInt(raw * 0.5f) : raw;
        CurrentHP = Mathf.Max(0, CurrentHP - dmg);
        StatsChanged?.Invoke(this);
        if (CurrentHP == 0) Die();
        return dmg;
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Min(Data.maxHP, CurrentHP + amount);
        StatsChanged?.Invoke(this);
    }

    public void RestoreMP(int amount)
    {
        if (IsDead) return;
        CurrentMP = Mathf.Min(Data.maxMP, CurrentMP + amount);
        StatsChanged?.Invoke(this);
    }

    public bool TrySpendMP(int cost)
    {
        if (CurrentMP < cost) return false;
        CurrentMP -= cost;
        StatsChanged?.Invoke(this);
        return true;
    }

    public void ApplyAttackBuff(float mult, float seconds) { atkMult = mult; buffTime = seconds; StatsChanged?.Invoke(this); }

    // Lo llama el ATBSystem: los buffs avanzan con el tiempo de batalla (no durante animaciones).
    public void TickBuff(float dt)
    {
        if (buffTime <= 0f) return;
        buffTime -= dt;
        if (buffTime <= 0f) { atkMult = 1f; StatsChanged?.Invoke(this); }
    }

    void Die()
    {
        GridManager.Instance.Remove(this);
        Died?.Invoke(this);
        Invoke(nameof(Disappear), 0.8f);
    }

    void Disappear() { gameObject.SetActive(false); }
}
