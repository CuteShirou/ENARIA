using System.Collections.Generic;
using UnityEngine;

public class CraftManager : MonoBehaviour
{
    public static CraftManager Instance;

    public int playerProfessionLevel = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool CanCraft(CraftRecipeData recipe)
    {
        if (playerProfessionLevel < recipe.requiredProfessionLevel)
            return false;

        // Vérifier l'inventaire réel (à implémenter)
        return true; // provisoire
    }

    public bool Craft(CraftRecipeData recipe)
    {
        if (!CanCraft(recipe))
            return false;

        // Consommer les ingrédients dans l'inventaire réel

        // Ajouter le résultat dans l'inventaire réel

        Debug.Log($"Craft réussi ! Résultat : {recipe.result}");

        return true;
    }

    public int GetItemQuantity(Object item)
    {

        return 0;
    }

}














//// CraftManager.cs (MAJ : utilise FindAnyObjectByType / FindFirstObjectByType avec fallback)
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
//using UnityEngine.Networking;

//public class CraftManager : MonoBehaviour
//{
//    public static CraftManager Instance;

//    [Header("API")]
//    public string craftUrl = "https://tondomaine.com/api/craft.php";
//    public string httpUsername = ""; // optionnel Basic Auth
//    public string httpPassword = "";

//    [Header("Player (fallback si PlayerStats n'a pas d'ID)")]
//    public int inspectorPlayerId = 1;

//    [Header("Mapping DB ID -> InventoryItem.itemID (string)")]
//    public List<ItemIdMapping> idMappings = new List<ItemIdMapping>();

//    private void Awake()
//    {
//        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
//        Instance = this;
//    }

//    [Serializable]
//    public class ItemIdMapping { public int dbId; public string clientItemID; }
//    private string GetClientItemIDFromDbId(int dbId)
//    {
//        var m = idMappings.FirstOrDefault(x => x.dbId == dbId);
//        return m != null ? m.clientItemID : null;
//    }

//    #region Helpers find with compatibility
//    private T FindInventoryManager<T>() where T : UnityEngine.Object

//    {
//#if UNITY_2023_2_OR_NEWER
//        return UnityEngine.Object.FindAnyObjectByType<T>();
//#else
//        return UnityEngine.Object.FindObjectOfType<T>();
//#endif
//    }

//    private PlayerStats FindPlayerStats()
//    {
//#if UNITY_2023_2_OR_NEWER
//        return UnityEngine.Object.FindAnyObjectByType<PlayerStats>();
//#else
//        return UnityEngine.Object.FindObjectOfType<PlayerStats>();
//#endif
//    }

//    private InventoryManager FindInventoryManager()
//    {
//#if UNITY_2023_2_OR_NEWER
//        return UnityEngine.Object.FindAnyObjectByType<InventoryManager>();
//#else
//        return UnityEngine.Object.FindObjectOfType<InventoryManager>();
//#endif
//    }
//    #endregion

//    #region CanCraft / Local Inventory helpers
//    public bool CanCraft(CraftRecipeData recipe)
//    {
//        var player = FindPlayerStats();
//        if (player != null && player.level < recipe.requiredProfessionLevel) return false;

//        foreach (var ing in recipe.ingredients)
//        {
//            string clientId = GetClientItemIDFromDbId(ing.dbId);
//            if (string.IsNullOrEmpty(clientId))
//            {
//                Debug.LogWarning($"Mapping manquant pour dbId {ing.dbId}");
//                return false;
//            }
//            int have = GetItemQuantity(clientId);
//            if (have < ing.quantity) return false;
//        }
//        return true;
//    }

//    public int GetItemQuantity(string clientItemID)
//    {
//        var inv = FindInventoryManager();
//        if (inv == null) return 0;
//        int total = 0;
//        foreach (var slot in inv.inventorySlots)
//        {
//            if (slot.currentItem != null && slot.currentItem.itemID == clientItemID)
//                total += slot.currentItem.quantity;
//        }
//        return total;
//    }

//    private bool RemoveLocalItems(string clientItemID, int amount)
//    {
//        var inv = FindInventoryManager();
//        if (inv == null) return false;

//        int remaining = amount;
//        foreach (var slot in inv.inventorySlots)
//        {
//            if (slot.currentItem == null) continue;
//            if (slot.currentItem.itemID != clientItemID) continue;
//            if (remaining <= 0) break;

//            if (slot.currentItem.quantity > remaining)
//            {
//                slot.currentItem.quantity -= remaining;
//                slot.SetItem(slot.currentItem);
//                remaining = 0;
//            }
//            else
//            {
//                remaining -= slot.currentItem.quantity;
//                slot.ClearSlot();
//            }
//        }
//        return remaining == 0;
//    }

//    private bool AddLocalItem(string clientItemID, int amount)
//    {
//        var inv = FindInventoryManager();
//        if (inv == null) return false;

//        // 1) ajouter aux stacks existantes
//        foreach (var slot in inv.inventorySlots)
//        {
//            if (slot.currentItem == null) continue;
//            if (slot.currentItem.itemID != clientItemID) continue;
//            int space = slot.currentItem.maxStack - slot.currentItem.quantity;
//            if (space <= 0) continue;
//            int toAdd = Mathf.Min(space, amount);
//            slot.currentItem.quantity += toAdd;
//            slot.SetItem(slot.currentItem);
//            amount -= toAdd;
//            if (amount <= 0) return true;
//        }

//        // 2) créer dans des slots vides à partir du ItemDatabase
//        for (int i = 0; i < inv.inventorySlots.Length; i++)
//        {
//            var slot = inv.inventorySlots[i];
//            if (slot.currentItem != null) continue;
//            var template = inv.itemDatabase.items.FirstOrDefault(it => it.itemID == clientItemID);
//            if (template == null)
//            {
//                Debug.LogWarning($"Template item non trouvé dans ItemDatabase pour clientItemID={clientItemID}");
//                return false;
//            }
//            InventoryItem copy = new InventoryItem
//            {
//                itemID = template.itemID,
//                icon = template.icon,
//                maxStack = template.maxStack,
//                type = template.type,
//                quantity = 0
//            };
//            int toPlace = Mathf.Min(copy.maxStack, amount);
//            copy.quantity = toPlace;
//            slot.SetItem(copy);
//            amount -= toPlace;
//            if (amount <= 0) return true;
//        }

//        if (amount > 0)
//        {
//            Debug.LogWarning($"Inventaire plein — {amount} items non placés (item {clientItemID})");
//            return false;
//        }
//        return true;
//    }
//    #endregion

//    #region Network / Craft
//    public void StartCraft(CraftRecipeData recipe, int quantity = 1)
//    {
//        StartCoroutine(CraftCoroutine(recipe, quantity));
//    }

//    private IEnumerator CraftCoroutine(CraftRecipeData recipe, int quantity)
//    {
//        CraftRequestPayload payload = new CraftRequestPayload
//        {
//            recipeId = recipe.recipeId,
//            ingredients = recipe.ingredients.Select(i => new IngredientDTO
//            {
//                item_type = i.ingredientType == CraftRecipeData.IngredientType.Resource ? "resource" : "equipment",
//                item_id = i.dbId,
//                quantity = i.quantity * quantity
//            }).ToArray(),
//            result = new ResultDTO
//            {
//                item_type = recipe.resultType == CraftRecipeData.ResultType.Resource ? "resource" : "equipment",
//                item_id = recipe.resultDbId,
//                quantity = recipe.resultQuantity * quantity
//            }
//        };

//        string json = JsonUtility.ToJson(payload);
//        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

//        using (UnityWebRequest www = new UnityWebRequest(craftUrl, "POST"))
//        {
//            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
//            www.downloadHandler = new DownloadHandlerBuffer();
//            www.SetRequestHeader("Content-Type", "application/json");

//            int playerIdHeader = ResolvePlayerId();
//            www.SetRequestHeader("X-Player-Id", playerIdHeader.ToString());

//            if (!string.IsNullOrEmpty(httpUsername))
//            {
//                string auth = httpUsername + ":" + httpPassword;
//                string encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
//                www.SetRequestHeader("Authorization", "Basic " + encoded);
//            }

//            yield return www.SendWebRequest();

//#if UNITY_2020_1_OR_NEWER
//            bool error = www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError;
//#else
//            bool error = www.isNetworkError || www.isHttpError;
//#endif
//            if (error)
//            {
//                Debug.LogError($"Craft HTTP error: {www.error} Code: {www.responseCode} Body: {www.downloadHandler.text}");
//                yield break;
//            }

//            string resp = www.downloadHandler.text;
//            ServerCraftResponse serverResp = null;
//            try { serverResp = JsonUtility.FromJson<ServerCraftResponse>(resp); }
//            catch (Exception ex) { Debug.LogError("Impossible de parser la réponse craft: " + ex.Message + " raw: " + resp); }

//            if (serverResp == null)
//            {
//                Debug.LogError("Réponse craft invalide: " + resp);
//                yield break;
//            }
//            if (!serverResp.success)
//            {
//                Debug.LogWarning("Craft échoué: " + serverResp.message);
//                yield break;
//            }

//            ApplyLocalChangesFromRecipe(recipe, quantity);
//            Debug.Log("Craft réussi: " + serverResp.message);
//        }
//    }

//    private int ResolvePlayerId()
//    {
//        var ps = FindPlayerStats();
//        if (ps != null)
//        {
//            var t = ps.GetType();
//            var f = t.GetField("playerId");
//            if (f != null)
//            {
//                object val = f.GetValue(ps);
//                if (val is int) return (int)val;
//            }
//        }
//        return inspectorPlayerId;
//    }

//    private void ApplyLocalChangesFromRecipe(CraftRecipeData recipe, int quantity)
//    {
//        foreach (var ing in recipe.ingredients)
//        {
//            string clientId = GetClientItemIDFromDbId(ing.dbId);
//            if (string.IsNullOrEmpty(clientId)) continue;
//            bool ok = RemoveLocalItems(clientId, ing.quantity * quantity);
//            if (!ok) Debug.LogWarning($"Échec suppression locale {clientId} pour qty {ing.quantity * quantity}");
//        }
//        string resClientId = GetClientItemIDFromDbId(recipe.resultDbId);
//        if (!string.IsNullOrEmpty(resClientId))
//        {
//            bool added = AddLocalItem(resClientId, recipe.resultQuantity * quantity);
//            if (!added) Debug.LogWarning($"Impossible d'ajouter localement item {resClientId}");
//        }
//    }
//    #endregion

//    #region DTOs
//    [Serializable]
//    private class CraftRequestPayload
//    {
//        public int recipeId;
//        public IngredientDTO[] ingredients;
//        public ResultDTO result;
//    }
//    [Serializable]
//    private class IngredientDTO { public string item_type; public int item_id; public int quantity; }
//    [Serializable]
//    private class ResultDTO { public string item_type; public int item_id; public int quantity; }

//    [Serializable]
//    private class ServerCraftResponse { public bool success; public string message; }
//    #endregion
//}
