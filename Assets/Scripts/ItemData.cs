using UnityEngine;

// El efecto del ítem es una AbilityData (mpCost 0): reutiliza targeting, área y ejecución.
[CreateAssetMenu(menuName = "RPG/Item", fileName = "NewItem")]
public class ItemData : ScriptableObject
{
    public string displayName = "Item";
    public Sprite icon;
    public AbilityData effect;
}
