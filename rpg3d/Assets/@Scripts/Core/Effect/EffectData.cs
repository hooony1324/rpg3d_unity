using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EffectData
{
    public int level;

    [UnderlineTitle("Stack")]
    [Min(1)]
    public int maxStack;

    public EffectStackAction[] stackActions;

    [UnderlineTitle("Action")]
    [SerializeReference, SubclassSelector]
    public EffectAction action;

    [UnderlineTitle("Setting")]
    public EffectRunningFinishOption runningFinishOption;

    // Effect의 지속 시간이 만료되었을 때, 남은 적용 횟수가 있다면 모두 적용할 것인지 여부
    public bool isApplyAllWhenDurationExpires;

    // 적용 시간, 횟수, 주기(0이면 무한)
    // ex. (0, 2, 1) -> 1초 동안 2번 적용(0초, 1초)
    public StatScaleFloat duration;
    [Min(0)]
    public int applyCount;
    [Min(0f)]
    public float applyCycle;

    // Effect에 다양한 연출 추가
    // ex. Particle Spawn, Sound, Camera Shake...
    [UnderlineTitle("Custom Action")]
    [SerializeReference, SubclassSelector]
    public CustomAction[] customActions;
}
