using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class SampleScriptableObject : ScriptableObject
    {
        [SerializeField]
        private int privateField;
        public int PrivateFieldGetterSetter
        {
            get => privateField;
            set => privateField = value;
        }

        public int publicField;

        public SampleSerializableStruct serializableStruct;
        public SampleSerializableClass serializableClass;
        public SampleSerializableClass serializableClassNull;

        public int[] array;
        public int[] arrayNull;
        public List<int> list;
        public List<int> listNull;

        public int intField;
        public int[] intArrayField;
        public float floatField;
        public float[] floatArrayField;
        public string stringField;
        public string[] stringArrayField;
        public SampleSerializableStruct sampleSerializableStructField;
        public SampleSerializableStruct[] sampleSerializableStructArrayField;
        public SampleSerializableClass sampleSerializableClassField;
        public SampleSerializableClass[] sampleSerializableClassArrayField;
        public SampleScriptableObject sampleScriptableObject;
        public SampleScriptableObject[] sampleScriptableObjectArray;
        public Vector2 vector2Field;
        public Vector2[] vector2ArrayField;
        public Vector3 vector3Field;
        public Vector3[] vector3ArrayField;
        public Vector4 vector4Field;
        public Vector4[] vector4ArrayField;
        public Quaternion quaternionField;
        public Quaternion[] quaternionArrayField;
        public Vector2Int vector2IntField;
        public Vector2Int[] vector2IntArrayField;
        public Vector3Int vector3IntField;
        public Vector3Int[] vector3IntArrayField;
    }


    [Serializable]
    public struct SampleSerializableStruct
    {
        public int value;
    }

    [Serializable]
    public class SampleSerializableClass
    {
        public int value;
    }
}
