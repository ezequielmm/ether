using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class EnemyPrefab : MonoBehaviour
{
    public Transform intentMountingPoint;
    public Transform healthMountingPoint;
    public UnityEvent onCursorEnter;
    public UnityEvent onCursorExit;

    private void OnMouseEnter()
    {
        onCursorEnter.Invoke();
    }
    private void OnMouseExit()
    {
        onCursorExit.Invoke();
    }
}
