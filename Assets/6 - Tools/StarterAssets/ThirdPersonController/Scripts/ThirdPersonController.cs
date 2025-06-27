using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : NetworkBehaviour
{
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

    // Accesseurs publics pour compatibilité avec d'autres scripts
    public bool IsInCombat { get; set; } = false;
    public bool IsClickMoving => _isClickMoving;
    public Vector3 ClickTarget => _clickTarget;
    public bool HasAnimator => _hasAnimator;
    public Animator Animator => _animator;
    public int AnimIDSpeed => _animIDSpeed;
    public int AnimIDMotionSpeed => _animIDMotionSpeed;

    public override void OnStartLocalPlayer()
    {
        Transform camTransform = transform.Find("PlayerCamera");
        if (camTransform != null)
        {
            Camera cam = camTransform.GetComponent<Camera>();
            if (cam != null)
            {
                cam.enabled = true;
                cam.tag = "MainCamera";
                _mainCamera = cam.gameObject;
                Debug.Log("📸 Caméra locale activée pour : " + gameObject.name);
            }
        }

        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _hasAnimator = _animator != null;

        AssignAnimationIDs();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _clickDetected = true;
            Debug.Log("🖱 Clic détecté sur : " + gameObject.name);
        }

        if (_hasAnimator)
            _animator.SetBool("Grounded", true);

        ClickToMove();
    }

    private void ClickToMove()
    {
        if (_clickDetected)
        {
            _clickDetected = false;

            if (_mainCamera == null)
            {
                Debug.LogWarning("❌ Caméra non définie !");
                return;
            }

            Ray ray = _mainCamera.GetComponent<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, ClickableLayers))
            {
                Debug.Log("🎯 Raycast hit: " + hit.point);
                _clickTarget = hit.point;
                _isClickMoving = true;
            }
        }

        if (_isClickMoving)
        {
            Vector3 flatTarget = new Vector3(_clickTarget.x, transform.position.y, _clickTarget.z);
            Vector3 direction = (flatTarget - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, flatTarget);

            if (distance > 0.1f)
            {
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
}
