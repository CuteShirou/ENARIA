// Data_SkillAnimation.cs
// ScriptableObject servant de "fiche" d'animation 2D réutilisable.
// Permet de cataloguer ID, sprites, FPS, options, teinte/échelle, et de garder
// une référence optionnelle vers le Prefab généré par l'outil.

using UnityEngine;

[CreateAssetMenu(menuName = "ENARIA/Skill Animation", fileName = "New_SkillAnimation")]
public class Data_SkillAnimation : ScriptableObject
{
    // =======================================================================
    // Metadata
    // =======================================================================

    // Identifiant technique unique (ex: "slash_knife")
    public string id = "new_animation";

    // Nom lisible (ex: "Slash Knife")
    public string displayName = "New Animation";

    // =======================================================================
    // Frames
    // =======================================================================

    // Liste de sprites affichés séquentiellement (ordre d'animation)
    public Sprite[] frames;

    // =======================================================================
    // Lecture (paramètres de l'AnimationRunner)
    // =======================================================================

    public float framesPerSecond = 12f;    // Images/seconde
    public bool loop = false;              // Rejoue en boucle
    public bool playOnAwake = true;        // Démarre à OnEnable
    public bool autoDestroyOnEnd = true;   // Détruit le GO à la fin si non-loop
    public bool useUnscaledTime = false;   // Ignore Time.timeScale
    public bool randomStartFrame = false;  // Frame initiale aléatoire
    public float startDelay = 0f;          // Délai avant démarrage (secondes)

    // =======================================================================
    // Rendu
    // =======================================================================

    public string sortingLayerName = "Default"; // Couche de tri 2D
    public int sortingOrder = 0;                // Ordre dans la couche
    public Vector2 prefabScale = Vector2.one;   // Échelle par défaut du runner
    public Color tintColor = Color.white;       // Teinte appliquée au SpriteRenderer

    // =======================================================================
    // Prefab (optionnel)
    // =======================================================================

    // Prefab prêt à jouer (créé par ton Tool_Animation2D_Creator)
    public Sprite_AnimationRunner prefab;

    private void OnValidate()
    {
        // Clamp des valeurs et sécurités simples à l'édition
        if (framesPerSecond < 1f) framesPerSecond = 1f;
        if (prefabScale.x <= 0f) prefabScale.x = 1f;
        if (prefabScale.y <= 0f) prefabScale.y = 1f;
        if (startDelay < 0f) startDelay = 0f;
    }
}
