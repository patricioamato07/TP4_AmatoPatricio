using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public ATBSystem atb;
    public PlayerTurnController playerTurn;
    public EnemyAI enemyAI;
    public IsometricCameraController cam;
    public VictoryCondition victory;
    public BattleHUD hud;
    public Color areaColor = new Color(1f, 0.25f, 0.2f, 0.55f);

    public readonly List<Unit> PlayerUnits = new();
    public readonly List<Unit> EnemyUnits = new();
    public bool BattleOver { get; private set; }

    void Awake() { Instance = this; }

    public void Begin()
    {
        hud.Build(PlayerUnits, EnemyUnits);
        StartCoroutine(BattleLoop());
    }

    IEnumerator BattleLoop()
    {
        yield return new WaitForSeconds(0.5f);
        while (!BattleOver)
        {
            if (atb.TryDequeue(out var unit))
            {
                unit.BeginTurn();
                cam.FocusOn(unit.transform.position);
                if (unit.IsPlayer) yield return playerTurn.RunTurn(unit);
                else yield return enemyAI.RunTurn(unit);

                atb.ResetGauge(unit);
                if (CheckEnd()) yield break;
                cam.ShowOverview();
            }
            yield return null;
        }
    }

    bool CheckEnd()
    {
        if (!victory.Evaluate(PlayerUnits, EnemyUnits, out bool won)) return false;
        BattleOver = true;
        atb.Paused = true;
        GridManager.Instance.ClearAll();
        hud.ShowResult(won);
        return true;
    }

    public IEnumerator ExecuteDefend(Unit unit)
    {
        atb.Paused = true;
        unit.Defend();
        hud.Announce("Defender");
        yield return new WaitForSeconds(0.5f);
        hud.Announce(null);
        atb.Paused = false;
    }

    // Núcleo de acción: sirve para ataque básico, habilidades e ítems.
    public IEnumerator ExecuteAbility(Unit caster, Vector2Int aim, AbilityData ab)
    {
        if (!caster.TrySpendMP(ab.mpCost)) yield break;
        var grid = GridManager.Instance;
        atb.Paused = true;

        caster.Movement.FaceTowards(grid.CellToWorld(aim));
        cam.FocusOn(caster.transform.position);
        hud.Announce(ab.displayName);
        yield return new WaitForSeconds(0.5f);

        var cells = AbilityResolver.GetAffectedCells(caster.Cell, aim, ab);
        grid.Highlight(1, cells, areaColor);
        var targets = AbilityResolver.CollectTargets(caster, cells, ab);

        var pts = new List<Vector3> { caster.transform.position };
        foreach (var c in cells) pts.Add(grid.CellToWorld(c));
        cam.FocusOnPoints(pts);
        yield return new WaitForSeconds(0.6f);

        if (ab.vfxPrefab)
            foreach (var c in cells) Destroy(Instantiate(ab.vfxPrefab, grid.CellToWorld(c), Quaternion.identity), 2f);

        AbilityResolver.Apply(caster, targets, ab);
        if (ab.effect == AbilityEffect.Damage) cam.Shake(0.25f, 0.18f);
        yield return new WaitForSeconds(0.7f);

        grid.Clear(1);
        hud.Announce(null);
        atb.Paused = false;
    }
}
