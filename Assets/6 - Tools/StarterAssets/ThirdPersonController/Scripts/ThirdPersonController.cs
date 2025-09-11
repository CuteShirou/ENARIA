using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Serialization; // Pour FormerlySerializedAs

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Click To Move")]
    [FormerlySerializedAs("ClickMoveSpeed")]
    public float walkSpeed = 5f;                      // Vitesse de marche
    public float runSpeed = 8f;                       // Vitesse de course
    public LayerMask ClickableLayers;                 // Couches cliquables pour le raycast
    public GameObject CinemachineCameraTarget;        // Cible caméra (inchangé)

    [Header("Animation Settings")]
    public float runDistanceThreshold = 5f;           // Seuil de distance initiale pour basculer en course
    public float runAnimSpeedMultiplier = 1.5f;       // Multiplicateur de vitesse Animator quand on court

    private CharacterController _controller;          // Référence CharacterController
    private Animator _animator;                       // Référence Animator
    private GameObject _mainCamera;                   // Cache caméra active

    private Entity_Animations _playerAnimations;      // Proxy d’animations
    private bool _hasAnimProxy;                       // True si proxy présent

    private Vector3 _clickTarget;                     // Cible click-to-move
    private bool _isClickMoving = false;              // Déplacement en cours
    private bool _isRunningThisPath = false;          // Verrou marche/course pour ce trajet

    private bool _hasAnimator;                        // True si Animator trouvé
    private int _animIDSpeed;                         // Fallback param Speed
    private int _animIDMotionSpeed;                   // Fallback param MotionSpeed

    private bool _clickDetected = false;              // Clic détecté cette frame
    public GameObject uiPrefab;                       // Prefab UI joueur (inchangé)
    private GameObject _uiInstance;                   // Instance UI

    // Accesseurs publics
    public bool IsInCombat { get; set; } = false;     // Indique si en combat
    public bool IsClickMoving => _isClickMoving;      // État déplacement
    public Vector3 ClickTarget => _clickTarget;       // Cible actuelle
    public bool HasAnimator => _hasAnimator;          // Présence Animator
    public Animator Animator => _animator;            // Expose Animator
    public int AnimIDSpeed => _animIDSpeed;           // ID Speed (fallback)
    public int AnimIDMotionSpeed => _animIDMotionSpeed; // ID MotionSpeed (fallback)

    // Initialisation des références
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();     // Récupère CharacterController
        _animator = GetComponent<Animator>();                  // Récupère Animator
        _playerAnimations = GetComponent<Entity_Animations>(); // Récupère proxy anim
        _hasAnimProxy = _playerAnimations != null;             // Proxy présent ?
        _hasAnimator = _animator != null;                      // Animator présent ?

        AssignAnimationIDs();                                   // Enregistre IDs fallback
        EnsureMainCamera();                                     // Sécurise la caméra

        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, 0f);              // Fallback: Speed à 0
            _animator.SetFloat(_animIDMotionSpeed, 0f);        // Fallback: MotionSpeed à 0
            _animator.speed = 1f;                              // Vitesse lecture par défaut
        }
    }

    // Post-init (ordre d’exécution garanti après Awake() des autres composants)
    private void Start()
    {
        if (_mainCamera == null || (Camera.main != null && _mainCamera != Camera.main.gameObject))
        {
            EnsureMainCamera();                                // Revalide caméra active
        }

        // Important: on remet Walk à false ici (et plus dans Awake) pour éviter le null
        if (_hasAnimProxy) _playerAnimations.SetWalk(false);   // State Walk au repos

        if (runAnimSpeedMultiplier < 1f) runAnimSpeedMultiplier = 1f; // Clamp valeurs
        if (runDistanceThreshold < 0f) runDistanceThreshold = 0f;
        if (runSpeed < walkSpeed) runSpeed = walkSpeed;               // Cohérence
    }

    // Contexte réseau éventuel (inchangé)
    public void OnStartLocalPlayer()
    {
        Transform camTransform = transform.Find("PlayerCamera");      // Cherche caméra enfant
        if (camTransform != null)
        {
            Camera cam = camTransform.GetComponent<Camera>();         // Récupère caméra
            if (cam != null)
            {
                cam.enabled = true;                                   // Active caméra
                cam.tag = "MainCamera";                               // Tag MainCamera
                _mainCamera = cam.gameObject;                         // Mémorise
                Debug.Log("Camera locale activée (OnStartLocalPlayer) pour : " + gameObject.name);
            }
        }

        Transform uiTransform = transform.Find("UI");                 // Cherche UI existante
        if (_uiInstance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("UI/PlayerUI"); // Charge UI
            if (prefab != null)
            {
                _uiInstance = Instantiate(prefab);                     // Instancie UI
                _uiInstance.name = "UI_" + gameObject.name;            // Nom lisible
                Canvas canvas = _uiInstance.GetComponent<Canvas>();    // Récupère Canvas
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay; // Mode affichage
                    canvas.enabled = true;                             // Active
                }
                DontDestroyOnLoad(_uiInstance);                        // Persiste
            }
            else
            {
                Debug.LogWarning("UI prefab introuvable dans Resources/UI/PlayerUI");
            }
        }

        EnsureMainCamera();                                           // Sécurise caméra
    }

    // Boucle principale
    private void Update()
    {
        // Bloque si une UI prioritaire est ouverte
        if (typeof(UIToggle).GetProperty("IsInventoryOpen") != null)
        {
            if (UIToggle.IsInventoryOpen) return;
        }
        if (typeof(OnClick3D).GetField("cerealesIsActive") != null)
        {
            if (OnClick3D.cerealesIsActive) return;
        }

        // Ignore les clics au-dessus de l’UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Détection du clic gauche (InputSystem ou InputLegacy)
        if ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Mouse.current == null && Input.GetMouseButtonDown(0)))
        {
            _clickDetected = true;                                    // Note le clic
            Debug.Log("Clic détecté sur : " + gameObject.name);
        }

        // Maintient Grounded à true pour l’Animator si utilisé
        if (_hasAnimator) _animator.SetBool("Grounded", true);

        ClickToMove();                                                // Gère le déplacement
    }

    // Déplacement click-to-move + verrou marche/course pour tout le trajet
    private void ClickToMove()
    {
        if (_clickDetected)
        {
            _clickDetected = false;                                   // Consomme l’événement
            EnsureMainCamera();                                       // S’assure d’une caméra
            if (_mainCamera == null)
            {
                Debug.LogWarning("Camera non définie (aucune MainCamera trouvée).");
                return;
            }

            Vector2 screenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition; // Position écran
            Ray ray = _mainCamera.GetComponent<Camera>().ScreenPointToRay(screenPos); // Raycast

            int mask = ClickableLayers.value == 0 ? ~0 : ClickableLayers.value; // Si aucun layer, tape tout
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask))
            {
                _clickTarget = hit.point;                              // Enregistre la cible
                _isClickMoving = true;                                 // Lance le déplacement

                // Calcule distance horizontale initiale et verrouille le mode
                Vector3 flatTarget = new Vector3(_clickTarget.x, transform.position.y, _clickTarget.z);
                float initialDistance = (flatTarget - transform.position).magnitude;
                _isRunningThisPath = initialDistance > runDistanceThreshold;

                // Met à jour la vitesse de lecture Animator selon le mode
                ApplyWalkAnimatorSpeed(_isRunningThisPath);
            }
        }

        if (_isClickMoving)
        {
            Vector3 flatTarget = new Vector3(_clickTarget.x, transform.position.y, _clickTarget.z);
            Vector3 direction = (flatTarget - transform.position);
            float distance = direction.magnitude;

            if (distance > 0.1f)
            {
                direction.Normalize();

                // Utilise la vitesse verrouillée pour tout le trajet
                float currentMoveSpeed = _isRunningThisPath ? runSpeed : walkSpeed;
                _controller.Move(direction * currentMoveSpeed * Time.deltaTime);                     // Déplace
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f); // Oriente

                // Active Walk pendant le déplacement
                if (_hasAnimProxy) _playerAnimations.SetWalk(true);
                else if (_hasAnimator)
                {
                    _animator.SetFloat(_animIDSpeed, currentMoveSpeed);                              // Fallback
                    _animator.SetFloat(_animIDMotionSpeed, 1f);
                }

                // Assure vitesse de lecture conforme
                ApplyWalkAnimatorSpeed(_isRunningThisPath);
            }
            else
            {
                // Arrêt au point cible
                _clickTarget = Vector3.zero;
                _isClickMoving = false;
                _isRunningThisPath = false;

                if (_hasAnimProxy) _playerAnimations.SetWalk(false);
                else if (_hasAnimator)
                {
                    _animator.SetFloat(_animIDSpeed, 0f);                                            // Fallback
                    _animator.SetFloat(_animIDMotionSpeed, 0f);
                }

                if (_hasAnimator) _animator.speed = 1f;                                              // Restaure vitesse
            }
        }
        else
        {
            // Hors déplacement, garantit un état propre
            if (_hasAnimProxy) _playerAnimations.SetWalk(false);
            if (_hasAnimator) _animator.speed = 1f;
            _isRunningThisPath = false;
        }
    }

    // Ajuste la vitesse de lecture Animator selon marche/course
    private void ApplyWalkAnimatorSpeed(bool isRunning)
    {
        if (!_hasAnimator) return;                                    // Nécessite un Animator
        float targetSpeed = isRunning ? runAnimSpeedMultiplier : 1f;  // Vitesse visuelle
        _animator.speed = targetSpeed;                                // Applique
    }

    // Enregistre les IDs d’Animator (fallback)
    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");                // ID param Speed
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");    // ID param MotionSpeed
    }

    // Stoppe immédiatement le mouvement en cours (ex: entrée en combat)
    public void ForceStopMovement()
    {
        _clickTarget = Vector3.zero;                                  // Efface la cible
        _isClickMoving = false;                                       // Stop le déplacement
        _isRunningThisPath = false;                                   // Déverrouille

        if (_hasAnimProxy) _playerAnimations.SetWalk(false);          // Coupe Walk
        else if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, 0f);                     // Fallback
            _animator.SetFloat(_animIDMotionSpeed, 0f);
        }

        if (_hasAnimator) _animator.speed = 1f;                       // Restaure vitesse
        Debug.Log("Mouvement stoppé manuellement.");
    }

    // Sécurise la présence d’une caméra pour les raycasts
    private void EnsureMainCamera()
    {
        if (_mainCamera != null) return;                              // Déjà définie

        Camera cam = Camera.main;                                     // Cherche Camera.main
        if (cam == null) cam = GetComponentInChildren<Camera>(true);  // Cherche caméra enfant
        if (cam == null)
        {
            var cams = FindObjectsOfType<Camera>();                   // Prend n’importe quelle caméra
            if (cams.Length > 0) cam = cams[0];
        }

        if (cam != null)
        {
            _mainCamera = cam.gameObject;                             // Mémorise la caméra
            if (cam.tag != "MainCamera") cam.tag = "MainCamera";      // Assure le tag
            Debug.Log("Camera assignée: " + _mainCamera.name);
        }
    }
}
