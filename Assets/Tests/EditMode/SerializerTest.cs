using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityObjectSerializer;

namespace Tests
{
    public class SerializerTest
    {
        [Test]
        public void SerializableTest()
        {
            var s = ScriptableObject.CreateInstance<SampleScriptableObject>();
            s.PrivateFieldGetterSetter = 1;
            s.publicField = 2;
            s.array = new int[]{3,4,5,6,};
            s.list = new List<int>(new[]{3,4,5,6,});
            s.serializableStruct = new SampleSerializableStruct { value = 7 };
            s.serializableClass = new SampleSerializableClass { value = 8 };
            var bin = Serializer.Serialize(s);
            var result = Serializer.Deserialize<SampleScriptableObject>(bin);
            Assert.AreEqual(s.PrivateFieldGetterSetter, result.PrivateFieldGetterSetter, "PrivateField");
            Assert.AreEqual(s.publicField, result.publicField, "PublicField");
            Assert.AreEqual(s.serializableStruct.value, result.serializableStruct.value, "SerializableStruct");
            Assert.AreEqual(s.serializableClass.value, result.serializableClass.value, "SerializableClass");

            CollectionAssert.AreEqual(s.array, result.array, "Array");
            CollectionAssert.AreEqual(s.arrayNull, result.arrayNull, "Array(null)");
            CollectionAssert.AreEqual(s.list, result.list, "List");
            CollectionAssert.AreEqual(s.listNull, result.listNull, "List(null)");
            Assert.AreEqual(s.serializableClassNull, result.serializableClassNull, "SerializableClass(null)");
        }

        [Test]
        public void SerializeSimpleTypeTest()
        {
            var s = ScriptableObject.CreateInstance<SampleScriptableObject>();
            s.intField = 1;
            s.intArrayField = new[] { 2, 3, 4, 5, };
            s.floatField = 6f;
            s.floatArrayField = new[] {7f, 8f, 9f, 10f, };
            s.stringField = "test";
            s.stringArrayField = new[] { "test2", "test3", "test4", "test5", };
            s.sampleSerializableStructField = new SampleSerializableStruct { value = 1 };
            s.sampleSerializableStructArrayField = new []
            {
                new SampleSerializableStruct { value = 2 },
                new SampleSerializableStruct { value = 3 },
                new SampleSerializableStruct { value = 4 },
            };
            s.sampleSerializableClassField = new SampleSerializableClass { value = 5 };
            s.sampleSerializableClassArrayField = new []
            {
                new SampleSerializableClass { value = 6 },
                new SampleSerializableClass { value = 7 },
                new SampleSerializableClass { value = 8 },
            };
            s.vector2Field = new Vector2(1f, 2f);
            s.vector2ArrayField = new[]{
                new Vector2(3f, 4f),
                new Vector2(4f, 5f),
                new Vector2(6f, 7f),
            };
            s.vector3Field = new Vector3(1f, 2f, 3f);
            s.vector3ArrayField = new[]{
                new Vector3(3f, 4f, 5f),
                new Vector3(4f, 5f, 6f),
                new Vector3(5f, 6f, 7f),
            };
            s.vector4Field = new Vector4(1f, 2f, 3f, 4f);
            s.vector4ArrayField = new[]{
                new Vector4(3f, 4f, 5f, 6f),
                new Vector4(4f, 5f, 6f, 7f),
                new Vector4(5f, 6f, 7f, 8f),
            };
            s.quaternionField = new Quaternion(1f, 2f, 3f, 4f);
            s.quaternionArrayField = new[]{
                new Quaternion(3f, 4f, 5f, 6f),
                new Quaternion(4f, 5f, 6f, 7f),
                new Quaternion(5f, 6f, 7f, 8f),
            };
            s.vector2IntField = new Vector2Int(1, 2);
            s.vector2IntArrayField = new[]{
                new Vector2Int(3, 4),
                new Vector2Int(4, 5),
                new Vector2Int(6, 7),
            };
            s.vector3IntField = new Vector3Int(1, 2, 3);
            s.vector3IntArrayField = new[]{
                new Vector3Int(3, 4, 5),
                new Vector3Int(4, 5, 6),
                new Vector3Int(5, 6, 7),
            };
            var bin = Serializer.Serialize(s);
            var result = Serializer.Deserialize<SampleScriptableObject>(bin);

            Assert.AreEqual(s.intField, result.intField, "int");
            Assert.AreEqual(s.floatField, result.floatField, "float");
            Assert.AreEqual(s.stringField, result.stringField, "string");
            Assert.AreEqual(s.sampleSerializableStructField.value, result.sampleSerializableStructField.value, "SampleSerializableStruct");
            Assert.AreEqual(s.sampleSerializableClassField.value, result.sampleSerializableClassField.value, "SampleSerializableArray");
            Assert.AreEqual(s.vector2Field, result.vector2Field, "Vector2");
            Assert.AreEqual(s.vector3Field, result.vector3Field, "Vector3");
            Assert.AreEqual(s.vector4Field, result.vector4Field, "Vector4");
            Assert.AreEqual(s.quaternionField, result.quaternionField, "Quaternion");
            Assert.AreEqual(s.vector2IntField, result.vector2IntField, "Vector2Int");
            Assert.AreEqual(s.vector3IntField, result.vector3IntField, "Vector3Int");
            CollectionAssert.AreEqual(s.intArrayField, result.intArrayField, "int[]");
            CollectionAssert.AreEqual(s.floatArrayField, result.floatArrayField, "float[]");
            CollectionAssert.AreEqual(s.stringArrayField, result.stringArrayField, "string[]");
            CollectionAssert.AreEqual(
                s.sampleSerializableStructArrayField.Select(o => o.value),
                result.sampleSerializableStructArrayField.Select(o => o.value),
                "SampleSerialzableStruct[]");
            CollectionAssert.AreEqual(
                s.sampleSerializableClassArrayField.Select(o => o.value),
                result.sampleSerializableClassArrayField.Select(o => o.value),
                "SampleSerialzableClass[]");
            CollectionAssert.AreEqual(s.vector2ArrayField, result.vector2ArrayField, "Vector2[]");
            CollectionAssert.AreEqual(s.vector3ArrayField, result.vector3ArrayField, "Vector3[]");
            CollectionAssert.AreEqual(s.vector4ArrayField, result.vector4ArrayField, "Vector4[]");
            CollectionAssert.AreEqual(s.quaternionArrayField, result.quaternionArrayField, "Quaternion[]");
            CollectionAssert.AreEqual(s.vector2IntArrayField, result.vector2IntArrayField, "Vector2Int[]");
            CollectionAssert.AreEqual(s.vector3IntArrayField, result.vector3IntArrayField, "Vector3Int[]");
        }

        [Test]
        public void SerializeScriptableObjectReferenceTest()
        {
            var s = ScriptableObject.CreateInstance<SampleScriptableObject>();
            SampleScriptableObject Create(int v)
            {
                var o = ScriptableObject.CreateInstance<SampleScriptableObject>();
                o.intField = v;
                return o;
            }
            s.sampleScriptableObject = Create(42);
            s.sampleScriptableObjectArray = new[]
            {
                Create(1),
                Create(2),
                Create(3),
            };

            var bin = Serializer.Serialize(s);
            var result = Serializer.Deserialize<SampleScriptableObject>(bin);
            Assert.AreEqual(s.sampleScriptableObject.intField, result.sampleScriptableObject.intField, "SmapleScriptableObject");
            CollectionAssert.AreEqual(
                s.sampleScriptableObjectArray.Select(o => o.intField),
                result.sampleScriptableObjectArray.Select(o => o.intField),
                "SampleScriptalbeObject[]");
        }

        [Test]
        public void SerializeReferenceTest()
        {
            var s = ScriptableObject.CreateInstance<SampleScriptableObject>();
            s.sampleScriptableObject = s;

            var bin = Serializer.Serialize(s);
            var result = Serializer.Deserialize<SampleScriptableObject>(bin);
            Assert.AreEqual(result, result.sampleScriptableObject, "CircularReference");
        }
    }
}
