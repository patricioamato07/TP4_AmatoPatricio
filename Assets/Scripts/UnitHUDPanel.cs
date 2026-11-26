using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Panel de un personaje: nombre, HP, MP y barra ATB.
public class UnitHUDPanel : MonoBehaviour
{
    public TMP_Text nameText, hpText, mpText;
    public Slider hpBar, mpBar, atbBar;
    public Image atbFill;
    public GameObject mpRow, atbRow;
    public CanvasGroup group;
    public Color atbFilling = new Color(0.2f, 0.8f, 1f), atbReady = new Color(1f, 0.85f, 0.1f);

    Unit unit;

    public void Bind(Unit u, bool showMpAndAtb)
    {
        unit = u;
        if (mpRow) mpRow.SetActive(showMpAndAtb);
        if (atbRow) atbRow.SetActive(showMpAndAtb);
        u.StatsChanged += Refresh;
        Refresh(u);
    }

    void OnDestroy() { if (unit != null) unit.StatsChanged -= Refresh; }

    void Refresh(Unit u)
    {
        if (nameText) nameText.text = u.Data.displayName + (u.IsDefending ? " (DEF)" : "");
        if (hpBar) { hpBar.maxValue = u.Data.maxHP; hpBar.value = u.CurrentHP; }
        if (hpText) hpText.text = $"{u.CurrentHP}/{u.Data.maxHP}";
        if (mpBar) { mpBar.maxValue = u.Data.maxMP; mpBar.value = u.CurrentMP; }
        if (mpText) mpText.text = $"{u.CurrentMP}/{u.Data.maxMP}";
        if (group) group.alpha = u.IsDead ? 0.35f : 1f;
    }

    void Update()
    {
        if (unit == null || atbBar == null) return;
        atbBar.value = unit.ATB;
        if (atbFill) atbFill.color = unit.IsReadyForTurn ? atbReady : atbFilling;
    }
}
