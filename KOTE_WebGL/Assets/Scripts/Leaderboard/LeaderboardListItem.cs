using DefaultNamespace.Leaderboard;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text rank;
    [SerializeField] private TMP_Text name;
    [SerializeField] private TMP_Text time;
    [SerializeField] private TMP_Text score;
    [Space]
    [SerializeField] private Image background;
    
    
    public void SetData(DataItem player, bool activeBg, int rank)
    {
        this.rank.text = rank.ToString("00");
        name.text = ParseName(player.address);
        time.text = "00:00:00";
        score.text = player.score.ToString();
        background.enabled = activeBg;
    }

    private string ParseName(string playerAddress)
    {
        if (playerAddress.Length == 42 && playerAddress.StartsWith("0x"))
            return $"{playerAddress.Substring(0, 6)}...{playerAddress.Substring(playerAddress.Length - 4, 4)}";

        return playerAddress;
    }
}
