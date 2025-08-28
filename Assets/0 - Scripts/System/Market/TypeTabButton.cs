using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TypeTabButton : MonoBehaviour
{
    public Button button;
    public TMP_Text label;
    public Item.ItemType? itemType;
    private Action<Item.ItemType?> onClick;

    public void Init(string text, Action<Item.ItemType?> onClickAction, Item.ItemType? type)
    {
        label.text = text;
        itemType = type;
        onClick = onClickAction;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(itemType));
    }

    public void SetSelected(bool selected)
    {
        if (label != null)
            label.color = selected ? new Color(0.85f, 0.75f, 0.25f, 1f) : Color.black;
    }
}
