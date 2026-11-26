using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool IsMoving { get; private set; }

    public IEnumerator MoveAlong(Unit unit, List<Vector2Int> path)
    {
        if (path == null || path.Count == 0) yield break;
        IsMoving = true;
        GridManager.Instance.Move(unit, path[path.Count - 1]);   // reserva el destino de inmediato
        foreach (var cell in path)
        {
            Vector3 target = unit.WorldPos(cell);
            FaceTowards(target);
            while ((transform.position - target).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
        IsMoving = false;
    }

    public void FaceTowards(Vector3 worldPos)
    {
        var dir = worldPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(dir);
    }
}
