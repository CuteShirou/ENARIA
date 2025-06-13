using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CoinSystem : MonoBehaviour
{
    [Tooltip("Multiplicateur de taille à appliquer au sac")]
    public float sizeMultiplier = 1.2f;

    [Tooltip("Référence vers le sac à agrandir")]
    public Transform sacToScale;

    [Tooltip("Tag du joueur")]
    public string playerTag = "Player";

    [Tooltip("UI Text ou TMP_Text qui affiche le compteur")]
    public Text coinCounterText;

    [Tooltip("Nombre maximum de pièces")]
    public int maxCoins = 5;

    private static int currentCoins = 0;

    private void Reset()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (sacToScale != null)
            sacToScale.localScale *= sizeMultiplier;

        currentCoins++;
        currentCoins = Mathf.Clamp(currentCoins, 0, maxCoins);

        if (coinCounterText != null)
            coinCounterText.text = currentCoins + " / " + maxCoins;

        Destroy(gameObject);
    }
}