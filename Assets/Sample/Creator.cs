using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class Creator : MonoBehaviour
{
    [SerializeField]
    private Hoge hoge = default;

    [SerializeField]
    private Hoge dest = default;

    [SerializeField]
    private Piyo piyo = default;

    [SerializeField]
    private Piyo destPiyo = default;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Test(hoge, ref dest);
            Test(piyo, ref destPiyo);
        }
    }

    private void Test<T>(T t, ref T dest) where T : ScriptableObject
    {
        if (t == null) return;
        var result = ScriptableObjectSerializer.Serializer.Serialize(t);
        var json = Encoding.UTF8.GetString(result);
        var h = ScriptableObjectSerializer.Serializer.Deserialize<T>(result);
        Debug.Log(json);
        dest = h;
    }
}
