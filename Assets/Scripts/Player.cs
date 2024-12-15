using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    public enum PlayerState
    {
        PUNCH, HANDGUN,
    }

    Rigidbody _rb;
    Animator _animator;
    public GameObject _gunObj;
    public GameObject _bulletObj;
    public Transform _shootTrans;
    public ParticleSystem _bloodParticle;
    public List<GameObject> _brokeGunPrefabs;
    public Animator _uiNoBullet_animator;

    [Header("摄像机")]
    [Tooltip("想要摄像机在两个向量的哪一部分")]
    [Range(0, 1)] public float _mouseCameraLerp;
    public Vector3 _cameraPosOffset;
    public float _mouseCameraLerpMaxLength;
    public float _cameraMoveTime;
    Vector3 _cameraVeloctiy;
    [Header("基础属性")]
    [SerializeField] PlayerState _playerState = PlayerState.PUNCH;
    public int _hp;
    public int _currentHp;
    public float _speed;
    public float _dropGunForce;
    Vector3 _mousePos;
    [Header("枪械属性")]
    public float _attackTime = 0.5f;
    float _currentAttackTime;
    float _punchTime = 0.18f;
    float _currentPunchTime = 0;
    public int _current_bulletNum = 0;
    public int _handGun_bulletNum;

    bool _isPunchTime = false;
    bool _isHandGunAttack = true;
    bool _isPunchR;
    [HideInInspector] public bool _isPunchAttack;
    [HideInInspector] public bool _isDead;

    public static event Action<int, int> OnHpChange;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }
    private void Start()
    {
        _gunObj.SetActive(false);
        _currentAttackTime = 0;
        _currentHp = _hp;
    }
    private void FixedUpdate()
    {
        if (!_isDead)
            Move();
        else
        {
            _rb.constraints = RigidbodyConstraints.None;
        }
    }
    private void Update()
    {
        //死亡检查
        if (!_isDead && _currentHp <= 0)
        {
            _currentHp = 0;
            OnHpChange?.Invoke(_currentHp, _hp);
            _isDead = true;
        }
        //获取鼠标位置
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mouseRay, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Plane")))
        {
            _mousePos = hit.point;
        }
        //攻击
        if (!_isDead)
        {
            switch (_playerState)
            {
                case PlayerState.PUNCH:
                    PunchAttack();
                    break;
                case PlayerState.HANDGUN:
                    GunAttack();
                    break;
            }
        }
        //丢枪
        DropGunAttack();
    }
    private void LateUpdate()
    {
        CameraMove();
    }
    void CameraMove()
    {
        Vector3 mousePlayerPos = _mousePos - transform.position;
        if (mousePlayerPos.sqrMagnitude > _mouseCameraLerpMaxLength * _mouseCameraLerpMaxLength)
        {
            mousePlayerPos = mousePlayerPos.normalized * _mouseCameraLerpMaxLength;
        }
        // 将限制后的鼠标位置加回到玩家位置
        Vector3 clampedMousePos = transform.position + mousePlayerPos;
        Vector3 targetPos = Vector3.Lerp(transform.position, clampedMousePos, _mouseCameraLerp);
        targetPos.y = 0;
        //Camera.main.transform.position = targetPos + _cameraPosOffset;
        Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, targetPos + _cameraPosOffset, ref _cameraVeloctiy, _cameraMoveTime);
    }
    void Move()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        Vector3 moveDir = new Vector3(horizontal, 0, vertical).normalized;
        //旋转
        Vector3 seeDir = new Vector3(_mousePos.x - transform.position.x, 0, _mousePos.z - transform.position.z).normalized;
        if (seeDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(seeDir, Vector3.up);
        //移动
        _rb.velocity = new(moveDir.x * _speed, _rb.velocity.y, moveDir.z * _speed);
    }
    void GunAttack()
    {
        if (_current_bulletNum > 0)
        {
            //手枪
            if (Input.GetMouseButtonDown(0) && _isHandGunAttack)
            {
                _currentAttackTime = _attackTime;
                Vector3 seeDir = new Vector3(_mousePos.x - transform.position.x, 0, _mousePos.z - transform.position.z).normalized;
                GameObject bullet = Instantiate(_bulletObj, _shootTrans.position, Quaternion.LookRotation(seeDir, Vector3.up));
                _isHandGunAttack = false;
                _animator.SetTrigger("tGunAttack");
                _current_bulletNum--;
            }
            if (!_isHandGunAttack)
            {
                _currentAttackTime += Time.deltaTime;
                if (_currentAttackTime > _attackTime)
                {
                    _currentAttackTime = 0;
                    _isHandGunAttack = true;
                }
            }
        }
        else
        {
            //没子弹
            _uiNoBullet_animator.transform.position = Camera.main.WorldToScreenPoint(transform.position);
            if (Input.GetMouseButtonDown(0))
            {
                _uiNoBullet_animator.SetTrigger("tMessage");
            }
        }
    }
    void DropGunAttack()
    {
        if (_playerState != PlayerState.PUNCH && Input.GetMouseButtonDown(1))
        {
            _gunObj.SetActive(false);
            int gunNum = 0;
            switch (_playerState)
            {
                case PlayerState.HANDGUN:
                    gunNum = 0;
                    _animator.SetBool("isHandGun", false);
                    break;
            }
            GameObject gunObj = Instantiate(_brokeGunPrefabs[gunNum], _shootTrans.position, Quaternion.Euler(0, 0, 90));
            Rigidbody gunRb = gunObj.GetComponent<Rigidbody>();
            gunRb.AddForce(transform.forward * _dropGunForce, ForceMode.Impulse);
            gunRb.AddTorque(new Vector3(0, 3, 0), ForceMode.Impulse);
            //gunObj.GetComponent<Animator>().enabled = false;
            _playerState = PlayerState.PUNCH;
        }
    }
    void PunchAttack()
    {
        if (_isPunchTime)
        {
            _currentPunchTime += Time.deltaTime;
            if (_currentPunchTime > _punchTime)
            {
                _currentPunchTime = 0;
                _isPunchTime = false;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!_isPunchR)
                    _animator.SetTrigger("tPunchR");
                else
                    _animator.SetTrigger("tPunchL");
                _isPunchR = !_isPunchR;
                _isPunchTime = true;
            }
        }
    }
    public void OnPunchAttack(int flag)
    {
        _isPunchAttack = flag != 0;
    }
    void EnemyDead()
    {
        _currentHp += 2;
        if (_currentHp > _hp)
            _currentHp = _hp;
        OnHpChange?.Invoke(_currentHp, _hp);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (
            _playerState == PlayerState.PUNCH && other.gameObject.CompareTag("Item_gun"))
        {
            Destroy(other.gameObject);
            _current_bulletNum = _handGun_bulletNum;
            _playerState = PlayerState.HANDGUN;
            _animator.SetBool("isHandGun", true);
            _gunObj.SetActive(true);
        }
        if (other.gameObject.CompareTag("Bullet"))
        {
            _currentHp -= 1;
            OnHpChange?.Invoke(_currentHp, _hp);
            Vector3 seeDir = new Vector3(other.transform.position.x - transform.position.x, 0, other.transform.position.z - transform.position.z).normalized;
            if (seeDir.sqrMagnitude > 0.001f)
                _bloodParticle.transform.rotation = Quaternion.LookRotation(seeDir, Vector3.up);
            _bloodParticle.Play();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyPunch"))
        {
            _currentHp -= 1;
            OnHpChange?.Invoke(_currentHp, _hp);
            Vector3 seeDir = new Vector3(other.transform.position.x - transform.position.x, 0, other.transform.position.z - transform.position.z).normalized;
            if (seeDir.sqrMagnitude > 0.001f)
                _bloodParticle.transform.rotation = Quaternion.LookRotation(seeDir, Vector3.up);
            _bloodParticle.Play();
        }
    }

    private void OnEnable()
    {
        Enemy.OnEnemyDead += EnemyDead;
    }
    private void OnDisable()
    {

        Enemy.OnEnemyDead -= EnemyDead;
    }
}
