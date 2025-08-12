using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatToggleController : MonoBehaviour
{

    [Header("Toggles")]
    [SerializeField] private Toggle Toggle_Global;
    [SerializeField] private Toggle Toggle_Trade;
    [SerializeField] private Toggle Toggle_Private;

    [Tooltip("Transform qui contient tous les onglets (Tab_Global, Tab_Commerce, Tab_Pseudo...)")]
    [SerializeField] private Transform ongletContainer;

    private const string NameGlobalTab = "Tab_Global";
    private const string NameCommerceTab = "Tab_Commerce";

    private void Start()
    {
        Toggle_Global.onValueChanged.AddListener(on => SetTabActive(NameGlobalTab, on));
        SetTabActive(NameGlobalTab, Toggle_Global.isOn);

        Toggle_Trade.onValueChanged.AddListener(on => SetTabActive(NameCommerceTab, on));
        SetTabActive(NameCommerceTab, Toggle_Trade.isOn);

        Toggle_Private.onValueChanged.AddListener(on => SetPrivateTabs(on));
        SetPrivateTabs(Toggle_Private.isOn);
    }

    private void SetTabActive(string tabName, bool active)
    {
        var tab = ongletContainer.Find(tabName);
        if (tab != null)
            tab.gameObject.SetActive(active);
        else
            Debug.LogWarning($"[ChatToggleController] Impossible de trouver l'onglet « {tabName} »");
    }

    private void SetPrivateTabs(bool active)
    {
        foreach (Transform child in ongletContainer)
        {
            if (child.name == NameGlobalTab) continue;
            if (child.name == NameCommerceTab) continue;
            child.gameObject.SetActive(active);
        }
    }

}
