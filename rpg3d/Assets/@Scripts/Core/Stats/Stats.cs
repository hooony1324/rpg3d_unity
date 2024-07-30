using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class Stats : MonoBehaviour
{
    [SerializeField]
    private Stat hpStat;
    [SerializeField]
    private Stat skillCostStat;

    [Space]
    [SerializeField]
    private StatOverride[] statOverrides;

    private Stat[] stats;

    public Entity Owner { get; private set; }
    public Stat HPStat { get; private set; }
    public Stat SkillCostStat { get; private set; }

    private void OnGUI()
    {
        if (!Owner.IsPlayer)
            return;

        GUI.Box(new Rect(2f, 2f, 250f, 250f), string.Empty);
        GUI.Label(new Rect(4f, 2f, 100f, 30f), "Player Stat");

        var textRect = new Rect(4f, 22f, 200f, 30f);
        var plusButtonRect = new Rect(textRect.x + textRect.width, textRect.y, 20f, 20f);
        var minusButtonRect = plusButtonRect;
        minusButtonRect.x += 22f;

        foreach (var stat in stats)
        {
            string defaultValueAsString = stat.IsPercentType ?
                $"{stat.DefaultValue * 100f:0.##;-0.##}%" :
                stat.DefaultValue.ToString("0.##;-0.##");

            string bonusValueAsString = stat.IsPercentType ?
                $"{stat.BonusValue * 100f:0.##;-0.##}%" :
                stat.BonusValue.ToString("0.##;-0.##");

            GUI.Label(textRect, $"{stat.DisplayName}: {defaultValueAsString} ({bonusValueAsString})");
            // + Button을 누르면 Stat 증가
            if (GUI.Button(plusButtonRect, "+"))
            {
                if (stat.IsPercentType)
                    stat.DefaultValue += 0.01f;
                else
                    stat.DefaultValue += 1f;
            }

            // - Button을 누르면 Stat 감소
            if (GUI.Button(minusButtonRect, "-"))
            {
                if (stat.IsPercentType)
                    stat.DefaultValue -= 0.01f;
                else
                    stat.DefaultValue -= 1f;
            }

            // 다음 Stat 정보 출력을 위해 y축으로 한칸 내림
            textRect.y += 22f;
            plusButtonRect.y = minusButtonRect.y = textRect.y;
        }
    }

    public void Setup(Entity entity)
    {
        Owner = entity;

        stats = statOverrides.Select(x => x.CreateStat()).ToArray();
        HPStat = hpStat ? GetStat(hpStat) : null;
        SkillCostStat = skillCostStat ? GetStat(skillCostStat) : null;
    }
    private void OnDestroy()
    {
        foreach (var stat in stats)
            Destroy(stat);
        stats = null;
    }
    public Stat GetStat(Stat stat)
    {
        Debug.Assert(stat != null, $"Stats::GetStat - stat은 null이 될 수 없습니다.");
        return stats.FirstOrDefault(x => x.ID == stat.ID);
    }
    public bool TryGetStat(Stat stat, out Stat outStat)
    {
        Debug.Assert(stat != null, $"Stats::TryGetStat - stat은 null이 될 수 없습니다.");

        outStat = stats.FirstOrDefault(x => x.ID == stat.ID);
        return outStat != null;
    }

    public float GetValue(Stat stat)
        => GetStat(stat).Value;

    public bool HasStat(Stat stat)
    {
        Debug.Assert(stat != null, $"Stats::HasStat - stat은 null이 될 수 없습니다.");
        return stats.Any(x => x.ID == stat.ID);
    }

    public void SetDefaultValue(Stat stat, float value)
        => GetStat(stat).DefaultValue = value;

    public float GetDefaultValue(Stat stat)
        => GetStat(stat).DefaultValue;

    public void IncreaseDefaultValue(Stat stat, float value)
        => GetStat(stat).DefaultValue += value;

    public void SetBonusValue(Stat stat, object key, float value)
        => GetStat(stat).SetBonusValue(key, value);
    public void SetBonusValue(Stat stat, object key, object subKey, float value)
        => GetStat(stat).SetBonusValue(key, subKey, value);

    public float GetBonusValue(Stat stat)
        => GetStat(stat).BonusValue;
    public float GetBonusValue(Stat stat, object key)
        => GetStat(stat).GetBonusValue(key);
    public float GetBonusValue(Stat stat, object key, object subKey)
        => GetStat(stat).GetBonusValue(key, subKey);

    public void RemoveBonusValue(Stat stat, object key)
        => GetStat(stat).RemoveBonusValue(key);
    public void RemoveBonusValue(Stat stat, object key, object subKey)
        => GetStat(stat).RemoveBonusValue(key, subKey);

    public bool ContainsBonusValue(Stat stat, object key)
        => GetStat(stat).ContainsBonusValue(key);
    public bool ContainsBonusValue(Stat stat, object key, object subKey)
        => GetStat(stat).ContainsBonusValue(key, subKey);

#if UNITY_EDITOR
    [ContextMenu("LoadStats")]
    private void LoadStats()
    {
        var stats = Resources.LoadAll<Stat>("Stat").OrderBy(x => x.ID);
        statOverrides = stats.Select(x => new StatOverride(x)).ToArray();
    }
#endif
}
