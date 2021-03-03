using UnityEngine;

[CreateAssetMenu(fileName="Piyo", menuName="ScriptableObjects/Piyo")]
public class Piyo : ScriptableObject
{
    [SerializeField]
    private Hoge hoge = default;
}