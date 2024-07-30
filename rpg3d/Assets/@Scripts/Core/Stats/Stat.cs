using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Stat : IdentifiedObject
{
    public delegate void ValueChangedHandler(Stat stat, float currentValue, float prevValue);

    [SerializeField]
    private bool isPercentType;
    [SerializeField]
    private float maxValue;
    [SerializeField]
    private float minValue;
    [SerializeField]
    private float defaultValue;

    /// <summary>
    /// Giver: 추가 스탯을 부여한 대상(장비, 버프 등)
    /// Subkey: 동일Giver가 여러 스탯 증가치를 부여할 때, 각각 구분하기 위한 용도, 구분 필요없으면 string.Empty
    ///          [Giver]      [Subkey]  [Value]
    ///       AttackSkill_1,   Stack_3,    5
    ///       AttackSkill_1,   Stack_5,    10
    /// Value: 추가 수치
    /// </summary>
    private Dictionary<object/*Giver*/, Dictionary<object/*Subkey*/, float/*Value*/>> bonusValuesByKey = new();

    public bool IsPercentType => isPercentType;
    public float MaxValue
    {
        get => maxValue;
        set => maxValue = value;
    }

    public float MinValue
    {
        get => minValue;
        set => minValue = value;
    }

    public float DefaultValue
    {
        get => defaultValue;
        set
        {
            float prevValue = Value;
            defaultValue = Mathf.Clamp(value, MinValue, MaxValue);
            // value가 변했을 시 event로 알림
            TryInvokeValueChangedEvent(Value, prevValue);
        }
    }
    public float BonusValue { get; private set; }
    // Default + Bonus, 현재 총 수치
    public float Value => Mathf.Clamp(defaultValue + BonusValue, MinValue, MaxValue);
    public bool IsMax => Mathf.Approximately(Value, maxValue);
    public bool IsMin => Mathf.Approximately(Value, minValue);

    public event ValueChangedHandler onValueChanged;
    public event ValueChangedHandler onValueMax;
    public event ValueChangedHandler onValueMin;

    private void TryInvokeValueChangedEvent(float currentValue, float prevValue)
    {
        if (!Mathf.Approximately(currentValue, prevValue))
        {
            onValueChanged?.Invoke(this, currentValue, prevValue);
            if (Mathf.Approximately(currentValue, MaxValue))
                onValueMax?.Invoke(this, MaxValue, prevValue);
            else if (Mathf.Approximately(currentValue, MinValue))
                onValueMin?.Invoke(this, MinValue, prevValue);
        }
    }

    public void SetBonusValue(object key, object subkey, float value)
    {
        // 새로 들어온 BonusValue
        if (!bonusValuesByKey.ContainsKey(key))
            bonusValuesByKey[key] = new Dictionary<object, float>();
        // 중복 BonusValue
        else
            BonusValue -= bonusValuesByKey[key][subkey];

        float prevValue = Value;
        bonusValuesByKey[key][subkey] = value;
        BonusValue += value;

        TryInvokeValueChangedEvent(Value, prevValue);
    }

    public void SetBonusValue(object key, float value)
        => SetBonusValue(key, string.Empty, value);

    public float GetBonusValue(object key)
        => bonusValuesByKey.TryGetValue(key, out var bonusValuesBySubkey) ?
        bonusValuesBySubkey.Sum(x => x.Value) : 0f;

    public float GetBonusValue(object key, object subkey)
    {
        if (bonusValuesByKey.TryGetValue(key, out var bonusValuesBySubkey))
        {
            if (bonusValuesBySubkey.TryGetValue(subkey, out var value))
                return value;
        }
        return 0f;
    }

    public bool RemoveBonusValue(object key)
    {
        if (bonusValuesByKey.TryGetValue(key, out var bonusValuesBySubkey))
        {
            float prevValue = Value;
            BonusValue -= bonusValuesBySubkey.Values.Sum();
            bonusValuesByKey.Remove(key);

            TryInvokeValueChangedEvent(Value, prevValue);
            return true;
        }

        return false;
    }
    public bool RemoveBonusValue(object key, object subKey)
    {
        if (bonusValuesByKey.TryGetValue(key, out var bonusValuesBySubkey))
        {
            if (bonusValuesBySubkey.Remove(subKey, out var value))
            {
                var prevValue = Value;
                BonusValue -= value;
                TryInvokeValueChangedEvent(Value, prevValue);
                return true;
            }
        }
        return false;
    }

    public bool ContainsBonusValue(object key)
    => bonusValuesByKey.ContainsKey(key);

    public bool ContainsBonusValue(object key, object subKey)
        => bonusValuesByKey.TryGetValue(key, out var bonusValuesBySubKey) ? bonusValuesBySubKey.ContainsKey(subKey) : false;
}
