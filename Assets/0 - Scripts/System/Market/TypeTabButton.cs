using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TypeTabButton : MonoBehaviour
{
    public Button button;
    public TMP_Text label;
    public EquipmentType? equipmentType;
    private Action<EquipmentType?> onClick;

    public void Init(string text, Action<EquipmentType?> onClickAction, EquipmentType? type)
    {
        label.text = text;
        equipmentType = type;
        onClick = onClickAction;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(equipmentType));
    }

    public void SetSelected(bool selected)
    {
        if (label != null)
            label.color = selected ? new Color(0.85f, 0.75f, 0.25f, 1f) : Color.black;
    }
}
