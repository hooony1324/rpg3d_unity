using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class UI_InputField_Test : UI_Base
{
    enum GameObjects
    {
        InputField,
    }

    private void Awake()
    {
        BindObject(typeof(GameObjects));

        GetObject((int)GameObjects.InputField).GetComponent<TMP_InputField>().onValueChanged.AddListener(ChangePlayerStat);
    }

    void ChangePlayerStat(string value)
    {
        if (int.TryParse(value, out int result))
            Stat_Test.Instance.TakeDamage(result);
    }


}


