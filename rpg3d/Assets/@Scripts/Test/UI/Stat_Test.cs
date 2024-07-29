using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Stat_Test : MonoSingleton<Stat_Test>
{
    public int MaxHealth = 100;

    public ReactiveProperty<int> Health => health;
    public ReactiveProperty<int> Mana => mana;
    public ReactiveProperty<int> AttackPower => attackPower;

    private ReactiveProperty<int> health = new ReactiveProperty<int>(100);
    private ReactiveProperty<int> mana = new ReactiveProperty<int>(50);
    private ReactiveProperty<int> attackPower = new ReactiveProperty<int>(10);

    public void TakeDamage(int damage)
    {
        health.Value -= damage;
        if (health.Value < 0) health.Value = 0;
    }

    public void UseMana(int amount)
    {
        mana.Value -= amount;
        if (mana.Value < 0) mana.Value = 0;
    }

    public void IncreaseAttackPower(int amount)
    {
        attackPower.Value += amount;
    }
}
