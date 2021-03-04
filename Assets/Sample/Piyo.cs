using UnityEngine;

[CreateAssetMenu(fileName="Piyo", menuName="ScriptableObjects/Piyo")]
public class Piyo : ScriptableObject
{
    [SerializeField]
    private Hoge hoge = default;

    [SerializeField]
    private Piyo piyo = default;
    [SerializeField]
    private int index = default;
    [SerializeField]
    private float f = default;
    [SerializeField]
    private Vector2 vec2 = default;
    [SerializeField]
    private Vector2Int vec2i = default;
    [SerializeField]
    private Vector3 vec3 = default;
    [SerializeField]
    private Vector3Int vec3i = default;
    [SerializeField]
    private Vector4 vec4 = default;
    [SerializeField]
    private Quaternion quat = default;
}