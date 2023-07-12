using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharactersListManager : MonoBehaviour
{
    [SerializeField] private GameObject listItemPrefab;
    [SerializeField] private Transform listContainer;
    [SerializeField] private int pageLength = 6;
    [SerializeField] private TMP_Text pagesCount;

    private int currentIndex = 0;
    private List<Nft> characters = new List<Nft>();

    public void Show(List<Nft> characters)
    {
        this.characters = characters;
        currentIndex = 0;
        ShowPage(currentIndex);
    }

    public void NextPage()
    {
        if (currentIndex + pageLength > characters.Count)
            currentIndex = 0;
        else
            currentIndex += pageLength;
        int page = currentIndex / pageLength + 1;
        pagesCount.text = $"{page}/{characters.Count / pageLength + 1}";
        ShowPage(currentIndex);
    }
    
    public void PreviousPage()
    {
        if (currentIndex == 0 && characters.Count > pageLength)
            currentIndex = pageLength * (characters.Count / pageLength);
        else
            currentIndex -= pageLength;
        if(currentIndex < 0)
            currentIndex = 0;
        int page = currentIndex / pageLength + 1;
        pagesCount.text = $"{page}/{characters.Count / pageLength + 1}";
        ShowPage(currentIndex); 
    }

    private void ShowPage(int startIndex)
    {
        ClearList();
        
        for(int i = startIndex; i < characters.Count && i < startIndex + pageLength; i++)
        {
            GameObject itemInstance = Instantiate(listItemPrefab, listContainer);
            var item = itemInstance.GetComponent<CharacterListItem>();
            item.SetCharacter(characters[i]);
            item.OnClick.AddListener(() => OnCharacterClick(item));
        }
    }

    private void ClearList()
    {
        var childs = listContainer.childCount;
        for (var i = childs - 1; i > 0; i--) 
        {
            Destroy(listContainer.GetChild(i).gameObject);
        }
    }

    private void OnCharacterClick(CharacterListItem item)
    {
        GameManager.Instance.NftSelected(item.Nft);
    }
}
