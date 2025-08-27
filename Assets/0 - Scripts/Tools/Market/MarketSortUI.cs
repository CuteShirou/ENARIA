using UnityEngine;
using UnityEngine.UI;
using System;

public class MarketSortUI : MonoBehaviour
{
    public Button buttonName;
    public Button buttonLevel;
    public Button buttonQuantity;
    public Button buttonPrice;

    private bool ascending = true;

    public Action<string, bool> OnSortRequested;

    private void Start()
    {
        buttonName.onClick.AddListener(() => RequestSort("name"));
        buttonLevel.onClick.AddListener(() => RequestSort("level"));
        buttonQuantity.onClick.AddListener(() => RequestSort("quantity"));
        buttonPrice.onClick.AddListener(() => RequestSort("price"));
    }

    private void RequestSort(string criteria)
    {
        OnSortRequested?.Invoke(criteria, ascending);
        ascending = !ascending;
    }
}
