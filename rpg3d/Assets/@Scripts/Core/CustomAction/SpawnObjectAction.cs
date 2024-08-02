using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[System.Serializable]
public class SpawnObjectAction : CustomAction
{
    // 언제 Spawn
    private enum MethodType { Start, Run }

    // 누구를 기준으로 Spawn
    private enum TargetType { OwnerOrUser, Target }

    [SerializeField]
    private TargetType targetType;
    [SerializeField]
    private MethodType methodType;
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private Vector3 scaleFactor = Vector3.one;
    [SerializeField]
    private bool isAttachToTarget;
    [SerializeField]
    private bool isDestroyOnRelease;

    private GameObject spawnedObject;

    public override void Start(object data)
    {
        if (methodType == MethodType.Start)
            Spawn(data);
    }

    public override void Run(object data)
    {
        if (methodType == MethodType.Run)
            Spawn(data);
    }

    public override void Release(object data)
    {
        if (spawnedObject && isDestroyOnRelease)
            GameObject.Destroy(spawnedObject);
    }
    private GameObject Spawn(Vector3 position)
    {
        spawnedObject = GameObject.Instantiate(prefab);
        spawnedObject.transform.position = position + offset;
        var localScale = spawnedObject.transform.localScale;
        spawnedObject.transform.localScale = Vector3.Scale(localScale, scaleFactor);

        return spawnedObject;
    }

    private void Spawn(Transform transform)
    {
        var spawnedObject = Spawn(transform.position);
        if (isAttachToTarget)
            spawnedObject.transform.SetParent(transform, true);
    }

    //private void Spawn(Skill data)
    //{
    //    if (targetType == TargetType.OwnerOrUser)
    //        Spawn(data.Owner.transform);
    //    else
    //        Spawn(data.TargetSelectionResult.selectedPosition);
    //}

    private void Spawn(Effect data)
    {
        var targetTransform = targetType == TargetType.OwnerOrUser ? data.User.transform : data.Target.transform;
        Spawn(targetTransform);
    }

    private void Spawn(object data)
    {
        if (data is Skill)
            Spawn(data as Skill);
        else
            Spawn(data as Effect);
    }

    public override object Clone()
    {
        return new SpawnObjectAction()
        {
            isAttachToTarget = isAttachToTarget,
            isDestroyOnRelease = isDestroyOnRelease,
            methodType = methodType,
            targetType = targetType,
            offset = offset,
            prefab = prefab,
            scaleFactor = scaleFactor,
        };
    }
}
