using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class EffectAction : ICloneable
{
    // Effect 시작
    public virtual void Start(Effect effect, Entity user, Entity target, int level, float scale) { }

    // Effect 효과
    public abstract bool Apply(Effect effect, Entity user, Entity target, int level, int stack, float scale);

    // Effect가 종료
    public virtual void Release(Effect effect, Entity user, Entity target, int level, float scale) { }

    // Effect의 Stack이 바뀌었을 때 호출
    // Stack마다 Bonus값 있는 Action의 경우, 이 함수로 값 갱신
    public virtual void OnEffectStackChanged(Effect effect, Entity user, Entity target, int level, int stack, float scale) { }

    protected virtual IReadOnlyDictionary<string/*Text Mark*/, string/*Text*/> GetStringsByKeyword(Effect effect) => null;

    public string BuildDescription(Effect effect, string description, int stackActionIndex, int stack, int effectIndex)
    {
        var stringsByKeyword = GetStringsByKeyword(effect);
        if (stringsByKeyword == null)
            return description;

        if (stack == 0)
            // ex. description = "적에게 $[EffectAction.defaultDamage.0] 피해를 줍니다."
            // description.Replace("$[EffectAction.defaultDamage.0]", "300") => "적에게 300 피해를 줍니다."
            description = TextReplacer.Replace(description, "effectAction", stringsByKeyword, effectIndex.ToString());
        else
            // Mark = $[EffectAction.Keyword.StackActionIndex.Stack.EffectIndex]
            description = TextReplacer.Replace(description, "effectAction", stringsByKeyword, $"{stackActionIndex}.{stack}.{effectIndex}");

        return description;
    }


    public abstract object Clone();
}
