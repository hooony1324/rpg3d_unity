using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    [System.Serializable]
    public class TestEffectAction : EffectAction
    {
        [SerializeField]
        private int value;

        private int increaseValue;

        public override bool Apply(Effect effect, Entity user, Entity target, int level, int stack, float scale)
        {
            increaseValue = value * stack;

            Debug.Log($"Effect: {effect.CodeName} Apply - User: {user.name}, Target: {target.name}, Scale: {scale}, Stack: {stack}");
            Debug.Log($"함 {increaseValue} 증가 / {value} * Stack = {increaseValue}");

            return true;
        }

        public override void Release(Effect effect, Entity user, Entity target, int level, float scale)
        {
            Debug.Log($"Effect: {effect.CodeName} Release");
            Debug.Log($"함 {increaseValue} 감소");
        }

        public override void OnEffectStackChanged(Effect effect, Entity user, Entity target, int level, int stack, float scale)
        {
            Debug.Log($"Effect: {effect.CodeName}, New Stack: {stack}");
            Release(effect, user, target, level, scale);
            Apply(effect, user, target, level, stack, scale);
        }

        protected override IReadOnlyDictionary<string, string> GetStringsByKeyword(Effect effect)
        {
            var stringsByKeyword = new Dictionary<string, string>();
            stringsByKeyword["value"] = value.ToString();
            return stringsByKeyword;
        }

        public override object Clone()
            => new TestEffectAction() { value = value };
    }
}