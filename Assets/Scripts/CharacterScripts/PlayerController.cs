using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// サードパーソン視点のプレイヤー移動・カメラ制御
/// アニメーション変数: Speed (float), isGrounded (bool), isJumping (bool)
/// カメラ制御はマウスのみ対応
/// Input System のデフォルト PlayerInput コンポーネント経由で入力を受け取る
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // ─── Movement ────────────────────────────────────────
    [Header("Movement")]
    [Tooltip("通常移動速度 (m/s)")]
    public float MoveSpeed = 2.0f;

    [Tooltip("スプリント移動速度 (m/s)")]
    public float SprintSpeed = 5.335f;

    [Tooltip("加速・減速の速度")]
    public float SpeedChangeRate = 10.0f;

    [Tooltip("キャラクターの回転スムース時間")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    // ─── Jump & Gravity ──────────────────────────────────
    [Header("Jump & Gravity")]
    [Tooltip("ジャンプの高さ")]
    public float JumpHeight = 1.0f;
    private float DefaultJumpHeight = 1.0f;

    [Tooltip("カスタム重力値 (デフォルト: -9.81f)")]
    public float Gravity = -9.81f;

    [Tooltip("次のジャンプまでの待機時間")]
    public float JumpTimeout = 0.50f;

    [Tooltip("落下アニメーション開始までの待機時間（下り階梯対策）")]
    public float FallTimeout = 0.15f;

    private float _terminalVelocity = 53.0f;

    // ─── Grounded Check ──────────────────────────────────
    [Header("Grounded Check")]
    [Tooltip("接地チェックのオフセット")]
    public float GroundedOffset = -0.14f;

    [Tooltip("接地チェックの半径（CharacterControllerの半径に合わせる）")]
    public float GroundedRadius = 0.28f;

    [Tooltip("地面として扱うレイヤー")]
    public LayerMask GroundLayers;

    // ─── Camera ──────────────────────────────────────────
    [Header("Camera")]
    [Tooltip("Cinemachine カメラが追随するターゲット オブジェクト")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("カメラの上方向の最大角度")]
    public float TopClamp = 70.0f;

    [Tooltip("カメラの下方向の最大角度")]
    public float BottomClamp = -30.0f;

    [Tooltip("マウスの感度（X軸）")]
    public float MouseSensitivityX = 0.5f;

    [Tooltip("マウスの感度（Y軸）")]
    public float MouseSensitivityY = 0.5f;

    // ─── Private Fields ──────────────────────────────────
    private CharacterController _controller;
    private PlayerInput _playerInput;
    private Animator _animator;
    private GameObject _mainCamera;
    private bool _hasAnimator;

    // Input 値（イベントから書き込み、Update で読み取る）
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _jumpPressed;
    private bool _sprintPressed;

    // Movement
    private float _speed;
    private float _animationBlend;
    private float _targetRotation;
    private float _rotationVelocity;

    // Vertical
    private float _verticalVelocity;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // Camera
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // Grounded
    private bool _grounded = true;

    // ─── Animation IDs ───────────────────────────────────
    private int _animIDSpeed;
    private int _animIDIsGrounded;
    private int _animIDIsJumping;

    // ─── Unity Lifecycle ─────────────────────────────────

    private void Awake()
    {
        if (_mainCamera == null)
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    private void Start()
    {
        // コンポーネントの取得
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
        _hasAnimator = TryGetComponent(out _animator);

        // Input System イベント登録
        _playerInput.actions["Move"].performed   += OnMove;
        _playerInput.actions["Move"].canceled    += OnMove;
        _playerInput.actions["Look"].performed   += OnLook;
        _playerInput.actions["Look"].canceled    += OnLook;
        _playerInput.actions["Jump"].started     += OnJump;
        _playerInput.actions["Sprint"].started   += OnSprint;
        _playerInput.actions["Sprint"].canceled  += OnSprint;

        // カーソルを非表示にして中央に固定
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // カメラターゲットの初期回転を取得
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        // アニメーション変数のハッシュを事前計算
        _animIDSpeed       = Animator.StringToHash("Speed");
        _animIDIsGrounded  = Animator.StringToHash("isGrounded");
        _animIDIsJumping   = Animator.StringToHash("isJumping");

        // タイムアウトの初期化
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        GroundedCheck();
        JumpAndGravity();
        Move();
    }

    private void LateUpdate()
    {
        // カメラ更新は LateUpdate で行い、移動後の位置に対応
        // playerはキー入力で回転するが、無理やり最後に子オブジェクトのcamerarootを独立で回転させる
        CameraRotation();
    }

    // ─── Input System Callbacks (Events モード) ─────────

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        _jumpPressed = true;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        _sprintPressed = context.ReadValue<float>() > 0.5f;
    }

    // ─── Grounded Check ──────────────────────────────────

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(
            transform.position.x,
            transform.position.y - GroundedOffset,
            transform.position.z
        );

        _grounded = Physics.CheckSphere(
            spherePosition,
            GroundedRadius,
            GroundLayers,
            QueryTriggerInteraction.Ignore
        );

        if (_hasAnimator)
            _animator.SetBool(_animIDIsGrounded, _grounded);
    }

    // ─── Movement ────────────────────────────────────────

    private void Move()
    {
        // イベントから書き込まれた入力値を読む
        Vector2 moveInput = _moveInput;
        bool    sprint    = _sprintPressed;

        // ターゲット速度の決定
        float targetSpeed = sprint ? SprintSpeed : MoveSpeed;
        if (moveInput == Vector2.zero) targetSpeed = 0.0f;

        // 現在の水平速度
        float currentHorizontalSpeed = new Vector3(
            _controller.velocity.x, 0.0f, _controller.velocity.z
        ).magnitude;

        float speedOffset   = 0.1f;
        float inputMagnitude = moveInput.magnitude; // アナログ入力対応

        // 加速・減速（Lerp による有機的な変化）
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(
                currentHorizontalSpeed,
                targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate
            );
            _speed = Mathf.Round(_speed * 1000f) / 1000f; // 浮動小数点誤差対策
        }
        else
        {
            _speed = targetSpeed;
        }

        // アニメーションブレンド値の更新
        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // 入力方向の正規化
        Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

        // 移動入力がある場合はキャラクターを回転させる
        if (moveInput != Vector2.zero)
        {
            // カメラの向きに相対で回転ターゲットを計算
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg
                            + _mainCamera.transform.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                _targetRotation,
                ref _rotationVelocity,
                RotationSmoothTime
            );

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        // 移動方向の算出（キャラクターの向きに基づく）
        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // CharacterController で移動を適用（水平 + 垂直）
        _controller.Move(
            targetDirection.normalized * (_speed * Time.deltaTime) +
            new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
        );

        // アニメーション変数の更新
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
        }
    }

    // ─── Jump & Gravity ──────────────────────────────────

    private void JumpAndGravity()
    {
        if (_grounded)
        {
            // フォールタイムアウトリセット
            _fallTimeoutDelta = FallTimeout;

            // アニメーション: ジャンプリセット
            if (_hasAnimator)
                _animator.SetBool(_animIDIsJumping, false);

            // 接地中は負の速度を微小値に抑える
            if (_verticalVelocity < 0.0f)
                _verticalVelocity = -2f;

            // ジャンプ入力の処理
            if (_jumpPressed && _jumpTimeoutDelta <= 0.0f)
            {
                // 目標高さに到達するために必要な初速を計算: v = sqrt(H * -2 * G)
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                if (_hasAnimator)
                    _animator.SetBool(_animIDIsJumping, true);
            }

            // フラグを読んだのでリセット
            _jumpPressed = false;

            // ジャンプタイムアウトのカウントダウン
            if (_jumpTimeoutDelta >= 0.0f)
                _jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            // ジャンプタイムアウトリセット
            _jumpTimeoutDelta = JumpTimeout;

            // フォールタイムアウトのカウントダウン（アニメーション遅延対策）
            if (_fallTimeoutDelta >= 0.0f)
                _fallTimeoutDelta -= Time.deltaTime;

            // 空中ではジャンプ入力を無視してリセット
            _jumpPressed = false;
        }

        // 重力の適用（ターミナルベロシティ以下の場合のみ）
        if (_verticalVelocity < _terminalVelocity)
            _verticalVelocity += Gravity * Time.deltaTime;
    }

    // ─── Camera Rotation (Mouse Only) ────────────────────

    private void CameraRotation()
    {
        // イベントから書き込まれたマウス移動量を読む（Time.deltaTime を掛けない）
        float mouseX = _lookInput.x * MouseSensitivityX;
        float mouseY = _lookInput.y * MouseSensitivityY;

        _cinemachineTargetYaw   += mouseX;
        _cinemachineTargetPitch -= mouseY;

        // Yaw は 360 度で正規化
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);

        // Pitch は上下角度でクランプ
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine のターゲットの回転を更新
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
            _cinemachineTargetPitch,
            _cinemachineTargetYaw,
            0.0f
        );
    }

    // ─── Utility ─────────────────────────────────────────

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle >  360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    // ─── Gizmo ───────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _grounded
            ? new Color(0.0f, 1.0f, 0.0f, 0.35f)
            : new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius
        );
    }

    // 収集アイテムとの衝突判定
    private void OnTriggerEnter(Collider other)
    {
        CollectibleItem collectible = other.GetComponent<CollectibleItem>();
        if(collectible != null)
        {
            collectible.OnCollect(gameObject);
        }
    }

// 非同期処理で5秒間ジャンプ力を増加させる
    public void IncreaseJumpPower(float jumpPower)
    {
        JumpHeight = jumpPower;
        Debug.Log("ジャンプ力が" + jumpPower + "になった。");
        // 5秒後に元のジャンプ力に戻す
        Invoke("ResetJumpPower", 5f);

    }

    private void ResetJumpPower()
    {
        JumpHeight = DefaultJumpHeight;
        Debug.Log("ジャンプ力が元に戻った。");
    }

    // ダメージオブジェクトとの衝突判定
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        DamageBlock damageBlock = hit.gameObject.GetComponent<DamageBlock>();
        if(damageBlock != null)
        {
            PlayerHealth playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageBlock.DamageAmount);
            }
        }
    }
    
}