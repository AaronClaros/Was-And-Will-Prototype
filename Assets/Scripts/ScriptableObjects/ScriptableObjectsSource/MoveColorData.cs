using UnityEngine;

[CreateAssetMenu(fileName = "MoveColorData", menuName = "ScriptableObjects/MoveColorDataScriptable", order = 1)]
public class MoveColorData : ScriptableObject
{
    public MoveColor moveColor;
    public LayerMask blockMovementLayer;
    public LayerMask pushMovementLayer;
    public Sprite sprite;
}
