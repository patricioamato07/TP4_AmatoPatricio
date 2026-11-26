using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Control del jugador: menú (Atacar / Habilidad / Ítem / Defender / Mover), apuntado en grilla y límite de tiempo.
public class PlayerTurnController : MonoBehaviour
{
    public ActionMenuUI menu;
    public ATBSystem atb;
    public PartyInventory inventory;
    public IsometricCameraController cam;
    public Color moveColor = new Color(0.2f, 0.6f, 1f, 0.45f);
    public Color rangeColor = new Color(1f, 0.9f, 0.2f, 0.35f);
    public Color hoverColor = new Color(1f, 0.25f, 0.2f, 0.6f);

    enum Mode { Menu, Move, Target, Busy }
    Mode mode;
    Unit active;
    bool turnFinished;
    float timer;
    Dictionary<Vector2Int, Vector2Int> reachable;
    AbilityData pending;
    ItemData pendingItem;

    GridManager Grid => GridManager.Instance;

    void Awake()
    {
        menu.AttackChosen += () => BeginTargeting(active.Data.basicAttack, null);
        menu.AbilityChosen += a => BeginTargeting(a, null);
        menu.ItemChosen += i => BeginTargeting(i.effect, i);
        menu.MoveChosen += BeginMove;
        menu.DefendChosen += () => StartCoroutine(DoDefend());
    }

    public IEnumerator RunTurn(Unit unit)
    {
        active = unit;
        turnFinished = false;
        timer = atb.actionTimeLimit;
        mode = Mode.Menu;
        menu.Show(unit, inventory, true);

        while (!turnFinished)
        {
            if (mode != Mode.Busy)
            {
                timer -= Time.deltaTime;
                menu.SetTimer(timer / atb.actionTimeLimit);
                if (timer <= 0f)           // se acabó el tiempo: defensa forzada
                {
                    mode = Mode.Busy;
                    Grid.ClearAll();
                    menu.Hide();
                    yield return BattleManager.Instance.ExecuteDefend(unit);
                    break;
                }
            }
            yield return null;
        }
        Grid.ClearAll();
        menu.Hide();
        active = null;
    }

    // ---------- Entrada ----------
    void Update()
    {
        if (active == null || (mode != Mode.Move && mode != Mode.Target)) return;

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) { Cancel(); return; }

        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (overUI || !Grid.TryGetMouseCell(cam.Camera, out var cell)) { Grid.Clear(2); return; }

        if (mode == Mode.Move) HandleMove(cell); else HandleTarget(cell);
    }

    void HandleMove(Vector2Int cell)
    {
        if (cell == active.Cell || !reachable.ContainsKey(cell)) { Grid.Clear(2); return; }
        Grid.Highlight(2, Grid.BuildPath(reachable, cell), hoverColor);
        if (Input.GetMouseButtonDown(0)) StartCoroutine(DoMove(cell));
    }

    void HandleTarget(Vector2Int cell)
    {
        if (!AbilityResolver.IsValidAim(active, cell, pending)) { Grid.Clear(2); return; }
        Grid.Highlight(2, AbilityResolver.GetAffectedCells(active.Cell, cell, pending), hoverColor);
        if (Input.GetMouseButtonDown(0)) StartCoroutine(DoAction(cell));
    }

    void Cancel()
    {
        Grid.ClearAll();
        mode = Mode.Menu;
    }

    // ---------- Comandos ----------
    void BeginMove()
    {
        if (active.HasMoved) return;
        Grid.ClearAll();
        reachable = Grid.GetReachable(active.Cell, active.Data.moveRange);
        var cells = new List<Vector2Int>(reachable.Keys);
        cells.Remove(active.Cell);
        Grid.Highlight(0, cells, moveColor);
        mode = Mode.Move;
    }

    void BeginTargeting(AbilityData ab, ItemData item)
    {
        if (ab == null || active.CurrentMP < ab.mpCost) return;
        Grid.ClearAll();
        pending = ab;
        pendingItem = item;
        if (AbilityResolver.IsSelfCentered(ab)) { StartCoroutine(DoAction(active.Cell)); return; }
        Grid.Highlight(0, AbilityResolver.GetSelectableCells(active.Cell, ab), rangeColor);
        mode = Mode.Target;
    }

    IEnumerator DoMove(Vector2Int cell)
    {
        mode = Mode.Busy;
        var path = Grid.BuildPath(reachable, cell);
        Grid.ClearAll();
        atb.Paused = true;
        cam.Follow(active.transform);
        yield return active.Movement.MoveAlong(active, path);
        cam.FocusOn(active.transform.position);
        atb.Paused = false;
        active.HasMoved = true;
        menu.Show(active, inventory, false);   // sólo refresca; el timer sigue
        mode = Mode.Menu;
    }

    IEnumerator DoAction(Vector2Int aim)
    {
        mode = Mode.Busy;
        Grid.ClearAll();
        menu.Hide();
        if (pendingItem != null) inventory.TryConsume(pendingItem);
        yield return BattleManager.Instance.ExecuteAbility(active, aim, pending);
        turnFinished = true;
    }

    IEnumerator DoDefend()
    {
        mode = Mode.Busy;
        Grid.ClearAll();
        menu.Hide();
        yield return BattleManager.Instance.ExecuteDefend(active);
        turnFinished = true;
    }
}
