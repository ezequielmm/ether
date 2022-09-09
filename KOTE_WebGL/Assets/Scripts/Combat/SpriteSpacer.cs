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
    [HideInInspector] public float fadeSpeed = 1;
    public ContentAlign contentAlign = ContentAlign.Center;
    public Display display = Display.Horizontal;
    public bool fadeOnCreate = false;

    public enum ContentAlign 
    {
        Left = -1,
        Center = 0,
        Right = 1,
        Top = -1,
        Bottom = 1
    }
    public enum Display
    {
        Horizontal,
        Vertical
    }

    private void Awake()
    {
        spacers = new List<GameObject>();
        container = new GameObject();
        container.transform.SetParent(this.transform);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.name = "Sprite Container";
    }

    public void SetFadeSpeed(float value) 
    {
        fadeSpeed = value;
    }

    public void ReorganizeSprites() 
    {
        // Set transformations to zero
        Quaternion lastRotation = container.transform.rotation;
        container.transform.rotation = Quaternion.identity;
        Vector3 lastPosition = container.transform.localPosition;
        container.transform.position = Vector3.zero;


        int spacersNeeded = 0;
        float length = 0;
        for (int i = 0; i < icons.Count; i++)
        {
            GameObject item = icons[i];

            bool newItem = item.transform.localPosition == -Vector3.one;

            RectTransform rectTransform = item.transform as RectTransform;
            var width = rectTransform.rect.width;
            var desiredlocalPosition = new Vector3(length + (width / 2), 0, 0);

            if (display == Display.Vertical)
            {
                width = rectTransform.rect.height;
                desiredlocalPosition = new Vector3(0, length + (width / 2), 0);
            }

            length += width;

            if (newItem)
            {
                item.transform.localPosition = desiredlocalPosition;
                if (fadeOnCreate)
                {
                    item.transform.localScale = Vector3.zero;
                    item.transform.DOScale(1, fadeSpeed);
                }
            }
            else 
            {
                item.transform.DOLocalMove(desiredlocalPosition, fadeSpeed).SetEase(Ease.InOutCirc);
            }

            if (i != icons.Count - 1)
            {
                length += iconSpace;
                if (spacer != null)
                {
                    spacersNeeded++;
                    if (spacers.Count < spacersNeeded)
                    {
                        length = CreateSpacer(length);
                    }
                    else 
                    {
                        length += GetSpacerLength();
                    }
                }
            }
        }

        for (int i = spacers.Count; i > spacersNeeded; i--) 
        {
            DestorySpacer(spacers[i-1]);
        }


        // Place based on content size
        switch (display)
        {
            case Display.Horizontal:
                switch (contentAlign)
                {
                    case ContentAlign.Left:
                        // Alight Left by design
                        break;
                    case ContentAlign.Center:
                        lastPosition.x = -(length / 2);
                        break;
                    case ContentAlign.Right:
                        lastPosition.x = -length;
                        break;
                }
                break;
            case Display.Vertical:
                switch (contentAlign)
                {
                    case ContentAlign.Top:
                        // Alight Top by design
                        break;
                    case ContentAlign.Center:
                        lastPosition.y = -(length / 2);
                        break;
                    case ContentAlign.Bottom:
                        lastPosition.y = -length;
                        break;
                }
                break;
        }

        // Return transformation back
        container.transform.localPosition = lastPosition;
        container.transform.rotation = lastRotation;
    }

    private float GetSpacerLength() 
    {
        RectTransform spacerTransform = spacer.transform as RectTransform;
        var spaceWidth = spacerTransform.rect.width;
        float length = 0;

        length += spaceWidth;
        length += iconSpace;

        return length;
    }

    private float CreateSpacer(float length) 
    {
        GameObject _spacer = Instantiate(spacer, container.transform);
        spacers.Add(_spacer);
        RectTransform spacerTransform = _spacer.transform as RectTransform;
        var spaceWidth = spacerTransform.rect.width;
        _spacer.transform.localPosition = new Vector3(length + (spaceWidth / 2), 0, 0);

        if (display == Display.Vertical)
        {
            spaceWidth = spacerTransform.rect.height;
            _spacer.transform.localPosition = new Vector3(0, length + (spaceWidth / 2), 0);
        }

        length += spaceWidth;
        length += iconSpace;

        if (fadeOnCreate)
        {
            _spacer.transform.localScale = Vector3.zero;
            _spacer.transform.DOScale(1, fadeSpeed);
        }

        return length;
    }

    public void CreateSprites() 
    {
        // Set transformations to zero
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
            item.transform.localPosition = new Vector3(length + (width / 2), 0, 0);

            if (display == Display.Vertical) 
            {
                width = rectTransform.rect.height;
                item.transform.localPosition = new Vector3(0, length + (width / 2), 0);
            }
            
            length += width;

            if (fadeOnCreate)
            {
                item.transform.localScale = Vector3.zero;
                item.transform.DOScale(1, fadeSpeed);
            }

            if (i != icons.Count - 1) 
            {
                length += iconSpace;
                if (spacer != null) 
                {
                    length = CreateSpacer(length);
                }
            }
        }

        // Place based on content size
        switch (display) 
        {
            case Display.Horizontal:
                switch (contentAlign)
                {
                    case ContentAlign.Left:
                        // Alight Left by design
                        break;
                    case ContentAlign.Center:
                        lastPosition.x = -(length / 2);
                        break;
                    case ContentAlign.Right:
                        lastPosition.x = -length;
                        break;
                }
                break;
            case Display.Vertical:
                switch (contentAlign)
                {
                    case ContentAlign.Top:
                        // Alight Top by design
                        break;
                    case ContentAlign.Center:
                        lastPosition.y = -(length / 2);
                        break;
                    case ContentAlign.Bottom:
                        lastPosition.y = -length;
                        break;
                }
                break;
        }

        // Return transformation back
        container.transform.localPosition = lastPosition;
        container.transform.rotation = lastRotation;
    }

    public void AddIcon(GameObject icon) 
    {
        icon.transform.SetParent(container.transform);
        icons.Add(icon);
        icon.transform.localPosition = -Vector3.one;
    }

    public void DeleteIcon(GameObject icon) 
    {
        icons.Remove(icon);
        icon.transform.DOScale(0, fadeSpeed).OnComplete(() => 
        {
            Destroy(icon);
        });
    }

    public void ClearIcons() 
    {
        DestorySpacers();
        for (int i = 0; i < icons.Count;) 
        {
            DeleteIcon(icons[i]);
        }
    }

    private void DestorySpacers() 
    {
        for (int i = 0; i < spacers.Count;) 
        {
            DestorySpacer(spacers[i]);
        }
    }

    private void DestorySpacer(GameObject spacer) 
    {
        spacers.Remove(spacer);
        spacer.transform.DOScale(0, fadeSpeed).OnComplete(() =>
        {
            Destroy(spacer);
        });
    }

    public void SetSpacer(GameObject spacer) 
    {
        this.spacer = spacer;
        CreateSprites();
    }
}
