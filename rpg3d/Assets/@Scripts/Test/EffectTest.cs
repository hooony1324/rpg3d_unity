using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class EffectTest : MonoBehaviour
    {
        [SerializeField]
        private Effect testEffect;
        [SerializeField, Min(1)]
        private int effectLevel = 1;

        [ContextMenu("Instant Test")]
        private void InstantTest()
        {
            StartCoroutine("InstantTestCoroutine");
        }

        private IEnumerator InstantTestCoroutine()
        {
            Debug.Log("<color=yellow>[InstantTest] Start</color>");

            // Skill이 보관할 Effect 사본 생성
            var original = testEffect.Clone() as Effect;
            original.Setup(this, GetComponent<Entity>(), effectLevel, 1f);

            // Target에게 적용할 Skill이 보관중인 Effect 사본을 생성
            var clone = original.Clone() as Effect;
            clone.SetTarget(GetComponent<Entity>());

            Debug.Log(original.Description);
            Debug.Log($"User: {clone.User.name}, Target: {clone.Target.name}");

            clone.Start();

            if (clone.IsApplicable)
                clone.Apply();

            while (!clone.IsFinished)
            {
                yield return null;
                clone.Update();
            }

            clone.Release();

            Destroy(original);
            Destroy(clone);

            Debug.Log("<color=green>[InstantTest] Complete</color>");
        }

        [ContextMenu("Stack Test")]
        private void StackTest()
        {
            StartCoroutine("StackTestCoroutine");
        }

        private IEnumerator StackTestCoroutine()
        {
            Debug.Log("<color=yellow>[StackTest] Start</color>");

            // Skill이 보관할 Effect 사본 생성
            var original = testEffect.Clone() as Effect;
            original.Setup(this, GetComponent<Entity>(), effectLevel);

            // Target에게 적용할 Skill이 보관중인 Effect 사본을 생성
            var clone = original.Clone() as Effect;
            clone.SetTarget(GetComponent<Entity>());

            Debug.Log(original.Description);
            Debug.Log($"User: {clone.User.name}, Target: {clone.Target.name}");

            clone.Start();

            for (int i = 0; i < 4; i++)
            {
                yield return new WaitForSeconds(1f);
                Debug.Log("<color=green>Stack +1</color>");
                clone.CurrentStack++;
            }

            for (int i = 0; i < 4; i++)
            {
                yield return new WaitForSeconds(1f);
                Debug.Log("<color=yellow>Stack -1</color>");
                clone.CurrentStack--;
            }

            clone.Release();

            Destroy(original);
            Destroy(clone);

            Debug.Log("<color=green>[StackTest] Complete</color>");
        }
    }
}