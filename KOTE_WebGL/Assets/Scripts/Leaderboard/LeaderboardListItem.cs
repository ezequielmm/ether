using System;
using DefaultNamespace.Leaderboard;
using DefaultNamespace.Leaderboard.New;
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
        time.text = ParseTime(player.totalTime);
        score.text = player.score.ToString();
        background.enabled = activeBg;
    }

    private string ParseName(string playerAddress)
    {
        if (playerAddress.Length == 42 && playerAddress.StartsWith("0x"))
            return $"{playerAddress.Substring(0, 6)}...{playerAddress.Substring(playerAddress.Length - 4, 4)}";

        return playerAddress;
    }

    public string ParseTime(int totalSeconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

        // Si el tiempo es menor a una hora, formatear como minutos y segundos.
        if (time.TotalHours < 1)
        {
            return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }
        // Si el tiempo es menor a un día pero mayor o igual a una hora, formatear como horas y minutos.
        else if (time.TotalDays < 1)
        {
            return $"{time.Hours}Hr {time.Minutes:D2}m";
        }
        // Si el tiempo es mayor o igual a un día, formatear como días, horas y minutos.
        else
        {
            return $"{time.Days}Ds {time.Hours}Hr";
        }
    }
}
