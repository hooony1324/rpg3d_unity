using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectStackAction
{
    // 몇 Stack일 때 적용할 지
    [SerializeField, Min(1)]
    private int stack;

    // 다음 Stack쌓일 때 이 Action을 날릴 지
    [SerializeField]
    private bool isReleaseOnNextApply;

    // 최초 1번만 적용할지의 여부
    // ex. true, 3스택에 효과 적용
    // 2스택 > 3스택(Apply&Release)> 2스택 > 3스택(적용되지 않는다)
    [SerializeField]
    private bool isApplyOnceInLifeTime;

    [UnderlineTitle("Action")]
    [SerializeReference, SubclassSelector]
    private EffectAction action;

    private bool hasEverApplied;
    public int Stack => stack;
    public bool IsReleaseOnNextApply => isReleaseOnNextApply;

    public bool IsApplicable => !isApplyOnceInLifeTime || (isApplyOnceInLifeTime && !hasEverApplied);

    public void Start(Effect effect, Entity user, Entity target, int level, float scale)
    => action.Start(effect, user, target, level, scale);

    public void Apply(Effect effect, int level, Entity user, Entity target, float scale)
    {
        action.Apply(effect, user, target, level, stack, scale);
        hasEverApplied = true;
    }

    public void Release(Effect effect, int level, Entity user, Entity target, float scale)
        => action.Release(effect, user, target, level, scale);

    public string BuildDescription(Effect effect, string baseDescription, int stackActionIndex, int effectIndex)
        => action.BuildDescription(effect, baseDescription, stackActionIndex, stack, effectIndex);
}
