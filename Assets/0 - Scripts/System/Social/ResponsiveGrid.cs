using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveRowGrid : MonoBehaviour
{
    public int minRowHeight = 100;

    private GridLayoutGroup grid;
    private RectTransform rectTransform;

    void Start()
    {
        grid = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        UpdateGrid();
    }

    void Update()
    {
        UpdateGrid();
    }

    void UpdateGrid()
    {
        float height = rectTransform.rect.height;
        //int rowCount = Mathf.Max(1, Mathf.FloorToInt(height / (minRowHeight + grid.spacing.y)));
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
    }
}
