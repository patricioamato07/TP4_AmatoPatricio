using System.Collections;
using System.Linq;
using UnityEngine;

// IA enemiga: puntúa cada celda alcanzable.
//  - Normal: busca una celda desde la que pueda golpear; unidades a distancia prefieren quedarse al máximo alcance.
//  - Con poca vida: huye (sale del radio de ataque de los héroes) y sólo ataca si aún puede.
public class EnemyAI : MonoBehaviour
{
    [Range(0f, 1f)] public float retreatHpThreshold = 0.3f;
    [Range(0f, 1f)] public float abilityChance = 0.4f;
    public float thinkTime = 0.6f;

    public IEnumerator RunTurn(Unit e)
    {
        var bm = BattleManager.Instance;
        var grid = GridManager.Instance;
        var heroes = bm.PlayerUnits.Where(p => !p.IsDead).ToList();
        if (heroes.Count == 0) yield break;

        yield return new WaitForSeconds(thinkTime);

        AbilityData ab = ChooseAbility(e);
        Unit target = heroes.OrderBy(p => GridManager.Distance(e.Cell, p.Cell)).ThenBy(p => p.CurrentHP).First();
        bool retreat = e.CurrentHP <= e.Data.maxHP * retreatHpThreshold;

        // --- Mover ---
        var reach = grid.GetReachable(e.Cell, e.Data.moveRange);
        Vector2Int best = e.Cell;
        float bestScore = float.MinValue;
        foreach (var cell in reach.Keys)
        {
            float s = Score(cell, e, target, heroes, ab, retreat);
            if (s > bestScore) { bestScore = s; best = cell; }
        }
        if (best != e.Cell)
        {
            bm.atb.Paused = true;
            bm.cam.Follow(e.transform);
            yield return e.Movement.MoveAlong(e, grid.BuildPath(reach, best));
            bm.atb.Paused = false;
        }

        // --- Atacar: primero la habilidad elegida, si no el ataque básico ---
        foreach (var candidate in new[] { ab, e.Data.basicAttack }.Distinct())
        {
            if (candidate == null || e.CurrentMP < candidate.mpCost) continue;
            var victim = heroes.Where(h => !h.IsDead && CanHit(e.Cell, h, candidate))
                               .OrderBy(h => h.CurrentHP).FirstOrDefault();
            if (victim == null) continue;
            yield return bm.ExecuteAbility(e, Aim(e.Cell, victim, candidate), candidate);
            break;
        }
    }

    AbilityData ChooseAbility(Unit e)
    {
        var usable = e.Data.abilities.Where(a => a != null && a.effect == AbilityEffect.Damage && e.CurrentMP >= a.mpCost).ToList();
        if (usable.Count > 0 && Random.value < abilityChance) return usable[Random.Range(0, usable.Count)];
        return e.Data.basicAttack;
    }

    float Score(Vector2Int cell, Unit e, Unit target, System.Collections.Generic.List<Unit> heroes, AbilityData ab, bool retreat)
    {
        int dTarget = GridManager.Distance(cell, target.Cell);
        float s = 0f;
        bool canHit = CanHit(cell, target, ab);
        if (retreat)
        {
            s += heroes.Min(h => GridManager.Distance(cell, h.Cell)) * 3f;
            if (canHit) s += 5f;
        }
        else if (canHit)
        {
            s += 100f;
            int preferred = ab.shape == AreaShape.Line ? ab.lineLength : Mathf.Max(1, ab.range);
            s -= Mathf.Abs(dTarget - preferred);
        }
        else s -= dTarget;
        s -= GridManager.Distance(cell, e.Cell) * 0.01f;   // desempate: moverse poco
        return s;
    }

    static Vector2Int Aim(Vector2Int from, Unit target, AbilityData ab) =>
        AbilityResolver.IsSelfCentered(ab) ? from : target.Cell;

    static bool CanHit(Vector2Int from, Unit target, AbilityData ab)
    {
        var aim = Aim(from, target, ab);
        if (!AbilityResolver.GetSelectableCells(from, ab).Contains(aim)) return false;
        return AbilityResolver.GetAffectedCells(from, aim, ab).Contains(target.Cell);
    }
}
