using NUnit.Framework;
using UnityEngine;

namespace KOTE.UI.Armory
{
    public class GearIconManagerTests : MonoBehaviour
    {
        [Test]
        public void DoesDefaultImageExist()
        {
            Assert.NotNull(GearIconManager.Instance.defaultImage);
        }
    }
}