using System.Collections.Generic;
using UnityEngine;

// Active Time Battle: el gauge se llena con 'speed'. Se pausa SOLO durante la ejecución de acciones/animaciones.
// Mientras el jugador decide, los demás siguen llenándose (modo Active) y el jugador tiene 'actionTimeLimit'.
public class ATBSystem : MonoBehaviour
{
    [Tooltip("Segundos para llenar el gauge con speed = 1")] public float secondsToFill = 8f;
    [Tooltip("Tiempo máximo que tiene el jugador para actuar; si se agota, el personaje se defiende")] public float actionTimeLimit = 10f;
    public bool Paused { get; set; }

    readonly List<Unit> units = new();
    readonly Queue<Unit> ready = new();

    public void Register(Unit u) { units.Add(u); u.ATB = Random.Range(0f, 0.35f); }

    public void ResetGauge(Unit u) { u.ATB = 0f; u.IsReadyForTurn = false; }

    public bool TryDequeue(out Unit unit)
    {
        while (ready.Count > 0)
        {
            unit = ready.Dequeue();
            if (!unit.IsDead) return true;
        }
        unit = null;
        return false;
    }

    void Update()
    {
        if (Paused) return;
        float dt = Time.deltaTime;
        foreach (var u in units)
        {
            if (u.IsDead) continue;
            u.TickBuff(dt);
            if (u.IsReadyForTurn) continue;
            u.ATB = Mathf.Min(1f, u.ATB + dt * u.Data.speed / secondsToFill);
            if (u.ATB >= 1f) { u.IsReadyForTurn = true; ready.Enqueue(u); }
        }
    }
}
