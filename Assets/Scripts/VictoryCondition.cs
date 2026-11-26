using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Condición de victoria: aniquilar enemigos. Derrota: todos los héroes caen.
// Para otros objetivos (sobrevivir N turnos, proteger a alguien) extender Evaluate.
public class VictoryCondition : MonoBehaviour
{
    public bool Evaluate(IReadOnlyList<Unit> players, IReadOnlyList<Unit> enemies, out bool victory)
    {
        bool anyEnemy = enemies.Any(e => !e.IsDead);
        bool anyPlayer = players.Any(p => !p.IsDead);
        if (!anyEnemy) { victory = true; return true; }
        if (!anyPlayer) { victory = false; return true; }
        victory = false;
        return false;
    }
}
