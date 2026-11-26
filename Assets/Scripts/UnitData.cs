using System.Collections.Generic;
using UnityEngine;

public enum UnitRole { Tank, Ranger, Sorcerer, Commander, Enemy }

[CreateAssetMenu(menuName = "RPG/Unit", fileName = "NewUnit")]
public class UnitData : ScriptableObject
{
    public string displayName = "Unit";
    public UnitRole role = UnitRole.Tank;
    public bool isPlayerControlled = true;
    public GameObject prefab;           // opcional: si es null se usa una cápsula
    public Sprite portrait;

    [Header("Stats")]
    public int maxHP = 100;
    public int maxMP = 30;
    public int attack = 10;
    public int defense = 5;
    public int magic = 10;
    [Tooltip("Velocidad de llenado del ATB (1 = normal)")] public float speed = 1f;
    public int moveRange = 3;

    [Header("Acciones")]
    public AbilityData basicAttack;
    [Tooltip("Habilidades del menú 'Habilidad' (la especial va aquí)")]
    public List<AbilityData> abilities = new();
}
