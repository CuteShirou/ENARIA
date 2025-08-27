using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(InventorySlotView))]
public class SlotRemoveButton : MonoBehaviour
{
    [SerializeField] private Button removeButton;
    [SerializeField] private InventorySlotView slotView; // si null, on le récupère en Awake
    
    private void Awake()
    {
        if (slotView == null) slotView = GetComponent<InventorySlotView>();
        if (removeButton != null)
            removeButton.onClick.AddListener(OnRemoveClicked);
    }
    
    private void OnDestroy()
    {
        if (removeButton != null)
            removeButton.onClick.RemoveListener(OnRemoveClicked);
    }
    
    private void OnRemoveClicked()
    {
        if (InventoryManager.Instance == null || slotView == null) return;
        InventoryManager.Instance.ClearItemAt(slotView.Index);
    }
}