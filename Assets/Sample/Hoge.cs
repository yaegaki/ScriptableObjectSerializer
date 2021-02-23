using System;
using UnityEngine;

[CreateAssetMenu(fileName="Data", menuName="ScriptableObjects/Hoge")]
public class Hoge : ScriptableObject
{
    [SerializeField]
    private int hogehoge;

    [SerializeField]
    private int fugafuga;

    public int piyopiyo;

    public Fuu fuu;
}

[Serializable]
public struct Fuu
{
    public int aa;
    public Hoo hoo;
}

[Serializable]
public struct Hoo
{
    public int bb;
}