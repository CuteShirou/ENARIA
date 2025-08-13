using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Click To Move")]
    public float ClickMoveSpeed = 5f;
    public LayerMask ClickableLayers;
    public GameObject CinemachineCameraTarget;

    private CharacterController _controller;
    private Animator _animator;
    private GameObject _mainCamera;

    private Vector3 _clickTarget;
    private bool _isClickMoving = false;
    private bool _hasAnimator;
    private int _animIDSpeed;
    private int _animIDMotionSpeed;

    private bool _clickDetected = false;
    public GameObject uiPrefab;
    private GameObject _uiInstance;

    // Accesseurs publics
    public bool IsInCombat { get; set; } = false;
    public bool IsClickMoving => _isClickMoving;
    public Vector3 ClickTarget => _clickTarget;
    public bool HasAnimator => _hasAnimator;
    public Animator Animator => _animator;
    public int AnimIDSpeed => _animIDSpeed;
    public int AnimIDMotionSpeed => _animIDMotionSpeed;

    private void Awake()
    {
        // Composants de base
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _hasAnimator = _animator != null;
        AssignAnimationIDs();

        // Sécurise une caméra active pour le raycast
        EnsureMainCamera();
    }

    private void Start()
    {
        // Si une autre caméra devient MainCamera au Start (ex: Cinemachine),
        // on la récupère ici également.
        if (_mainCamera == null || Camera.main != null && _mainCamera != Camera.main.gameObject)
        {
            EnsureMainCamera();
        }
    }

    /// <summary>
    /// Appelé par un contexte réseau. On garde pour compatibilité,
    /// mais on sécurise aussi hors réseau via Awake/Start.
    /// </summary>
    public void OnStartLocalPlayer()
    {
        // Recherche d'une caméra enfant éventuelle
        Transform camTransform = transform.Find("PlayerCamera");
        if (camTransform != null)
        {
            Camera cam = camTransform.GetComponent<Camera>();
            if (cam != null)
            {
                cam.enabled = true;
                cam.tag = "MainCamera";
                _mainCamera = cam.gameObject;
                Debug.Log("📸 Caméra locale activée (OnStartLocalPlayer) pour : " + gameObject.name);
            }
        }

        // UI (inchangé)
        Transform uiTransform = transform.Find("UI");
        if (_uiInstance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("UI/PlayerUI");
            if (prefab != null)
            {
                _uiInstance = Instantiate(prefab);
                _uiInstance.name = "UI_" + gameObject.name;
                Canvas canvas = _uiInstance.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.enabled = true;
                }
                DontDestroyOnLoad(_uiInstance);
            }
            else
            {
                Debug.LogWarning("❌ UI prefab introuvable dans Resources/UI/PlayerUI");
            }
        }

        // On s'assure à nouveau d'avoir une caméra.
        EnsureMainCamera();
    }

    private void Update()
    {
        // Garde-fous UI / états
        if (typeof(UIToggle).GetProperty("IsInventoryOpen") != null)
        {
            if (UIToggle.IsInventoryOpen) return;
        }
        if (typeof(OnClick3D).GetField("cerealesIsActive") != null)
        {
            if (OnClick3D.cerealesIsActive) return;
        }

        // Si EventSystem est manquant, on ne bloque pas le clic
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Nouveau Input System : Mouse.current peut être null selon le device.
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current == null && Input.GetMouseButtonDown(0))
        {
            _clickDetected = true;
            Debug.Log("🖱 Clic détecté sur : " + gameObject.name);
        }

        if (_hasAnimator) _animator.SetBool("Grounded", true);

        ClickToMove();
    }

    private void ClickToMove()
    {
        if (_clickDetected)
        {
            _clickDetected = false;

            EnsureMainCamera();
            if (_mainCamera == null)
            {
                Debug.LogWarning("❌ Caméra non définie (aucune MainCamera trouvée).");
                return;
            }

            Vector2 screenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
            Ray ray = _mainCamera.GetComponent<Camera>().ScreenPointToRay(screenPos);

            // Si ClickableLayers vaut 0 (par défaut), on tape tout (~0)
            int mask = ClickableLayers.value == 0 ? ~0 : ClickableLayers.value;
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask))
            {
                Debug.Log("🎯 Raycast hit: " + hit.point);
                _clickTarget = hit.point;
                _isClickMoving = true;
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
                _controller.Move(direction * ClickMoveSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);

                if (_hasAnimator)
                {
                    _animator.SetFloat(_animIDSpeed, ClickMoveSpeed);
                    _animator.SetFloat(_animIDMotionSpeed, 1f);
                }
            }
            else
            {
                _clickTarget = Vector3.zero;
                _isClickMoving = false;
                if (_hasAnimator)
                {
                    _animator.SetFloat(_animIDSpeed, 0f);
                    _animator.SetFloat(_animIDMotionSpeed, 0f);
                }
            }
        }
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    public void ForceStopMovement()
    {
        _clickTarget = Vector3.zero;
        _isClickMoving = false;

        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, 0f);
            _animator.SetFloat(_animIDMotionSpeed, 0f);
        }

        Debug.Log("🛑 Mouvement stoppé manuellement.");
    }

    /// <summary>
    /// Tente de trouver une caméra utilisable :
    /// 1) Camera.main
    /// 2) Une caméra enfant du joueur (active ou inactive)
    /// 3) La première caméra active dans la scène
    /// </summary>
    private void EnsureMainCamera()
    {
        if (_mainCamera != null) return;

        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = GetComponentInChildren<Camera>(true);
        }
        if (cam == null)
        {
            var cams = FindObjectsOfType<Camera>();
            if (cams.Length > 0) cam = cams[0];
        }

        if (cam != null)
        {
            _mainCamera = cam.gameObject;
            // On s'assure que la caméra a bien le tag pour d'autres systèmes
            if (cam.tag != "MainCamera")
                cam.tag = "MainCamera";
            Debug.Log($"📸 Caméra assignée: {_mainCamera.name}");
        }
    }
}
