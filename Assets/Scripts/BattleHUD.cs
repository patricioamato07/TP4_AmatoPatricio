using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleHUD : MonoBehaviour
{
    public UnitHUDPanel panelPrefab;
    public Transform partyContainer, enemyContainer;
    public GameObject resultPanel;
    public TMP_Text resultText, announceText;
    public string menuScene = "MainMenu";

    void Awake()
    {
        if (resultPanel) resultPanel.SetActive(false);
        Announce(null);
    }

    public void Build(IEnumerable<Unit> players, IEnumerable<Unit> enemies)
    {
        foreach (var u in players) Instantiate(panelPrefab, partyContainer).Bind(u, true);
        foreach (var u in enemies) Instantiate(panelPrefab, enemyContainer).Bind(u, false);
    }

    public void Announce(string text)
    {
        if (!announceText) return;
        announceText.gameObject.SetActive(!string.IsNullOrEmpty(text));
        announceText.text = text;
    }

    public void ShowResult(bool victory)
    {
        resultPanel.SetActive(true);
        resultText.text = victory ? "¡VICTORIA!" : "DERROTA";
    }

    public void Retry() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void BackToMenu() => SceneManager.LoadScene(menuScene);
}
