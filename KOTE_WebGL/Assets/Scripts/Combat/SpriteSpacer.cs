using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSpacer : MonoBehaviour
{
    [SerializeField] List<GameObject> icons;
    List<GameObject> spacers;
    [Tooltip("Spacer Prefab. Leave blank for none.")]
    [SerializeField] GameObject spacer;
    GameObject container;
    [Tooltip("Space between all sprites.")]
    public float iconSpace = 0.1f;
    [HideInInspector] public float fadeSpeed = GameSettings.INTENT_FADE_SPEED;

    private void Awake()
    {
        spacers = new List<GameObject>();
        container = new GameObject();
        container.transform.SetParent(this.transform);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.name = "Sprite Container";
    }

    public void ReorganizeSprites() 
    {
        Quaternion lastRotation = container.transform.rotation;
        container.transform.rotation = Quaternion.identity;
        Vector3 lastPosition = container.transform.localPosition;
        container.transform.position = Vector3.zero;

        DestorySpacers();
        float length = 0;
        for(int i = 0; i < icons.Count; i++)
        {
            GameObject item = icons[i];

            RectTransform rectTransform = item.transform as RectTransform;
            var width = rectTransform.rect.width;
            
            item.transform.localPosition = new Vector3(length + (width/2), 0, 0);
            length += width;

            if (i != icons.Count - 1) 
            {
                length += iconSpace;
                if (spacer != null) 
                {
                    GameObject _spacer = Instantiate(spacer, container.transform);
                    spacers.Add(_spacer);
                    RectTransform spacerTransform = _spacer.transform as RectTransform;
                    var spaceWidth = spacerTransform.rect.width;

                    _spacer.transform.localPosition = new Vector3(length + (spaceWidth/2), 0, 0);
                    length += spaceWidth;
                    length += iconSpace;
                }
            }
        }

        lastPosition.x = -(length / 2);

        container.transform.localPosition = lastPosition;
        container.transform.rotation = lastRotation;
    }

    public void AddIcon(GameObject icon) 
    {
        icon.transform.SetParent(container.transform);
        icons.Add(icon);
    }

    public void RemoveIcon(GameObject icon) 
    {
        icons.Remove(icon);
        icon.transform.DOScale(0, fadeSpeed);
        StartCoroutine(DestoryAfterTime(icon));
    }

    private IEnumerator DestoryAfterTime(GameObject icon)
    {
        yield return new WaitForSeconds(fadeSpeed);
        Destroy(icon);
    }

    public void ClearIcons() 
    {
        DestorySpacers();
        for (int i = 0; i < icons.Count;) 
        {
            RemoveIcon(icons[i]);
        }
    }

    private void DestorySpacers() 
    {
        Debug.Log("Spacers Destroyed!");
        for (int i = 0; i < spacers.Count; i++) 
        {
            spacers[i].transform.DOScale(0, fadeSpeed);
            StartCoroutine(DestoryAfterTime(spacers[i]));
        }
        spacers.Clear();
    }

    public void SetSpacer(GameObject spacer) 
    {
        this.spacer = spacer;
        ReorganizeSprites();
    }
}
