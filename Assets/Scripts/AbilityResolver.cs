using System.Collections.Generic;
using UnityEngine;

// Geometría de habilidades (línea / área / simple) + cálculo y aplicación de efectos.
public static class AbilityResolver
{
    const float Scale = 2f;

    public static bool IsSelfCentered(AbilityData ab) => ab.shape == AreaShape.Circle && ab.range == 0;

    public static bool Matches(Unit caster, Unit u, TargetFilter f) =>
        f == TargetFilter.Any || (f == TargetFilter.Enemies ? u.IsPlayer != caster.IsPlayer : u.IsPlayer == caster.IsPlayer);

    // Celdas que se pueden clickear como punto de apuntado.
    public static List<Vector2Int> GetSelectableCells(Vector2Int from, AbilityData ab)
    {
        var grid = GridManager.Instance;
        var res = new List<Vector2Int>();
        if (IsSelfCentered(ab)) { res.Add(from); return res; }

        if (ab.shape == AreaShape.Line)
        {
            foreach (var d in GridManager.Dirs)
                for (int i = 1; i <= ab.lineLength; i++)
                {
                    var c = from + d * i;
                    if (grid.InBounds(c)) res.Add(c);
                }
            return res;
        }

        for (int dx = -ab.range; dx <= ab.range; dx++)
            for (int dy = -ab.range; dy <= ab.range; dy++)
            {
                if (Mathf.Abs(dx) + Mathf.Abs(dy) > ab.range) continue;
                var c = from + new Vector2Int(dx, dy);
                if (grid.InBounds(c)) res.Add(c);
            }
        return res;
    }

    // Todas las celdas afectadas al apuntar a 'aim'.
    public static List<Vector2Int> GetAffectedCells(Vector2Int from, Vector2Int aim, AbilityData ab)
    {
        var grid = GridManager.Instance;
        var cells = new List<Vector2Int>();
        switch (ab.shape)
        {
            case AreaShape.Single:
                cells.Add(aim);
                break;
            case AreaShape.Line:
            {
                var d = aim - from;
                Vector2Int dir = Mathf.Abs(d.x) >= Mathf.Abs(d.y)
                    ? new Vector2Int(System.Math.Sign(d.x), 0)
                    : new Vector2Int(0, System.Math.Sign(d.y));
                for (int i = 1; i <= ab.lineLength; i++)
                {
                    var c = from + dir * i;
                    if (!grid.InBounds(c)) break;
                    cells.Add(c);
                }
                break;
            }
            case AreaShape.Circle:
                for (int dx = -ab.radius; dx <= ab.radius; dx++)
                    for (int dy = -ab.radius; dy <= ab.radius; dy++)
                    {
                        if (Mathf.Abs(dx) + Mathf.Abs(dy) > ab.radius) continue;
                        var c = aim + new Vector2Int(dx, dy);
                        if (grid.InBounds(c)) cells.Add(c);
                    }
                break;
        }
        return cells;
    }

    public static bool IsValidAim(Unit caster, Vector2Int aim, AbilityData ab)
    {
        if (!GetSelectableCells(caster.Cell, ab).Contains(aim)) return false;
        if (ab.shape != AreaShape.Single) return true;
        var u = GridManager.Instance.GetUnitAt(aim);
        return u != null && !u.IsDead && Matches(caster, u, ab.targets);
    }

    public static List<Unit> CollectTargets(Unit caster, List<Vector2Int> cells, AbilityData ab)
    {
        var res = new List<Unit>();
        foreach (var c in cells)
        {
            var u = GridManager.Instance.GetUnitAt(c);
            if (u != null && !u.IsDead && Matches(caster, u, ab.targets) && !res.Contains(u)) res.Add(u);
        }
        return res;
    }

    public static int CalculateDamage(Unit caster, Unit target, AbilityData ab)
    {
        float stat = ab.damageType == DamageType.Magical ? caster.MagicPower : caster.AttackPower;
        float raw = stat * ab.power * Scale;
        float def = ab.damageType == DamageType.True ? 0f
                  : target.Data.defense * (ab.damageType == DamageType.Magical ? 0.5f : 1f);
        float dmg = raw * 20f / (20f + def) * Random.Range(0.9f, 1.1f);
        return Mathf.Max(1, Mathf.RoundToInt(dmg));
    }

    public static void Apply(Unit caster, List<Unit> targets, AbilityData ab)
    {
        foreach (var t in targets)
        {
            switch (ab.effect)
            {
                case AbilityEffect.Damage: t.TakeDamage(CalculateDamage(caster, t, ab)); break;
                case AbilityEffect.Heal:
                    t.Heal(ab.flatAmount > 0 ? ab.flatAmount : Mathf.RoundToInt(caster.MagicPower * ab.power * Scale)); break;
                case AbilityEffect.RestoreMP:
                    t.RestoreMP(ab.flatAmount > 0 ? ab.flatAmount : Mathf.RoundToInt(caster.MagicPower * ab.power)); break;
                case AbilityEffect.BuffAttack: t.ApplyAttackBuff(ab.buffMultiplier, ab.buffDuration); break;
            }
        }
    }
}
