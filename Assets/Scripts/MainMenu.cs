using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Serializable]
    public class CharacterOption
    {
        public UnitData data;      // Tank, Ranger, Sorcerer o Commander
        public Toggle toggle;
        public TMP_Text infoText;
    }

    public List<CharacterOption> options = new();
    public string battleScene = "Battle";
    public TMP_Text warningText;

    void Start()
    {
        foreach (var o in options)
        {
            o.toggle.isOn = true;
            o.infoText.text = Describe(o.data);
        }
        if (warningText) warningText.gameObject.SetActive(false);
    }

    static string Describe(UnitData d)
    {
        string special = d.abilities.Count > 0 && d.abilities[0] != null ? d.abilities[0].displayName : "-";
        return $"<b>{d.displayName}</b>\nHP {d.maxHP}  MP {d.maxMP}\nATQ {d.attack}  DEF {d.defense}  MAG {d.magic}\nVEL {d.speed:0.0}  MOV {d.moveRange}\nEspecial: {special}";
    }

    public void StartBattle()
    {
        PartySelection.Selected.Clear();
        foreach (var o in options) if (o.toggle.isOn) PartySelection.Selected.Add(o.data);
        if (PartySelection.Selected.Count == 0)
        {
            if (warningText) { warningText.text = "Elegí al menos un personaje."; warningText.gameObject.SetActive(true); }
            return;
        }
        SceneManager.LoadScene(battleScene);
    }

    public void Quit() => Application.Quit();
}
