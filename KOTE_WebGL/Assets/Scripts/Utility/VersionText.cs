using System;
using TMPro;
using UnityEngine;

namespace DefaultNamespace.Utility
{
    public class VersionText : MonoBehaviour
    {
        private void Start()
        {
            if (TryGetComponent<TextMeshProUGUI>(out var text))
            {
                text.text = Application.version;
            }
        }
    }
}