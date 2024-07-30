using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatOverride
{
    [SerializeField]
    private Stat stat;
    [SerializeField]
    private bool isUseOverride;
    [SerializeField]
    private float overrideDefaultValue;

    public StatOverride(Stat stat)
        => this.stat = stat;

    public Stat CreateStat()
    {
        var newStat = stat.Clone() as Stat;
        if (isUseOverride)
            newStat.DefaultValue = overrideDefaultValue;
        return newStat;
    }
}
