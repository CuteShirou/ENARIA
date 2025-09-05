// Sprite_AnimationRunner.cs
// Lecteur d'animation 2D (flipbook) basé sur une liste de Sprites.
// Fournit une API simple pour démarrer à une position et confirmer la fin.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("ENARIA/Sprite Animation Runner")]
public class Sprite_AnimationRunner : MonoBehaviour
{
    // ============================ Metadata ============================
    public string id = "new_animation";            // Identifiant technique
    public string displayName = "New Animation";   // Nom lisible

    // ============================= Frames =============================
    public Sprite[] frames;                        // Suite de Sprites à lire

    // ============================ Lecture =============================
    public float framesPerSecond = 12f;            // Images par seconde
    public bool loop = false;                      // Rejoue en boucle
    public bool playOnAwake = true;                // Démarre à OnEnable
    public bool autoDestroyOnEnd = true;           // Détruit le GO à la fin (si non-loop)
    public bool useUnscaledTime = false;           // Ignore Time.timeScale
    public bool randomStartFrame = false;          // Frame de départ aléatoire
    public float startDelay = 0f;                  // Délai avant démarrage

    // ============================= Événements ==========================
    public UnityEvent onCompleted;                 // Appelé quand l'anim se termine (non-loop)

    // ============================ État interne =========================
    private SpriteRenderer spriteRenderer;         // SpriteRenderer local
    private Coroutine playRoutine;                 // Routine de lecture
    private bool isPlaying = false;                // Indique si en lecture
    private int currentFrame = 0;                  // Index de frame courant

    private bool hasCompleted = false;             // Indique qu'une lecture vient de finir
    private Action onFinishedOnce;                 // Callback de fin one-shot

    // ------------------------------------------------------------------
    // Initialisation
    // ------------------------------------------------------------------
    private void Awake()
    {
        // Récupère/ajoute le SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        // Affiche la première frame si disponible
        if (frames != null && frames.Length > 0)
        {
            currentFrame = Mathf.Clamp(currentFrame, 0, frames.Length - 1);
            spriteRenderer.sprite = frames[currentFrame];
        }
    }

    private void OnEnable()
    {
        // Démarre automatiquement si demandé
        if (playOnAwake) Play();
    }

    private void OnDisable()
    {
        // Stoppe la lecture si le GO est désactivé
        Stop();
    }

    // ------------------------------------------------------------------
    // Contrôle de lecture
    // ------------------------------------------------------------------

    public void Play()
    {
        // Démarre la lecture si possible
        if (frames == null || frames.Length == 0 || isPlaying) return;
        hasCompleted = false; // reset du flag de fin
        playRoutine = StartCoroutine(CoPlay());
        isPlaying = true;
    }

    public void Stop()
    {
        // Stoppe la lecture et revient à la première frame
        if (playRoutine != null) StopCoroutine(playRoutine);
        playRoutine = null;
        isPlaying = false;

        if (frames != null && frames.Length > 0)
        {
            currentFrame = 0;
            if (spriteRenderer != null) spriteRenderer.sprite = frames[currentFrame];
        }
    }

    public void Pause()
    {
        // Met en pause sans réinitialiser l'index
        if (playRoutine != null) StopCoroutine(playRoutine);
        playRoutine = null;
        isPlaying = false;
    }

    public float GetDuration()
    {
        // Retourne la durée totale d'une passe (en secondes)
        if (frames == null || frames.Length == 0 || framesPerSecond <= 0f) return 0f;
        return frames.Length / framesPerSecond;
    }

    public bool IsPlaying()
    {
        // Indique si l'animation est en cours
        return isPlaying;
    }

    // ------------------------------------------------------------------
    // Démarrer avec position / rotation / parent
    // ------------------------------------------------------------------

    public void PlayAtPosition(Vector3 worldPosition)
    {
        // Place l'objet en monde puis lance la lecture
        transform.position = worldPosition;
        Play();
    }

    public void PlayAtPositionWithRotation(Vector3 worldPosition, Quaternion rotation)
    {
        // Place + oriente l'objet en monde puis lance la lecture
        transform.SetPositionAndRotation(worldPosition, rotation);
        Play();
    }

    public void PlayAtPositionWithParent(Vector3 position, Transform parent, bool positionIsWorldSpace = true)
    {
        // Définit le parent, place la position (monde ou locale), puis lance la lecture
        transform.SetParent(parent, worldPositionStays: positionIsWorldSpace);
        if (positionIsWorldSpace) transform.position = position;
        else transform.localPosition = position;
        Play();
    }

    // ------------------------------------------------------------------
    // Confirmation de fin (callback / attente / flag)
    // ------------------------------------------------------------------

    public void PlayWithCallback(Action onFinished)
    {
        // Démarre la lecture et appellera un callback one-shot à la fin
        onFinishedOnce = onFinished;
        Play();
    }

    public IEnumerator WaitForCompletion()
    {
        // Permet d'attendre la fin depuis une coroutine externe
        // Note : si AutoDestroyOnEnd = true, appelez ceci depuis un autre MonoBehaviour.
        while (isPlaying) yield return null;
    }

    public bool HasCompleted()
    {
        // Indique si la dernière lecture a été menée jusqu'à la fin
        return hasCompleted;
    }

    public static Sprite_AnimationRunner SpawnAndPlay(Sprite_AnimationRunner prefab, Vector3 position, Transform parent = null, Action onFinished = null)
    {
        // Instancie un prefab et lance la lecture
        if (prefab == null) return null;
        var inst = Instantiate(prefab, position, Quaternion.identity, parent);
        if (onFinished != null) inst.PlayWithCallback(onFinished);
        else inst.Play();
        return inst;
    }

    // ------------------------------------------------------------------
    // Boucle interne de lecture (coroutine)
    // ------------------------------------------------------------------
    private IEnumerator CoPlay()
    {
        // Délai de départ
        if (startDelay > 0f)
        {
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(startDelay);
            else yield return new WaitForSeconds(startDelay);
        }

        // Choix de la frame initiale
        if (randomStartFrame && frames != null && frames.Length > 0)
            currentFrame = UnityEngine.Random.Range(0, frames.Length);
        else
            currentFrame = Mathf.Clamp(currentFrame, 0, Mathf.Max(0, (frames?.Length ?? 1) - 1));

        // Durée d'une frame
        float frameTime = (framesPerSecond > 0f) ? (1f / framesPerSecond) : 0.0833f;

        while (true)
        {
            // Sécurité de base
            if (frames == null || frames.Length == 0 || spriteRenderer == null)
                yield break;

            // Affiche la frame courante
            spriteRenderer.sprite = frames[currentFrame];

            // Attend la durée d'une frame
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(frameTime);
            else yield return new WaitForSeconds(frameTime);

            // Passe à la frame suivante
            currentFrame++;

            // Gère la fin ou la boucle
            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    // Fin d'animation
                    hasCompleted = true;
                    isPlaying = false;

                    // Callback code (one-shot)
                    onFinishedOnce?.Invoke();
                    onFinishedOnce = null;

                    // UnityEvent (inspector)
                    onCompleted?.Invoke();

                    // Auto-destruction éventuelle
                    if (autoDestroyOnEnd)
                        Destroy(gameObject);

                    yield break;
                }
            }
        }
    }
}
