using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Poner este script en un objeto SIEMPRE activo (p. ej. el Canvas); 'root' es el panel que se muestra/oculta.
public class ActionMenuUI : MonoBehaviour
{
    public GameObject root;
    public TMP_Text titleText;
    public Button attackBtn, abilityBtn, itemBtn, defendBtn, moveBtn;
    public Transform subRoot;          // contenedor (VerticalLayoutGroup) de la lista de habilidades/ítems
    public Button subButtonPrefab;     // Button con un TMP_Text hijo
    public Slider timerBar;

    public event Action AttackChosen, DefendChosen, MoveChosen;
    public event Action<AbilityData> AbilityChosen;
    public event Action<ItemData> ItemChosen;

    Unit unit;
    PartyInventory inv;

    void Awake()
    {
        attackBtn.onClick.AddListener(() => { CloseSub(); AttackChosen?.Invoke(); });
        defendBtn.onClick.AddListener(() => { CloseSub(); DefendChosen?.Invoke(); });
        moveBtn.onClick.AddListener(() => { CloseSub(); MoveChosen?.Invoke(); });
        abilityBtn.onClick.AddListener(ShowAbilities);
        itemBtn.onClick.AddListener(ShowItems);
        root.SetActive(false);
    }

    public void Show(Unit u, PartyInventory inventory, bool canMove)
    {
        unit = u;
        inv = inventory;
        root.SetActive(true);
        if (titleText) titleText.text = u.Data.displayName;
        moveBtn.interactable = canMove;
        CloseSub();
    }

    public void Hide() { CloseSub(); root.SetActive(false); }
    public void SetTimer(float normalized) { if (timerBar) timerBar.value = Mathf.Clamp01(normalized); }

    public void CloseSub()
    {
        for (int i = subRoot.childCount - 1; i >= 0; i--) Destroy(subRoot.GetChild(i).gameObject);
        subRoot.gameObject.SetActive(false);
    }

    void ShowAbilities()
    {
        CloseSub();
        subRoot.gameObject.SetActive(true);
        foreach (var ab in unit.Data.abilities)
        {
            var a = ab;
            var b = Instantiate(subButtonPrefab, subRoot);
            b.GetComponentInChildren<TMP_Text>().text = $"{a.displayName}  ({a.mpCost} MP)";
            b.interactable = unit.CurrentMP >= a.mpCost;
            b.onClick.AddListener(() => { CloseSub(); AbilityChosen?.Invoke(a); });
        }
    }

    void ShowItems()
    {
        CloseSub();
        subRoot.gameObject.SetActive(true);
        foreach (var slot in inv.slots)
        {
            if (slot.item == null || slot.count <= 0) continue;
            var it = slot.item;
            var b = Instantiate(subButtonPrefab, subRoot);
            b.GetComponentInChildren<TMP_Text>().text = $"{it.displayName}  x{slot.count}";
            b.onClick.AddListener(() => { CloseSub(); ItemChosen?.Invoke(it); });
        }
    }
}
