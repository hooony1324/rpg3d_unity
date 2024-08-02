using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Base
{
    public string name;
}

[System.Serializable]
public class Child1 : Base
{
    public int one;
}

[System.Serializable]
public class Child2 : Base
{
    public float two;
}

public class TestGameObject : MonoBehaviour
{
    [SerializeReference, SubclassSelector]
    private Base test;
}
