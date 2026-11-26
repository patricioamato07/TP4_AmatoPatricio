#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

// Menú: RPG > Create Sample Data. Genera héroes, enemigos, habilidades e ítems de ejemplo en Assets/RPG/Data.
public static class SampleDataCreator
{
    const string Dir = "Assets/RPG/Data";

    [MenuItem("RPG/Create Sample Data")]
    public static void Create()
    {
        Directory.CreateDirectory(Dir);
        AssetDatabase.Refresh();

        // Ataques básicos
        var slash = Ab("Slash", "Golpe cuerpo a cuerpo.", 0, AbilityEffect.Damage, DamageType.Physical, 1f, TargetFilter.Enemies, AreaShape.Single, 1);
        var arrow = Ab("Arrow Shot", "Disparo a distancia.", 0, AbilityEffect.Damage, DamageType.Physical, 1f, TargetFilter.Enemies, AreaShape.Single, 4);
        var bolt = Ab("Magic Bolt", "Proyectil mágico.", 0, AbilityEffect.Damage, DamageType.Magical, 1f, TargetFilter.Enemies, AreaShape.Single, 3);
        var strike = Ab("Sword Strike", "Estocada de media distancia.", 0, AbilityEffect.Damage, DamageType.Physical, 1f, TargetFilter.Enemies, AreaShape.Single, 2);
        var claw = Ab("Claw", "Zarpazo.", 0, AbilityEffect.Damage, DamageType.Physical, 1f, TargetFilter.Enemies, AreaShape.Single, 1);
        var shoot = Ab("Bone Arrow", "Flecha de hueso.", 0, AbilityEffect.Damage, DamageType.Physical, 1f, TargetFilter.Enemies, AreaShape.Single, 4);
        var fist = Ab("Club Hit", "Garrotazo.", 0, AbilityEffect.Damage, DamageType.Physical, 1f, TargetFilter.Enemies, AreaShape.Single, 1);

        // Habilidades especiales
        var quake = Ab("Earthquake", "Daño en área alrededor del Tank (radio 1).", 8, AbilityEffect.Damage, DamageType.Physical, 1.2f, TargetFilter.Enemies, AreaShape.Circle, 0, radius: 1);
        var pierce = Ab("Piercing Shot", "Atraviesa 6 celdas en línea recta.", 10, AbilityEffect.Damage, DamageType.Physical, 1.4f, TargetFilter.Enemies, AreaShape.Line, 0, line: 6);
        var fire = Ab("Fireball", "Explosión de radio 1 hasta 4 celdas de distancia.", 14, AbilityEffect.Damage, DamageType.Magical, 1.6f, TargetFilter.Enemies, AreaShape.Circle, 4, radius: 1);
        var rally = Ab("Rally Cry", "Aliados en radio 2 ganan x1.5 ataque por 15s.", 10, AbilityEffect.BuffAttack, DamageType.True, 1f, TargetFilter.Allies, AreaShape.Circle, 0, radius: 2);
        var smash = Ab("Smash", "Golpe en área del Ogro.", 5, AbilityEffect.Damage, DamageType.Physical, 1.3f, TargetFilter.Enemies, AreaShape.Circle, 0, radius: 1);
        var volley = Ab("Bone Volley", "Disparo en línea de 5 celdas.", 6, AbilityEffect.Damage, DamageType.Physical, 1.2f, TargetFilter.Enemies, AreaShape.Line, 0, line: 5);

        // Ítems
        var potionFx = Ab("Potion", "Restaura 50 HP a un aliado cercano.", 0, AbilityEffect.Heal, DamageType.True, 1f, TargetFilter.Allies, AreaShape.Single, 2, flat: 50);
        var etherFx = Ab("Ether", "Restaura 20 MP a un aliado cercano.", 0, AbilityEffect.RestoreMP, DamageType.True, 1f, TargetFilter.Allies, AreaShape.Single, 2, flat: 20);
        Item("Potion", potionFx);
        Item("Ether", etherFx);

        // Héroes
        Unit("Tank", UnitRole.Tank, true, 180, 30, 12, 12, 4, 0.8f, 3, slash, quake);
        Unit("Ranger", UnitRole.Ranger, true, 100, 40, 14, 5, 6, 1.2f, 4, arrow, pierce);
        Unit("Sorcerer", UnitRole.Sorcerer, true, 80, 80, 4, 3, 18, 1.0f, 3, bolt, fire);
        Unit("Commander", UnitRole.Commander, true, 120, 50, 12, 8, 8, 1.0f, 3, strike, rally);

        // Enemigos
        Unit("Goblin", UnitRole.Enemy, false, 60, 0, 9, 3, 2, 0.9f, 3, claw);
        Unit("SkeletonArcher", UnitRole.Enemy, false, 45, 20, 10, 2, 2, 1.0f, 2, shoot, volley);
        Unit("Ogre", UnitRole.Enemy, false, 140, 20, 16, 6, 2, 0.6f, 2, fist, smash);

        AssetDatabase.SaveAssets();
        Debug.Log("RPG sample data creado en " + Dir);
    }

    static T Make<T>(string name) where T : ScriptableObject
    {
        string path = $"{Dir}/{name}.asset";
        var a = AssetDatabase.LoadAssetAtPath<T>(path);
        if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, path); }
        EditorUtility.SetDirty(a);
        return a;
    }

    static AbilityData Ab(string name, string desc, int mp, AbilityEffect eff, DamageType dt, float power,
        TargetFilter f, AreaShape shape, int range, int line = 4, int radius = 1, int flat = 0)
    {
        var a = Make<AbilityData>("Ability_" + name.Replace(" ", ""));
        a.displayName = name; a.description = desc; a.mpCost = mp; a.effect = eff; a.damageType = dt; a.power = power;
        a.targets = f; a.shape = shape; a.range = range; a.lineLength = line; a.radius = radius; a.flatAmount = flat;
        return a;
    }

    static void Item(string name, AbilityData fx)
    {
        var i = Make<ItemData>("Item_" + name);
        i.displayName = name; i.effect = fx;
    }

    static void Unit(string name, UnitRole role, bool player, int hp, int mp, int atk, int def, int mag,
        float speed, int move, AbilityData basic, AbilityData special = null)
    {
        var u = Make<UnitData>("Unit_" + name);
        u.displayName = name; u.role = role; u.isPlayerControlled = player;
        u.maxHP = hp; u.maxMP = mp; u.attack = atk; u.defense = def; u.magic = mag;
        u.speed = speed; u.moveRange = move; u.basicAttack = basic;
        u.abilities.Clear();
        if (special != null) u.abilities.Add(special);
    }
}
#endif
