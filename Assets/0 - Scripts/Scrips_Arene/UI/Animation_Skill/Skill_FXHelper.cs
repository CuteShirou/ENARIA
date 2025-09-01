// Skill_FXHelper.cs
// Helper pour jouer le FX lié à un Skill_Binding à une position donnée,
// avec offset Y, et attendre proprement la fin si besoin.

using System;
using System.Collections;
using UnityEngine;

public static class Skill_FXHelper
{
    /// <summary>
    ///   Joue le FX du binding à "basePos" (+ offset Y). Retourne l'instance (ou null).
    /// </summary>
    public static Sprite_AnimationRunner PlayFx(Skill_Binding binding, Vector3 basePos, Transform parent = null, Action onFinished = null)
    {
        if (binding == null) { onFinished?.Invoke(); return null; }

        Vector3 pos = basePos + new Vector3(0f, binding.fxYOffset, 0f);

        // Priorité à la fiche (Data_SkillAnimation)
        if (binding.fxData != null)
        {
            var inst = Animation2D_Utility.SpawnFromData(binding.fxData, pos, parent, onFinished);
            return inst;
        }

        // Sinon le prefab direct (Sprite_AnimationRunner)
        if (binding.fxPrefabOverride != null)
        {
            var inst = Sprite_AnimationRunner.SpawnAndPlay(binding.fxPrefabOverride, pos, parent, onFinished);
            return inst;
        }

        // Aucun FX configuré
        onFinished?.Invoke();
        return null;
    }

    /// <summary>
    ///   Variante coroutine: joue le FX et attend la fin.
    /// </summary>
    public static IEnumerator PlayFxAndWait(Skill_Binding binding, Vector3 basePos, Transform parent = null)
    {
        var inst = PlayFx(binding, basePos, parent, null);
        if (inst == null) yield break;
        yield return inst.WaitForCompletion();
    }

    /// <summary>
    ///   Variante avec rotation initiale (ex: orienter un slash).
    /// </summary>
    public static Sprite_AnimationRunner PlayFxWithRotation(Skill_Binding binding, Vector3 basePos, Quaternion rotation, Transform parent = null, Action onFinished = null)
    {
        var inst = PlayFx(binding, basePos, parent, onFinished);
        if (inst != null) inst.transform.rotation = rotation;
        return inst;
    }
}
