// Animation2D_Utility.cs
// Petit helper pour instancier et jouer une animation à partir d'un Data_SkillAnimation,
// ou pour appliquer une fiche sur un Sprite_AnimationRunner existant.

using System;
using UnityEngine;

public static class Animation2D_Utility
{
    public static Sprite_AnimationRunner SpawnFromData(
        Data_SkillAnimation data,
        Vector3 position,
        Transform parent = null,
        Action onFinished = null)
    {
        // Vérifie la fiche
        if (data == null) return null;

        // Si un Prefab est renseigné dans la fiche, on l'utilise
        if (data.prefab != null)
        {
            var inst = UnityEngine.Object.Instantiate(data.prefab, position, Quaternion.identity, parent);

            // Applique éventuellement la fiche pour garantir que les réglages sont à jour
            ApplyDataToRunner(inst, data);

            if (onFinished != null) inst.PlayWithCallback(onFinished);
            else inst.Play();
            return inst;
        }

        // Sinon, on construit un GameObject minimal à la volée
        var go = new GameObject(string.IsNullOrEmpty(data.displayName) ? data.name : data.displayName);
        go.transform.SetPositionAndRotation(position, Quaternion.identity);
        go.transform.localScale = new Vector3(data.prefabScale.x, data.prefabScale.y, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = (data.frames != null && data.frames.Length > 0) ? data.frames[0] : null;
        sr.color = data.tintColor;
        sr.sortingLayerName = data.sortingLayerName;
        sr.sortingOrder = data.sortingOrder;

        var runner = go.AddComponent<Sprite_AnimationRunner>();
        ApplyDataToRunner(runner, data);

        if (onFinished != null) runner.PlayWithCallback(onFinished);
        else runner.Play();

        return runner;
    }

    // =======================================================================
    // Variante avec rotation initiale
    // =======================================================================

    public static Sprite_AnimationRunner SpawnFromDataWithRotation(
        Data_SkillAnimation data,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null,
        Action onFinished = null)
    {
        // Instancie puis applique les réglages
        var inst = SpawnFromData(data, position, parent, onFinished);
        if (inst != null) inst.transform.rotation = rotation;
        return inst;
    }

    public static void ApplyDataToRunner(Sprite_AnimationRunner runner, Data_SkillAnimation data)
    {
        // Affecte tous les paramètres de lecture et de rendu
        if (runner == null || data == null) return;

        runner.id = data.id;
        runner.displayName = data.displayName;
        runner.frames = data.frames;
        runner.framesPerSecond = data.framesPerSecond;
        runner.loop = data.loop;
        runner.playOnAwake = data.playOnAwake;
        runner.autoDestroyOnEnd = data.autoDestroyOnEnd;
        runner.useUnscaledTime = data.useUnscaledTime;
        runner.randomStartFrame = data.randomStartFrame;
        runner.startDelay = data.startDelay;

        var sr = runner.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = data.tintColor;
            sr.sortingLayerName = data.sortingLayerName;
            sr.sortingOrder = data.sortingOrder;
        }

        runner.transform.localScale = new Vector3(data.prefabScale.x, data.prefabScale.y, 1f);
    }
}
