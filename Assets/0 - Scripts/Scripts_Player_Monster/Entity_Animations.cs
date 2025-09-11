using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Entity_Animations : MonoBehaviour
{
    // Référence Animator
    Animator animator;

    // Noms des paramètres booléens (identiques à ta config)
    [Header("Animator Bool Parameters")]
    [SerializeField] string castMagicParamName = "CastMagicSpell";   // Bool pour cast magique
    [SerializeField] string castPhysicParamName = "CastPhysicSpell"; // Bool pour cast physique/dex
    [SerializeField] string deathParamName = "Death";                // Bool pour la mort
    [SerializeField] string walkParamName = "Walk";                  // Bool pour marcher
    [SerializeField] string hitParamName = "Hit";                    // Bool pour coup reçu

    // Hash pour accès rapide
    int hashCastMagic;
    int hashCastPhysic;
    int hashDeath;
    int hashWalk;
    int hashHit;

    // Durées des impulsions one-shot (adapter aux clips/transitions)
    [Header("One-Shot Durations (seconds)")]
    [SerializeField] float castMagicDuration = 0.8f;   // Durée d'activation de CastMagicSpell
    [SerializeField] float castPhysicDuration = 0.7f;  // Durée d'activation de CastPhysicSpell
    [SerializeField] float hitDuration = 0.4f;         // Durée d'activation de Hit

    // Durée utile pour l'animation de mort
    [Header("Death Settings")]
    [SerializeField] float deathDuration = 1.2f;       // Durée avant retour visuel à IDLE

    // Options
    [Header("Options")]
    [SerializeField] bool blockWalkDuringOneShot = true; // Bloque Walk pendant un one-shot
    [SerializeField] bool enableDebugInput = false;      // Active Test_InputAnimation dans Update

    // États internes
    bool isPlayingOneShot;   // Vrai pendant Cast/Hit
    bool isDying;            // Vrai pendant Mort
    Coroutine currentOneShot;
    Coroutine deathRoutine;

    // Événement déclenché à la fin de l'anim de Mort (après retour possible à IDLE)
    public event Action OnDeathAnimationComplete;

    // Exposition simple : occupé si one-shot en cours ou mort
    public bool IsBusy { get { return isPlayingOneShot || isDying; } }

    void Awake()
    {
        // Récupération Animator + hash
        animator = GetComponent<Animator>();
        hashCastMagic = Animator.StringToHash(castMagicParamName);
        hashCastPhysic = Animator.StringToHash(castPhysicParamName);
        hashDeath = Animator.StringToHash(deathParamName);
        hashWalk = Animator.StringToHash(walkParamName);
        hashHit = Animator.StringToHash(hitParamName);
    }

    void Update()
    {
        // Test clavier optionnel (W/Q/E/T/R) si activé dans l'Inspector
        if (enableDebugInput) Test_InputAnimation();
    }

    // Test clavier simple pour tous les états (W/Q/E/T/R)
    void Test_InputAnimation()
    {
        // Mort prioritaire (R)
        if (Input.GetKeyDown(KeyCode.R))
            PlayDeath();

        // One-shots si pas occupé (Q/E/T)
        if (!IsBusy)
        {
            if (Input.GetKeyDown(KeyCode.Q)) PlayCastMagic();   // Q => Cast Magie
            if (Input.GetKeyDown(KeyCode.E)) PlayCastPhysic();  // E => Cast Physique
            if (Input.GetKeyDown(KeyCode.T)) PlayHit();         // T => Hit
        }

        // Déplacement : maintenir W pour marcher (bloqué si occupé)
        if (!IsBusy) SetWalk(Input.GetKey(KeyCode.W));
        else SetWalk(false);
    }

    // Active/Désactive la marche continue (Walk)
    public void SetWalk(bool active)
    {
        // Empêche Walk pendant un one-shot si option active
        if (blockWalkDuringOneShot && isPlayingOneShot)
        {
            animator.SetBool(hashWalk, false);
            return;
        }

        // Empêche Walk pendant la mort
        if (isDying)
        {
            animator.SetBool(hashWalk, false);
            return;
        }

        animator.SetBool(hashWalk, active);
    }

    // Démarre explicitement la marche
    public void PlayWalk()
    {
        SetWalk(true);
    }

    // Arrête explicitement la marche
    public void StopWalk()
    {
        SetWalk(false);
    }

    // Lance un cast magique (impulsion booléenne)
    public bool PlayCastMagic()
    {
        if (IsBusy) return false;
        StartOneShotBool(hashCastMagic, castMagicDuration);
        return true;
    }

    // Lance un cast physique/dextérité (impulsion booléenne)
    public bool PlayCastPhysic()
    {
        if (IsBusy) return false;
        StartOneShotBool(hashCastPhysic, castPhysicDuration);
        return true;
    }

    // Lance l'animation de coup reçu (impulsion booléenne)
    public bool PlayHit()
    {
        if (isDying) return false;
        StartOneShotBool(hashHit, hitDuration);
        return true;
    }

    // Lance la mort, coupe les autres états, puis revient à IDLE après la durée
    public bool PlayDeath(Action onComplete = null)
    {
        if (isDying) return false;

        // Coupe un éventuel one-shot en cours
        if (currentOneShot != null)
        {
            StopCoroutine(currentOneShot);
            currentOneShot = null;
        }

        // Stop marche et libère one-shot
        SetWalk(false);
        isPlayingOneShot = false;

        // Drapeau mort
        isDying = true;

        // Force tous les bool conflictuels à false, active Death
        animator.SetBool(hashCastMagic, false);
        animator.SetBool(hashCastPhysic, false);
        animator.SetBool(hashHit, false);
        animator.SetBool(hashDeath, true);

        // Routine de fin de mort
        if (deathRoutine != null) StopCoroutine(deathRoutine);
        deathRoutine = StartCoroutine(Co_DeathThenIdle(onComplete));

        return true;
    }

    // Démarre une impulsion booléenne avec durée
    void StartOneShotBool(int paramHash, float duration)
    {
        if (currentOneShot != null) StopCoroutine(currentOneShot);
        currentOneShot = StartCoroutine(Co_OneShotBool(paramHash, duration));
    }

    // Maintient le bool à true pendant "duration" puis repasse à false
    IEnumerator Co_OneShotBool(int paramHash, float duration)
    {
        isPlayingOneShot = true;

        // Stop Walk si option active
        if (blockWalkDuringOneShot) animator.SetBool(hashWalk, false);

        // Impulsion
        animator.SetBool(paramHash, true);
        yield return null;
        yield return new WaitForSeconds(duration);

        animator.SetBool(paramHash, false);
        isPlayingOneShot = false;
        currentOneShot = null;
    }

    // Gère la fin de mort, retour à IDLE et callback
    IEnumerator Co_DeathThenIdle(Action onComplete)
    {
        yield return null;
        yield return new WaitForSeconds(deathDuration);

        // Permet la transition Dead -> IDLE
        animator.SetBool(hashDeath, false);

        // Fin mort
        isDying = false;
        deathRoutine = null;

        // Callbacks éventuels
        onComplete?.Invoke();
        OnDeathAnimationComplete?.Invoke();
    }

    // Utilitaire: force retour immédiat à IDLE (tous bools à false)
    public void ForceResetToIdle()
    {
        animator.SetBool(hashCastMagic, false);
        animator.SetBool(hashCastPhysic, false);
        animator.SetBool(hashHit, false);
        animator.SetBool(hashWalk, false);
        animator.SetBool(hashDeath, false);
        isPlayingOneShot = false;
        isDying = false;
    }
}
