using UnityEngine;

public class MaterialSwapper : MonoBehaviour
{
    public Material hoverMaterial;
    private Material originalMaterial;
    private Renderer rend;
    private bool isHovering = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalMaterial = rend.material;
        }
    }

    void Update()
    {
        // Si aucune caméra n’est encore active, on ne fait rien
        if (Camera.main == null || rend == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool hitThisObject = Physics.Raycast(ray, out hit) && hit.transform == transform;

        if (hitThisObject && !isHovering)
        {
            rend.material = hoverMaterial;
            isHovering = true;
        }
        else if (!hitThisObject && isHovering)
        {
            rend.material = originalMaterial;
            isHovering = false;
        }
    }
}