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
    private const string StaticRules = "1. Each character can only win once a Map.\n" +
        "2. Map refeshes at UTC midnight.\n" +
        "3. A character gets only 1 reward for each of the major nodes: Elite & Boss.\n" +
        "4. If you finish a map after UTC midnight it will be valid for the daily " +
        "contest up to 6 hours after UTC midnight. After that YOU GET NOTHING (except the joy of playing.)";

    void Awake()
    {
        DisablePanel();
        Populate(StaticRules);
    }

    private void OnEnable()
    {
        if (!shownPanelPreviously) 
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
