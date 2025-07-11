using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatPreparationManager : MonoBehaviour
{
    [Header("Références")]
    public Grid grid;
    public CombatMapData mapData;
    private GameObject player;
    private List<GameObject> enemies;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public Button readyButton;

    [Header("Temps")]
    public float preparationTime = 120f;
    private float countdown;
    private bool isReady = false;

    private TileCoord currentPlayerTile;

    void Start()
    {
        countdown = preparationTime;
    }

    public void ForceSetup()
    {
        countdown = preparationTime;

        player = GameObject.FindGameObjectWithTag("Player");

        player.GetComponent<CombatController>().gridManager = grid;

        GameObject[] foundEnemies = GameObject.FindGameObjectsWithTag("Monster");
        enemies = new List<GameObject>(foundEnemies);

        foreach (var enemy in enemies)
        {
            CombatController cc = enemy.GetComponent<CombatController>();
            if (cc != null)
            {
                cc.gridManager = grid;
                cc.enabled = false;
            }
        }

        PlacePlayerRandomly();
        PlaceEnemiesRandomly();

        readyButton.onClick.AddListener(OnPlayerReady);
    }

    void Update()
    {
        if (isReady) return;

        countdown -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(countdown).ToString();

        if (countdown <= 0)
        {
            StartCombat();
        }
    }

    void PlacePlayerRandomly()
    {
        List<Vector2Int> greens = new List<Vector2Int>(mapData.greenTeamPositions);
        Vector2Int coord = greens[Random.Range(0, greens.Count)];
        GameObject tileGO = grid.TileMap[coord];
        player.transform.position = tileGO.transform.position + new Vector3(0, 0.1f, 0);

        currentPlayerTile = tileGO.GetComponent<TileCoord>();
        player.GetComponent<CombatController>().AssignToTile(currentPlayerTile);
    }

    void PlaceEnemiesRandomly()
    {
        List<Vector2Int> reds = new List<Vector2Int>(mapData.redTeamPositions);
        for (int i = 0; i < enemies.Count && reds.Count > 0; i++)
        {
            Vector2Int coord = reds[Random.Range(0, reds.Count)];
            reds.Remove(coord);

            GameObject tile = grid.TileMap[coord];
            enemies[i].transform.position = tile.transform.position + new Vector3(0, 0.1f, 0);

            TileCoord tileCoord = tile.GetComponent<TileCoord>();
            tileCoord.SetOccupant(enemies[i]);

            CombatController cc = enemies[i].GetComponent<CombatController>();
            if (cc != null)
                cc.AssignToTile(tileCoord);
        }
    }

    void OnPlayerReady()
    {
        isReady = true;
        StartCombat();
    }

    void StartCombat()
    {
        readyButton.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);

        // Réactiver les CombatController
        foreach (var enemy in enemies)
        {
            CombatController cc = enemy.GetComponent<CombatController>();
            if (cc != null)
                cc.enabled = true;
        }

        FindAnyObjectByType<CombatManager>().InitCombat();
        enabled = false;
    }


    public void TryMovePlayerTo(TileCoord newTile)
    {
        if (isReady) return;
        if (newTile == currentPlayerTile) return;
        if (!mapData.greenTeamPositions.Contains(newTile.Coord)) return;

        currentPlayerTile.ClearOccupant();
        currentPlayerTile = newTile;
        player.transform.position = newTile.transform.position + new Vector3(0, 0.1f, 0);
        newTile.SetOccupant(player);
        player.GetComponent<CombatController>().AssignToTile(newTile);
    }
}
