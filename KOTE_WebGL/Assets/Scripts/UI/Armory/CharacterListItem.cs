using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CharacterListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text TokenNameText;
    [SerializeField] private TMP_Text CanPlayText;
    [SerializeField] private GameObject InitiatedLabel;
    [SerializeField] private CharacterPortraitManager portraitManager;

    public UnityEvent OnClick => GetComponent<Button>().onClick;
    public Nft Nft { get; private set; }

    public void SetCharacter(Nft character)
    {
        Nft = character;
        TokenNameText.text = character.FormatTokenName();
        InitiatedLabel.SetActive(character.IsInitiated);
        CanPlayText.text = character.CanPlay ? ""
            : $"Available in: {ParseTime((int)(character.PlayableAt - DateTime.UtcNow).TotalSeconds)}";
        CanPlayText.transform.parent.gameObject.SetActive(!character.CanPlay);
        portraitManager.SetPortrait(character);
    }

    //TODO: parse time like a normal person
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

    internal void SetSelected(bool selected)
    {
        portraitManager.SetSelected(selected);
    }
}
