using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="Data", menuName="ScriptableObjects/Hoge")]
public class Hoge : ScriptableObject
{
    [SerializeField]
    private int hogehoge;

    [SerializeField]
    private int fugafuga;

    [SerializeField]
    private string str;

    [SerializeField]
    private string[] strArray;

    public int piyopiyo;

    public Fuu fuu;


    public int[] ValueArray;
    public List<int> ValueList;
    public List<int[]> ValueArrayList;
    public Fuu[] fuuArray;
    public int[][] ValueValue;
    public int[][][] ValueValueValue;
}

[Serializable]
public class Fuu
{
    public int aa;
    public Hoo hoo;
}

[Serializable]
public struct Hoo
{
    public int bb;
    public Yuu yuu;
}

[Serializable]
public struct Yuu
{
    public int aaa;
    public Zz zzz;
    public Yuu2 yuu2;
}

[Serializable]
public struct Yuu2
{
    public int h;
}

[Serializable]
public struct Zz
{
    public int h;
}