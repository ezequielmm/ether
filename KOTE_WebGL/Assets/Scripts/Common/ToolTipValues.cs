using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTipValues
{
    private static ToolTipValues instance;
    public static ToolTipValues Instance 
    {
        get 
        {
            if (instance == null) 
            {
                instance = new ToolTipValues();
            }
            return instance;
        }
    }

    public List<Tooltip> EnergyCounterTooltips => GenericToolTips["energy"];
    public List<Tooltip> DrawPileTooltips => GenericToolTips["draw"];
    public List<Tooltip> DiscardPileTooltips => GenericToolTips["discard"];
    public List<Tooltip> ExhaustPileTooltips => GenericToolTips["exhaust"];
    public List<Tooltip> EndTurnButtonTooltips => GenericToolTips["endTurn"];

    private Dictionary<string, List<Tooltip>> GenericToolTips;

    public List<Tooltip> GetTooltips(string listName) 
    {
        GenericToolTips.TryGetValue(listName, out List<Tooltip> list);
        return list;
    }

    private ToolTipValues() 
    {
        GenericToolTips = new Dictionary<string, List<Tooltip>>();
        GenericToolTips.Add("energy", new List<Tooltip>() { new Tooltip()
        {
            title = "Energy",
            description = "Your current energy count.\nCards require energy to play."
        }});
        GenericToolTips.Add("draw", new List<Tooltip>() { new Tooltip()
        {
            title = "Draw Pile",
            description = "At the start of each turn, 5 cards are drawn from here.\n\nClick to view the cards in your draw pile (shuffled)."
        }});
        GenericToolTips.Add("discard", new List<Tooltip>() { new Tooltip()
        {
            title = "Discard Pile",
            description = "If your draw pile is empty, the discard pile is shuffled into the draw pile.\n\nClick to view the cards in your discard pile."
        }});
        GenericToolTips.Add("exhaust", new List<Tooltip>() { new Tooltip()
        {
            title = "Exhausted Cards",
            description = "Click to view cards Exhausted this combat."
        }});
        GenericToolTips.Add("endTurn", new List<Tooltip>() { new Tooltip()
        {
            title = "End Turn",
            description = "Pressing this button will end your turn.\n\nYou will discard your hand, enemies will take their turn, you will draw 5 cards, then it will be your turn again."
        }});
        GenericToolTips.Add("settings", new List<Tooltip>() { new Tooltip()
        {
            title = "Settings",
            description = "Opens the game menu. Change or update your graphics, audio, and gameplay preferences here."
        }});
        GenericToolTips.Add("map", new List<Tooltip>() { new Tooltip()
        {
            title = "Map",
            description = "View the current Web 3.0 map."
        }});
        GenericToolTips.Add("deck", new List<Tooltip>() { new Tooltip()
        {
            title = "Deck",
            description = "View all of the cards in your deck."
        }});
        GenericToolTips.Add("health", new List<Tooltip>() { new Tooltip()
        {
            title = "Hit Points (HP)",
            description = "If you run out of HP, you die."
        }});
        GenericToolTips.Add("coins", new List<Tooltip>() { new Tooltip()
        {
            title = "Wallet",
            description = "How much gold you are carrying. Gold is the basic currency in Knights of The Ether."
        }});
        GenericToolTips.Add("noPotion", new List<Tooltip>() { new Tooltip()
        {
            title = "Wallet",
            description = "How much gold you are carrying. Gold is the basic currency in Knights of The Ether."
        }});
    }
}
