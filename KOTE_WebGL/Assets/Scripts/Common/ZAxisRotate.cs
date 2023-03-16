using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZAxisRotate : MonoBehaviour
{
    [SerializeField]
    public float Multiplier = 1f;

    // Update is called once per frame
    void Update()
    {
        Vector3 rotation = this.transform.localRotation.eulerAngles;
        rotation.z += Time.deltaTime * Multiplier;
        this.transform.localRotation = Quaternion.Euler(rotation);
    }
}
