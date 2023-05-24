using TMPro;
using UnityEngine;

public class LeaderboardListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text name;
    [SerializeField] private TMP_Text rank;
    [SerializeField] private TMP_Text score;

    public void SetData(string name, string rank, string score)
    {
        this.name.text = name;
        this.rank.text = rank;
        this.score.text = score;
    }
}
