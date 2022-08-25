using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampPanelManager : MonoBehaviour
{
   public GameObject CampContainer;
   private void Start()
   {
      GameManager.Instance.EVENT_SHOW_CAMP_PANEL.AddListener(ShowCampPanel);
   }

   private void ShowCampPanel()
   {
      CampContainer.SetActive(true);
   }

   public void OnRestSelected()
   {
      //TODO add rest functionality
      CampContainer.SetActive(false);
      GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
   }

   public void OnSmithSelected()
   {
      //TODO add smithing functionality
      CampContainer.SetActive(false);
      GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
   }
}
