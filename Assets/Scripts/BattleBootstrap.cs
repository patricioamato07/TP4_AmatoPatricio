using System;
using System.Collections.Generic;
using UnityEngine;

// Instancia héroes (según PartySelection) y enemigos, y arranca la batalla.
public class BattleBootstrap : MonoBehaviour
{
    [Serializable] public class EnemySpawn { public UnitData data; public Vector2Int cell; }

    public BattleManager battle;
    public ATBSystem atb;
    public GridManager grid;
    public IsometricCameraController cam;

    [Tooltip("Se usa si se abre la escena Battle directamente sin pasar por el menú")]
    public List<UnitData> fallbackParty = new();
    public Vector2Int[] playerSpawns = { new(1, 2), new(1, 4), new(2, 3), new(2, 5) };
    public List<EnemySpawn> enemies = new();

    void Start()
    {
        var party = PartySelection.Selected.Count > 0 ? PartySelection.Selected : fallbackParty;
        for (int i = 0; i < party.Count; i++)
            battle.PlayerUnits.Add(Spawn(party[i], playerSpawns[i % playerSpawns.Length]));
        foreach (var e in enemies)
            battle.EnemyUnits.Add(Spawn(e.data, e.cell));

        cam.SetOverview(grid.Center);
        cam.Snap();
        battle.Begin();
    }

    Unit Spawn(UnitData d, Vector2Int cell)
    {
        GameObject go;
        float yOff = 0f;
        if (d.prefab) go = Instantiate(d.prefab);
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.GetComponent<Renderer>().material.color = ColorFor(d);
            yOff = 1f;
        }
        go.name = d.displayName;
        var u = go.GetComponent<Unit>();
        if (u == null) u = go.AddComponent<Unit>();
        u.yOffset = yOff;
        u.Init(d, cell);
        atb.Register(u);
        return u;
    }

    static Color ColorFor(UnitData d)
    {
        switch (d.role)
        {
            case UnitRole.Tank: return new Color(0.25f, 0.45f, 0.9f);
            case UnitRole.Ranger: return new Color(0.3f, 0.8f, 0.35f);
            case UnitRole.Sorcerer: return new Color(0.7f, 0.35f, 0.9f);
            case UnitRole.Commander: return new Color(0.95f, 0.8f, 0.2f);
            default: return new Color(0.85f, 0.2f, 0.2f);
        }
    }
}
