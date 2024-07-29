using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UI_HPOrb_Test : UI_Base
{
    enum Images
    {
        fill,
    }
    private void Start()
    {
        BindImage(typeof(Images));

        Stat_Test.Instance.Health
            .Subscribe(health => OnStatChanged(health, Stat_Test.Instance.MaxHealth))
            .AddTo(this);
    }

    public void OnStatChanged(int curHealth, int maxHealth)
    {
        GetImage((int)Images.fill).fillAmount = Mathf.Clamp01(curHealth / (float)maxHealth);
    }
}
