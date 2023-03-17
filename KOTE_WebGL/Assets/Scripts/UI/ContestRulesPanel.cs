using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContestRulesPanel : MonoBehaviour
{
    [SerializeField]
    public GameObject Panel;
    [SerializeField]
    public GameObject RulesContainer;

    [SerializeField]
    GameObject RulePrefab;

    private bool shownPanelPreviously = false;

    private const string StaticRules =
        "1. Each Leaderboard and Map lasts for 24 hours. These reset every 24 hours at UTC Midnight.\n" +
        "2. Villagers and Blessed Villagers get to complete 1 full expedition per 24 hour period. Knights get 2,"+
        " Genesis Knights get 3. A full expedition is defined as beating the Stage 1 boss and receiving loot. Each "+
        "character gets unlimited attempts each day to complete their allotted expeditions.\n" +
        "3. If you are in the middle of an expedition at UTC Midnight you will have up to 6 hours afterwards to"+
        " complete the expedition and receive rewards, after this time you will have to start a new expedition on the new daily map.";

    void Awake()
    {
        DisablePanel();
        Populate(StaticRules);
    }

    private void OnEnable()
    {
        if (!shownPanelPreviously && ContestManager.Instance.HasContest) 
        {
            shownPanelPreviously = true;
            EnablePanel();
        }
    }

    public void EnablePanel() 
    {
        TogglePannel(true);
    }
    public void DisablePanel() 
    {
        TogglePannel(false);
    }
    public void TogglePannel(bool newState) 
    {
        Panel.SetActive(newState);
    }

    public void OnContinueButton() 
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        DisablePanel();
    }

    public void Populate(string rulesBlerb) 
    {
        string[] rules = SplitIntoRows(rulesBlerb);
        SetRules(rules);
    }

    private string[] SplitIntoRows(string rules) 
    {
        return rules.Split(new char[] { '\n' });
    }

    private void SetRules(string[] rules) 
    {
        ClearRules();
        foreach (string rule in rules) 
        {
            AddRule(rule);
        }
    }

    private void ClearRules() 
    {
        foreach (Transform oldRule in RulesContainer.transform) 
        {
            Destroy(oldRule.gameObject);
        }
    }

    private void AddRule(string rule) 
    {
        GameObject Rule = Instantiate(RulePrefab, RulesContainer.transform);
        TMPro.TMP_Text RuleText = Rule.GetComponent<TMPro.TMP_Text>();
        RuleText.text = rule;
    }
}
