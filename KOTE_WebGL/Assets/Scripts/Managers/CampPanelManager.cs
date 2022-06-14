using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampPanelManager : MonoBehaviour
{
   public GameObject CampContainer;
   private void Start()
   {
      CampContainer.SetActive(false);
   }

   private void OnCampSelected()
   {
      CampContainer.SetActive(true);
   }

   public void OnRestSelected()
   {
      //TODO add rest functionality
      CampContainer.SetActive(false);
   }

   public void OnSmithSelected()
   {
      //TODO add smithing functionality
      CampContainer.SetActive(false);
   }
}
