using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

    // --- Reflection helpers (plus exhaustifs) ---
    private IEnumerable ExtractIngredients(object recipe)
    {
        if (recipe == null) return null;
        Type t = recipe.GetType();

        string[] candidateNames = {
            "ingredients","Ingredients","reagents","Reagents","components","Components",
            "ingredientList","IngredientList","inputs","Inputs"
        };

        foreach (var name in candidateNames)
        {
            var pi = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (pi != null)
            {
                var val = pi.GetValue(recipe) as IEnumerable;
                if (val != null) return val;
            }
            var fi = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi != null)
            {
                var val = fi.GetValue(recipe) as IEnumerable;
                if (val != null) return val;
            }
        }

        // Enfin, si la recette expose directement une paire ou tableau "ingredient" unique
        return null;
    }

    private (Item item, int count) ExtractIngredientInfo(object ingredientObj)
    {
        if (ingredientObj == null) return (null, 0);
        Type it = ingredientObj.GetType();

        // noms probables pour item
        string[] itemNames = { "item", "Item", "resource", "Resource", "ingredient", "Ingredient" };
        Item foundItem = null;
        foreach (var n in itemNames)
        {
            var pi = it.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (pi != null)
            {
                var v = pi.GetValue(ingredientObj);
                if (v is Item) { foundItem = (Item)v; break; }
                if (v is UnityEngine.Object && (v as UnityEngine.Object) is Item) { foundItem = (Item)(UnityEngine.Object)v; break; }
            }
            var fi = it.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi != null)
            {
                var v = fi.GetValue(ingredientObj);
                if (v is Item) { foundItem = (Item)v; break; }
                if (v is UnityEngine.Object && (v as UnityEngine.Object) is Item) { foundItem = (Item)(UnityEngine.Object)v; break; }
            }
        }

        // noms probables pour quantité
        string[] countNames = { "count", "amount", "quantity", "qty", "stack", "required", "needed" };
        int qty = 1;
        bool qtyFound = false;
        foreach (var n in countNames)
        {
            var pi = it.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (pi != null)
            {
                var v = pi.GetValue(ingredientObj);
                if (v is int) { qty = (int)v; qtyFound = true; break; }
                if (v is short) { qty = Convert.ToInt32((short)v); qtyFound = true; break; }
            }
            var fi = it.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi != null)
            {
                var v = fi.GetValue(ingredientObj);
                if (v is int) { qty = (int)v; qtyFound = true; break; }
                if (v is short) { qty = Convert.ToInt32((short)v); qtyFound = true; break; }
            }
        }
        if (!qtyFound) qty = 1;

        return (foundItem, qty);
    }

    // --- Résultat : prise en charge de plusieurs structures courantes ---
    private (Item item, int count) ExtractResult(object recipe)
    {
        if (recipe == null) return (null, 0);
        Type t = recipe.GetType();

        // 1) Champs/propriétés simples
        string[] simpleResultNames = { "result", "Result", "output", "Output", "product", "Product", "resultItem", "resultPrefab" };
        object resultObj = null;
        foreach (var n in simpleResultNames)
        {
            var pi = t.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (pi != null) { resultObj = pi.GetValue(recipe); if (resultObj != null) break; }
            var fi = t.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi != null) { resultObj = fi.GetValue(recipe); if (resultObj != null) break; }
        }

        // 2) Si resultObj est un item direct
        if (resultObj is Item) return ((Item)resultObj, 1);
        if (resultObj is UnityEngine.Object && (resultObj as UnityEngine.Object) is Item) return ((Item)(UnityEngine.Object)resultObj, 1);

        // 3) Parfois resultObj est un container avec { item, count }
        if (resultObj != null)
        {
            Type rt = resultObj.GetType();
            // essayer d'extraire un item depuis le resultObj
            string[] itemNames = { "item", "Item", "result", "Result", "output", "Output", "product", "Product" };
            Item foundItem = null;
            foreach (var n in itemNames)
            {
                var pi = rt.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (pi != null)
                {
                    var v = pi.GetValue(resultObj);
                    if (v is Item) { foundItem = (Item)v; break; }
                    if (v is UnityEngine.Object && (v as UnityEngine.Object) is Item) { foundItem = (Item)(UnityEngine.Object)v; break; }
                }
                var fi = rt.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fi != null)
                {
                    var v = fi.GetValue(resultObj);
                    if (v is Item) { foundItem = (Item)v; break; }
                    if (v is UnityEngine.Object && (v as UnityEngine.Object) is Item) { foundItem = (Item)(UnityEngine.Object)v; break; }
                }
            }

            // count possible
            int count = 1;
            string[] countNames = { "count", "amount", "quantity", "qty", "resultCount", "stack" };
            foreach (var n in countNames)
            {
                var pi = rt.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (pi != null && pi.PropertyType == typeof(int)) { count = (int)pi.GetValue(resultObj); break; }
                var fi = rt.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fi != null && fi.FieldType == typeof(int)) { count = (int)fi.GetValue(resultObj); break; }
            }

            if (foundItem != null) return (foundItem, count);

            // 4) Si resultObj est un GameObject / Prefab : tenter de récupérer un component Item dessus
            if (resultObj is GameObject go)
            {
                var itemComp = go.GetComponent<Item>();
                if (itemComp != null) return (itemComp, 1);
            }
            if (resultObj is UnityEngine.Object uo)
            {
                // si c'est un prefab qui contient un Item ScriptableObject attaché (rare), on essaie un cast
                if (uo is Item) return ((Item)uo, 1);
            }
        }

        // 5) Si pas trouvé : tenter d'autres champs sur la recette (ex : result is array/list)
        var resultListProp = t.GetProperty("results", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                             ?? t.GetProperty("outputs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (resultListProp != null)
        {
            var val = resultListProp.GetValue(recipe) as IEnumerable;
            if (val != null)
            {
                foreach (var entry in val)
                {
                    // prendre le premier convertible
                    var extracted = ExtractResultFromGeneric(entry);
                    if (extracted.item != null) return extracted;
                }
            }
        }

        // 6) rien trouvé
        Debug.LogWarning($"[CraftManager] ExtractResult: type de recipe={t.FullName}, resultObj null ou non convertible.");
        return (null, 0);
    }

    private (Item item, int count) ExtractResultFromGeneric(object entry)
    {
        if (entry == null) return (null, 0);
        // si entry est Item direct
        if (entry is Item) return ((Item)entry, 1);
        // sinon essayer d'extraire item/count sur l'objet
        Type et = entry.GetType();
        string[] itemNames = { "item", "Item", "result", "Result", "product", "Product" };
        foreach (var n in itemNames)
        {
            var pi = et.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (pi != null)
            {
                var v = pi.GetValue(entry);
                if (v is Item) return ((Item)v, 1);
            }
            var fi = et.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi != null)
            {
                var v = fi.GetValue(entry);
                if (v is Item) return ((Item)v, 1);
            }
        }
        return (null, 0);
    }

    // Get quantity in inventory: match by id OR by name (fallback)
    public int GetItemQuantity(UnityEngine.Object itemObj)
    {
        if (itemObj == null) return 0;
        if (InventoryManager.Instance == null) return 0;

        Item it = itemObj as Item;
        if (it == null && itemObj is UnityEngine.Object)
        {
            if (itemObj is Item) it = (Item)itemObj;
        }
        if (it == null) return 0;

        int total = 0;
        int cap = InventoryManager.Instance.SlotCapacity;
        for (int i = 0; i < cap; i++)
        {
            var slotItem = InventoryManager.Instance.GetItemAt(i);
            if (slotItem == null) continue;

            // prefer id match (canonique)
            if (slotItem.id > 0 && it.id > 0)
            {
                if (slotItem.id == it.id) total += InventoryManager.Instance.GetCountAt(i);
            }
            else
            {
                // fallback : compare par reference ou par nom si ids non configurés
                if (ReferenceEquals(slotItem, it) || (!string.IsNullOrEmpty(slotItem.name) && slotItem.name == it.name))
                {
                    total += InventoryManager.Instance.GetCountAt(i);
                }
            }
        }
        return total;
    }

    // --- CanCraft public (utilise reflection pour rester générique) ---
    public bool CanCraft(object recipe)
    {
        if (recipe == null) return false;

        // 1) requiredProfessionLevel
        var lvlProp = recipe.GetType().GetProperty("requiredProfessionLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                      ?? recipe.GetType().GetProperty("RequiredProfessionLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                      ?? (MemberInfo)null as PropertyInfo;
        if (lvlProp != null)
        {
            try
            {
                object v = lvlProp.GetValue(recipe);
                if (v is int && playerProfessionLevel < (int)v) return false;
            }
            catch { /* ignore */ }
        }

        // 2) ingrédients
        var ingredientsEnumerable = ExtractIngredients(recipe);
        if (ingredientsEnumerable == null)
        {
            Debug.LogWarning("[CraftManager] CanCraft: impossible d'extraire la liste d'ingrédients depuis la recette.");
            return false;
        }

        foreach (var ingObj in ingredientsEnumerable)
        {
            var (item, qty) = ExtractIngredientInfo(ingObj);
            if (item == null)
            {
                Debug.LogWarning("[CraftManager] CanCraft: un ingrédient ne contient pas d'Item lisible.");
                return false;
            }
            int have = GetItemQuantity(item);
            if (have < qty)
            {
                // debug utile
                Debug.Log($"[CraftManager] Manque {qty - have} de {item.name} (possédé: {have}, requis: {qty})");
                return false;
            }
        }
        return true;
    }

    // surcharge typée si projet appelle CraftRecipeData
    public bool CanCraft(CraftRecipeData recipe) => CanCraft((object)recipe);

    // --- Remove & Add helpers (utilisent l'InventoryManager existant) ---
    private bool RemoveAmount(Item item, int amount)
    {
        if (item == null || amount <= 0) return false;
        if (InventoryManager.Instance == null) return false;

        int remaining = amount;
        int cap = InventoryManager.Instance.SlotCapacity;

        for (int i = 0; i < cap && remaining > 0; i++)
        {
            var slotItem = InventoryManager.Instance.GetItemAt(i);
            if (slotItem != null && (slotItem.id > 0 ? slotItem.id == item.id : slotItem.name == item.name))
            {
                int have = InventoryManager.Instance.GetCountAt(i);
                int toRemove = Mathf.Min(have, remaining);
                bool ok = InventoryManager.Instance.RemoveAmountAt(i, toRemove);
                if (!ok)
                {
                    Debug.LogError($"[CraftManager] Échec RemoveAmountAt pour l'index {i}.");
                    return false;
                }
                remaining -= toRemove;
            }
        }
        return remaining <= 0;
    }

    private bool AddAmount(Item item, int amount)
    {
        if (item == null || amount <= 0) return false;
        if (InventoryManager.Instance == null) return false;

        int firstIndex = InventoryManager.Instance.Add(item, amount);
        return firstIndex >= 0;
    }

    // --- Craft principal ---
    public bool Craft(object recipe)
    {
        if (recipe == null) return false;

        // check niveau (déjà fait par CanCraft normalement)
        var lvlProp = recipe.GetType().GetProperty("requiredProfessionLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (lvlProp != null)
        {
            try
            {
                object v = lvlProp.GetValue(recipe);
                if (v is int && playerProfessionLevel < (int)v) return false;
            }
            catch { /* ignore */ }
        }

        // extraire ingrédients
        var ingredientsEnumerable = ExtractIngredients(recipe);
        if (ingredientsEnumerable == null)
        {
            Debug.LogWarning("[CraftManager] Craft impossible : pas d'ingrédients trouvés.");
            return false;
        }

        var toConsume = new List<(Item item, int count)>();
        foreach (var ingObj in ingredientsEnumerable)
        {
            var info = ExtractIngredientInfo(ingObj);
            if (info.item == null)
            {
                Debug.LogWarning("[CraftManager] Craft impossible : ingrédient sans Item.");
                return false;
            }
            toConsume.Add(info);
        }

        // sécurité : vérifier encore les quantités
        foreach (var pair in toConsume)
        {
            if (GetItemQuantity(pair.item) < pair.count)
            {
                Debug.Log("[CraftManager] Craft aborted : ingrédients insuffisants.");
                return false;
            }
        }

        // consommer ingrédients (destructif)
        var consumed = new List<(Item item, int count)>();
        foreach (var pair in toConsume)
        {
            bool ok = RemoveAmount(pair.item, pair.count);
            if (!ok)
            {
                Debug.LogError("[CraftManager] Échec suppression ingrédients, rollback.");
                foreach (var c in consumed) AddAmount(c.item, c.count);
                return false;
            }
            consumed.Add(pair);
        }

        // extraire résultat
        var (resultItem, resultCount) = ExtractResult(recipe);
        if (resultItem == null)
        {
            Debug.LogWarning("[CraftManager] Craft : résultat non identifié (non-Item).");
            foreach (var c in consumed) AddAmount(c.item, c.count); // rollback
            return false;
        }

        // ajouter le résultat
        bool addOk = AddAmount(resultItem, resultCount);
        if (!addOk)
        {
            Debug.LogWarning("[CraftManager] Pas de place pour le résultat. Rollback.");
            foreach (var c in consumed) AddAmount(c.item, c.count);
            return false;
        }

        Debug.Log($"Craft réussi ! Résultat : {resultItem.name} x{resultCount}");

        // Sauvegarde optionnelle
        if (InventoryManager.Instance != null && (InventoryManager.Instance.DebugToggle == null || InventoryManager.Instance.DebugToggle.isOn))
        {
            InventorySaveSystem.Save(InventoryManager.Instance);
        }
        return true;
    }

    public bool Craft(CraftRecipeData recipe) => Craft((object)recipe);
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
