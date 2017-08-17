using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Zone Type Info")]
public class ZoneTypeInfo : ScriptableObject
{
    [SerializeField]
    private Sprite sprite;
    public Sprite Sprite { get { return sprite; } }

    [SerializeField]
    private string label;
    public string Label { get { return label; } }

    [SerializeField]
    private Zone.ZoneType type;
    public Zone.ZoneType Type { get { return type; } }

    [SerializeField]
    private int effectMagnitude;
    public int EffectMagnitude { get { return effectMagnitude; } }
}
