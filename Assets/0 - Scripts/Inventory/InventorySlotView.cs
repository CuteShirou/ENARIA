using Newtonsoft.Json.Converters;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour
{
    [Header("Refs UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    
    [Header("Data")]
    [SerializeField] private int index;
    public int Index => index;

    private Item currentItem;
    
    public void BindIndex(int i)
    {
        index = i;
    }

    public void Set(Item item, Sprite emptySprite)
    {
        currentItem = item;
        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.preserveAspect = true;
            iconImage.color = Color.white;
            iconImage.sprite = (item != null && item.icon != null) ? item.icon : emptySprite;
        }

        if (nameText != null)
        {
            bool show = (item != null && !string.IsNullOrEmpty(item.itemName));
            nameText.gameObject.SetActive(show);
            if (show) nameText.text = item.itemName;
        }
    }

    private void Start()
    {
        if (iconImage != null && InventoryManager.EmptySlotSprite != null && currentItem == null)
        {
            iconImage.enabled = true;
            iconImage.preserveAspect = true;
            iconImage.sprite = InventoryManager.EmptySlotSprite;
        }
    }
    
    public Item Get() => currentItem;
}