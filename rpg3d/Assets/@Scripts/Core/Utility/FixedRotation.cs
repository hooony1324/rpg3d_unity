using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class FixedRotation : MonoBehaviour
{
    private Quaternion originRotation;

    private void Awake()
    {
        originRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        transform.rotation = originRotation;
    }
}
