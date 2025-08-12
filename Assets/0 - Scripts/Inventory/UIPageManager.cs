using UnityEngine;

public class UIPageManager : MonoBehaviour
{
    public GameObject PageInventory;
    public GameObject PageCrafting;
    public GameObject PageStats;
    public GameObject PageSkillTree;
    
    
    
    public void ShowInventory()
    {
        HideAllPages();
        SetActivePage(PageInventory);
    }
    
    public void ShowCrafting()
    {
        HideAllPages();
        SetActivePage(PageCrafting);
    }
    
    public void ShowStats()
    {
        HideAllPages();
        SetActivePage(PageStats);
    }
    
    public void ShowSkillTree()
    {
        HideAllPages();
        SetActivePage(PageSkillTree);
    }
    
    private void HideAllPages()
    {
        PageInventory.SetActive(false);
        PageCrafting.SetActive(false);
        PageStats.SetActive(false);
        PageSkillTree.SetActive(false);
    }
    
    private void SetActivePage(GameObject ActivePage)
    {
        PageInventory.SetActive(ActivePage == PageInventory);
        PageCrafting.SetActive(ActivePage == PageCrafting);
        PageStats.SetActive(ActivePage == PageStats);
        PageSkillTree.SetActive(ActivePage == PageSkillTree);
    }
}