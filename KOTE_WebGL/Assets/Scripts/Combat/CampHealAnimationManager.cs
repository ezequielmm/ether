using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampHealAnimationManager : HealAnimationManager
{
   protected override void OnHeal(string who, int healAmount)
   {
      // if this was sent to the camp manager
      if (who == "camp")
      {
         // Run Animation
         StartCoroutine(HealthAnimation(healAmount));
      }
   }
}
