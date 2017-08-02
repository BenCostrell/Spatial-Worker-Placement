using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Item Stat Config")]
public class ItemStatConfig : ScriptableObject
{
    [SerializeField]
    private Sprite sprite;
    public Sprite Sprite { get { return sprite; } }

    [SerializeField]
    private int cost;
    public int Cost { get { return cost; } }

    [SerializeField]
    private Item.StatType statType;
    public Item.StatType StatType { get { return statType; } }

    [SerializeField]
    private string label;
    public string Label { get { return label; } }
}
