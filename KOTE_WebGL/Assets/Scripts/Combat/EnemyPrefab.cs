using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyPrefab : MonoBehaviour
{
    public Transform intentMountingPoint;
    public Transform healthMountingPoint;
    public UnityEvent onCursorEnter;
    public UnityEvent onCursorExit;
    public SpineAnimationsManagement spineAnimationsManagement;
    public SortingGroup sortingGroup;
    public int sortingOrder = 1;
    public Bounds setBounds;
    
    public void InitEnemy(EnemyData data, Action fitTransformsCallback)
    {
        spineAnimationsManagement = GetComponentInChildren<SpineAnimationsManagement>();
        SkeletonAnimation skeleton = transform.GetComponentsInChildren<SkeletonAnimation>()[0];
        
        spineAnimationsManagement.SetProperties();
        SetSkin(data, skeleton, fitTransformsCallback);

        
        sortingGroup = skeleton.gameObject.AddComponent<SortingGroup>();
        sortingGroup.sortingOrder = sortingOrder;
    }

    private void SetSkin(EnemyData data, SkeletonAnimation skeleton, Action fitTransformsCallback)
    {
        SetEnemySkin(data.name switch
        {
            "Swarm Cocoon" => data.size.ToLower(),
            "Mossy Bones" => "polyp 01",
            "Deep Sorcerer Green" => "Green",
            "Deep Sorcerer Red" => "Red",
            _ => ""
        }, skeleton, fitTransformsCallback);
    }

    private void SetEnemySkin(string skinName, SkeletonAnimation skeleton, Action callback = null)
    {
        if (string.IsNullOrEmpty(skinName))
        {
            FitColliderToArt();
            callback?.Invoke();
            return;
        }
        
        if (callback != null)
        {        
            SkeletonRenderer.SkeletonRendererDelegate skinUpdated = _ =>
            {
                FitColliderToArt();
                callback?.Invoke();
            };
            callback += () => skeleton.OnMeshAndMaterialsUpdated -= skinUpdated;
        
            skeleton.OnMeshAndMaterialsUpdated += skinUpdated;
        }
        skeleton.skeleton.SetSkin(skinName);
    }

    [ContextMenu("FitColliderToArt")]
    public void FitColliderToArt()
    {
        spineAnimationsManagement.PlayAnimationSequence("Idle");
        // Remove transformations
        Vector3 originalPos = transform.position;
        transform.position = Vector3.zero;
        Quaternion originalRot = transform.rotation;
        transform.rotation = Quaternion.identity;
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.one;

        // Get Collider
        BoxCollider2D collider = GetComponentInChildren<BoxCollider2D>();

        // Find bounds of art
        Bounds bounds = new Bounds();
        bounds.center = transform.position;
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            // Renderer bounds is in world space
            bounds.Encapsulate(renderer.bounds);
        }

        // Adjust bounds
        //bounds.center = bounds.center;
        //bounds.size = bounds.size;

        // Set collider to bounds
        collider.offset = bounds.center + (Vector3.up * (GameSettings.INTENT_HEIGHT - GameSettings.HEALTH_HEIGHT) / originalScale.y);
        collider.size = bounds.size + (Vector3.up * (GameSettings.INTENT_HEIGHT + GameSettings.HEALTH_HEIGHT) / originalScale.y);

        setBounds = bounds;

        // refit intent and health to top and bottom of bounding box
        intentMountingPoint.localPosition = new Vector3(bounds.center.x, bounds.center.y + bounds.extents.y, intentMountingPoint.position.z);
        healthMountingPoint.localPosition = new Vector3(bounds.center.x, bounds.center.y - bounds.extents.y, healthMountingPoint.position.z);

        // Apply Transformations
        transform.position = originalPos;
        transform.rotation = originalRot;
        transform.localScale = originalScale;
    }

    private void OnMouseEnter()
    {
        onCursorEnter.Invoke();
    }
    private void OnMouseExit()
    {
        onCursorExit.Invoke();
    }
}
