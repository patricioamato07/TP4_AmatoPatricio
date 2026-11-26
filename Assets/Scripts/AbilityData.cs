using UnityEngine;

public enum AbilityEffect { Damage, Heal, RestoreMP, BuffAttack }
public enum DamageType { Physical, Magical, True }
public enum TargetFilter { Enemies, Allies, Any }
public enum AreaShape { Single, Line, Circle }

[CreateAssetMenu(menuName = "RPG/Ability", fileName = "NewAbility")]
public class AbilityData : ScriptableObject
{
    public string displayName = "Ability";
    [TextArea] public string description;
    public int mpCost;
    public AbilityEffect effect = AbilityEffect.Damage;
    public DamageType damageType = DamageType.Physical;
    public TargetFilter targets = TargetFilter.Enemies;
    [Tooltip("Multiplicador sobre ataque o magia del lanzador")] public float power = 1f;
    [Tooltip("Si > 0 ignora stats y usa este valor fijo (pociones)")] public int flatAmount;

    [Header("Forma / alcance")]
    public AreaShape shape = AreaShape.Single;
    [Tooltip("Distancia Manhattan del lanzador al punto elegido. Circle con range 0 = centrado en el lanzador")]
    public int range = 1;
    [Tooltip("Line: largo en celdas (atraviesa a todos)")] public int lineLength = 4;
    [Tooltip("Circle: radio Manhattan en celdas")] public int radius = 1;

    [Header("Buff")]
    public float buffMultiplier = 1.5f;
    public float buffDuration = 15f;

    [Header("FX")]
    public GameObject vfxPrefab;
}
