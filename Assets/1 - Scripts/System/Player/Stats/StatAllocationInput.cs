//using TMPro;
//using UnityEngine;

//public class StatAllocationInput : MonoBehaviour
//{
//    public StatAllocationButton.StatType statToAllocate;
//    public StatsUIBinder statsUIBinder;
//    public TMP_InputField inputField;

//    public void OnValidateInput()
//    {
//        if (statsUIBinder == null || inputField == null) return;

//        if (int.TryParse(inputField.text, out int requestedAmount))
//        {
//            statsUIBinder.TryAllocateMultiplePoints(statToAllocate, requestedAmount);
//        }

//        inputField.text = "";
//    }
//}
