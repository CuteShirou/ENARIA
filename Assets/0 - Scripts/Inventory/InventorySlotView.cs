
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour
{
    [Header("Refs UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text countText;

    [Header("Data")]
    [SerializeField] private int index;
    public int Index => index;

    private Item currentItem;
    private int currentCount;

    public void BindIndex(int idx) => index = idx;

    private void Awake()
    {
        if (iconImage == null)
        {
            var t = transform.Find("ItemIcon");
            if (t != null) iconImage = t.GetComponent<Image>();
        }

        if (nameText == null)
        {
            var t = transform.Find("ItemName");
            if (t != null) nameText = t.GetComponent<Text>();
        }

        if (countText == null)
        {
            var t = transform.Find("ItemCount");
            if (t != null) countText = t.GetComponent<Text>();
        }
    }

    public void Set(Item item, int count, Sprite emptySprite)
    {
        currentItem = item;
        currentCount = count;

        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.preserveAspect = true;
            iconImage.color = Color.white;
            iconImage.sprite = (item != null && item.icon != null) ? item.icon : emptySprite;
        }

        ApplyName();
        ApplyCount();
    }

    public void Set(Item item, Sprite emptySprite)
    {
        int count = (item != null) ? 1 : 0;
        Set(item, count, emptySprite);
    }

    private void Start()
    {
        if (iconImage != null && InventoryManager.EmptySlotSprite != null && currentItem == null)
        {
            iconImage.enabled = true;
            iconImage.preserveAspect = true;
            iconImage.sprite = InventoryManager.EmptySlotSprite;
        }
        ApplyName();
        ApplyCount();
    }

    private void OnEnable()
    {
        ApplyName();
        ApplyCount();
    }

    private void ApplyName()
    {
        if (nameText == null) return;
        if (currentItem != null) nameText.text = currentItem.itemName;
        else nameText.text = string.Empty;
    }

    private void ApplyCount()
    {
        if (countText == null) return;

        bool stackable = (currentItem != null) && (currentItem.itemType == Item.ItemType.Consumable);
        bool show = stackable && currentCount > 1;
        countText.gameObject.SetActive(show);
        countText.text = show ? currentCount.ToString() : string.Empty;
    }

    public Item Get() => currentItem;
}
