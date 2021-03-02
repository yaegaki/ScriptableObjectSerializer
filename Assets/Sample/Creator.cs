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

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var result = ScriptableObjectSerializer.Serializer.Serialize(this.hoge);
            var json = Encoding.UTF8.GetString(result);
            var h = ScriptableObjectSerializer.Serializer.Deserialize<Hoge>(result);
            Debug.Log(json);
            Debug.Log($"{h}:{h.fuu.aa}:{h.fuu.hoo.bb}:{h.fuu.hoo.yuu.zzz.h}:{h.fuu.hoo.yuu.yuu2.h}");
            this.dest = h;
        }
    }
}
