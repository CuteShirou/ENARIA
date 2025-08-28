using System;
using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-500)]
[AddComponentMenu("Combat/Global/Tile Click Input (Singleton)")]
public class TileClickInput : MonoBehaviour
{
    public static TileClickInput Instance { get; private set; }
    public static event Action<SetupTile> OnTileClicked;

    [Header("Raycast")]
    public LayerMask raycastLayers = Physics.DefaultRaycastLayers;
    public float maxDistance = 500f;
    public bool ignoreUI = true;
    public Camera targetCamera; // optionnel, sinon Camera.main

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (ignoreUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null) return;

        if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hit, maxDistance, raycastLayers))
        {
            var setup = hit.collider.GetComponentInParent<SetupTile>();
            if (setup != null)
            {
                Debug.Log($"Joueur a cliqué sur la case {{{setup.tileX},{setup.tileY}}}");
                OnTileClicked?.Invoke(setup);
            }
        }
    }

    /// <summary>Crée l'instance globale si absente.</summary>
    public static void EnsureExists()
    {
        if (Instance != null) return;
        var go = new GameObject("TileClickInput");
        go.AddComponent<TileClickInput>();
    }
}
