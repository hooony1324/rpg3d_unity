using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EntityDefaultState : State<Entity>
{
    public override bool OnReceiveMessage(int message, object data)
    {
        if ((EntityStateMessage)message != EntityStateMessage.UsingSkill)
            return false;

        //var tupleData = ((Skill skill, AnimatorParameter animatorParameter))data;
        //Entity.Animator?.SetTrigger(tupleData.Item2.Hash);

        return true;
    }
}
