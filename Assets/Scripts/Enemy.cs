using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    public enum EnemyState
    {
        PUNCH, HANDGUN, SHOTGUN, SUBGUN
    }
    Rigidbody _rb;
    Collider _coll;

    public Transform _playerTrans;
    public EnemyState _enemyState = EnemyState.HANDGUN;
    public bool _isDrawRay;
    public GameObject _bulletObj;
    public Transform _shootTrans;
    public ParticleSystem _bloodParticle;
    public List<Collider> _gunColls;
    public Collider _handLColl;
    public Collider _handRColl;
    Animator _animator;
    [Header("基础属性")]
    public float _speed;
    public float _noGunSpeed;
    public float _rotationSpeed;
    public int _hp;
    [HideInInspector] public int _currentHp;
    public float _deadTime;
    float _currentDeadTime;
    // public float _punchAttackTime = 0.5f;
    // float _currentPunchAttackTime;
    public float _seePlayerLength;
    public float _gunLength;
    public float _punchLength;
    [Header("枪械属性")]
    public List<GameObject> _gunPrefabs;
    float _gunAttackTime;
    float _currentGunAttackTime;
    public float _handGun_attackTime = 0.5f;
    public float _shotGun_attackTime = 0.5f;
    public float _subGun_attackTime = 0.5f;
    public float _dropGunForce;

    bool _isAttack;
    [HideInInspector] public bool _isDead;

    public static event Action OnEnemyDead;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
    }
    private void Start()
    {
        _currentHp = _hp;
        foreach (var coll in _gunColls)
            coll.gameObject.SetActive(false);
        switch (_enemyState)
        {
            case EnemyState.PUNCH:
                _animator.SetTrigger("tHand");
                break;
            case EnemyState.HANDGUN:
                _gunColls[0].gameObject.SetActive(true);
                _gunAttackTime = _handGun_attackTime;
                break;
            case EnemyState.SHOTGUN:
                _gunColls[1].gameObject.SetActive(true);
                _gunAttackTime = _shotGun_attackTime;
                break;
            case EnemyState.SUBGUN:
                _gunColls[2].gameObject.SetActive(true);
                _gunAttackTime = _subGun_attackTime;
                break;
        }
    }
    private void FixedUpdate()
    {
        _rb.AddForce(Vector3.down * 20, ForceMode.Acceleration);//总是会跳起来，给一个向下的力量

        if (!_isDead)
            Move();
    }
    private void Update()
    {
        if (!_isDead)
        {
            if (_currentHp <= 0)
            {
                _currentHp = 0;
                foreach (var coll in _gunColls)
                    coll.gameObject.SetActive(false);
                _rb.AddTorque(transform.right * 3, ForceMode.VelocityChange);
                switch (_enemyState)
                {
                    case EnemyState.HANDGUN:
                        DropGun(0);
                        break;
                    case EnemyState.SHOTGUN:
                        DropGun(1);
                        break;
                    case EnemyState.SUBGUN:
                        DropGun(2);
                        break;
                }
                OnEnemyDead?.Invoke();
                _isDead = true;
            }
        }
        else
        {
            //_gunColl.enabled = false;
            _handLColl.enabled = false;
            _handRColl.enabled = false;
            _rb.constraints = RigidbodyConstraints.None;
            _currentDeadTime += Time.deltaTime;
            if (_currentDeadTime > _deadTime)
            {
                _currentDeadTime = 0;
                _coll.enabled = false;
            }
        }
        if (transform.position.y < -10)
            Destroy(gameObject);
    }

    void Move()
    {
        //搜索玩家
        float playerToLength = (_playerTrans.position - transform.position).sqrMagnitude;
        if (playerToLength < _seePlayerLength * _seePlayerLength)
        {
            int layerMask = ~(1 << LayerMask.NameToLayer("Character") | 1 << LayerMask.NameToLayer("Item") | 1 << LayerMask.NameToLayer("Punch"));
            Ray ray = new(transform.position, (_playerTrans.position - transform.position).normalized);
            if (_isDrawRay)
                Debug.DrawRay(transform.position, (_playerTrans.position - transform.position).normalized * _seePlayerLength, Color.red, 0.5f);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, _seePlayerLength, layerMask))
            {
                if (hitInfo.collider.CompareTag("Player") || hitInfo.collider.CompareTag("PlayerPunch"))
                {
                    _animator.SetTrigger("tAttack");
                    _isAttack = true;
                }
            }
        }
        //攻击状态
        if (_isAttack)
        {
            //旋转
            Vector3 seeDir = new Vector3(_playerTrans.position.x - transform.position.x, 0, _playerTrans.position.z - transform.position.z).normalized;
            if (seeDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(seeDir, Vector3.up);
                if (Vector3.Angle(transform.forward, seeDir) > 1.0f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
            }
            //持枪
            if (_enemyState != EnemyState.PUNCH)
            {
                if (playerToLength > _gunLength * _gunLength)//移动
                    _rb.velocity = transform.forward * _speed;
                _currentGunAttackTime += Time.deltaTime;
                if (_currentGunAttackTime >= _gunAttackTime)//攻击
                {
                    //查看Player在不在眼前
                    int layerMask = ~(1 << LayerMask.NameToLayer("Character") | 1 << LayerMask.NameToLayer("Punch"));
                    Ray ray = new(transform.position, (_playerTrans.position - transform.position).normalized);
                    if (Physics.Raycast(ray, out RaycastHit hitInfo, _seePlayerLength, layerMask))
                    {
                        if (hitInfo.collider.CompareTag("Player") || hitInfo.collider.CompareTag("PlayerPunch"))
                        {
                            switch (_enemyState)
                            {
                                case EnemyState.HANDGUN:
                                case EnemyState.SUBGUN:
                                    Instantiate(_bulletObj, _shootTrans.position, Quaternion.LookRotation(transform.forward, Vector3.up));
                                    break;
                                case EnemyState.SHOTGUN:
                                    Instantiate(_bulletObj, _shootTrans.position, Quaternion.LookRotation(seeDir, Vector3.up));
                                    Vector3 shotDir = Quaternion.Euler(0, -10, 0) * seeDir;
                                    Vector3 shotPos = _shootTrans.right * -0.4f + _shootTrans.position;
                                    Instantiate(_bulletObj, shotPos, Quaternion.LookRotation(shotDir, Vector3.up));
                                    shotDir = Quaternion.Euler(0, 10, 0) * seeDir;
                                    shotPos = _shootTrans.right * 0.4f + _shootTrans.position;
                                    Instantiate(_bulletObj, shotPos, Quaternion.LookRotation(shotDir, Vector3.up));
                                    shotDir = Quaternion.Euler(0, -20, 0) * seeDir;
                                    shotPos = _shootTrans.right * -0.8f + _shootTrans.position;
                                    Instantiate(_bulletObj, shotPos, Quaternion.LookRotation(shotDir, Vector3.up));
                                    shotDir = Quaternion.Euler(0, 20, 0) * seeDir;
                                    shotPos = _shootTrans.right * 0.8f + _shootTrans.position;
                                    Instantiate(_bulletObj, shotPos, Quaternion.LookRotation(shotDir, Vector3.up));
                                    break;
                            }
                            _currentGunAttackTime = 0;
                            _animator.SetTrigger("tGunAttack");
                        }
                    }
                }
            }
            //空手
            else
            {
                if (playerToLength > _punchLength * _punchLength)//移动
                    _rb.velocity = transform.forward * _noGunSpeed;
                _currentGunAttackTime += Time.deltaTime;
                if (_currentGunAttackTime >= _gunAttackTime)//攻击
                {
                    _currentGunAttackTime = 0;
                    _animator.SetTrigger("tPunch");
                }
            }
        }
    }
    void DropGun(int gunNum)
    {
        //丢出枪械
        GameObject gunObj = Instantiate(_gunPrefabs[gunNum], _shootTrans.position, Quaternion.Euler(0, 0, 90));
        Rigidbody gunRb = gunObj.GetComponent<Rigidbody>();
        Vector3 dropDir = Quaternion.Euler(-60, 0, 0) * transform.forward;
        gunRb.AddForce(dropDir * _dropGunForce, ForceMode.Impulse);
        gunRb.AddTorque(new Vector3(0, 3, 0), ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            _currentHp -= 1;
            if (!_isAttack)
            {
                _animator.SetTrigger("tAttack");
                _isAttack = true;
            }
            Vector3 seeDir = new Vector3(other.transform.position.x - transform.position.x, 0, other.transform.position.z - transform.position.z).normalized;
            if (seeDir.sqrMagnitude > 0.001f)
                _bloodParticle.transform.rotation = Quaternion.LookRotation(seeDir, Vector3.up);
            _bloodParticle.Play();
        }
        if (_enemyState != EnemyState.PUNCH && !_isDead && other.gameObject.CompareTag("Item_brokeGun"))
        {
            //丢出枪械
            switch (_enemyState)
            {
                case EnemyState.HANDGUN:
                    DropGun(0);
                    break;
                case EnemyState.SHOTGUN:
                    DropGun(1);
                    break;
                case EnemyState.SUBGUN:
                    DropGun(2);
                    break;
            }
            if (!_isAttack)
                _isAttack = true;
            _enemyState = EnemyState.PUNCH;
            _animator.SetTrigger("tHand");
            foreach (var coll in _gunColls)
                coll.gameObject.SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerPunch"))
        {
            if (!other.GetComponent<PlayerPunch>()._isPunchAttack)
                return;
            _currentHp -= 1;
            Vector3 seeDir = new Vector3(other.transform.position.x - transform.position.x, 0, other.transform.position.z - transform.position.z).normalized;
            if (seeDir.sqrMagnitude > 0.001f)
                _bloodParticle.transform.rotation = Quaternion.LookRotation(seeDir, Vector3.up);
            _bloodParticle.Play();
        }
    }
}
