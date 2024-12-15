using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    Rigidbody _rb;
    Collider _coll;
    public Transform _playerTrans;
    public GameObject _bulletObj;
    public Transform _shootTrans;
    public ParticleSystem _bloodParticle;
    public Collider _gunColl;
    public Collider _handLColl;
    public Collider _handRColl;
    Animator _animator;
    [Header("基础属性")]
    public float _speed;
    public float _rotationSpeed;
    public int _hp;
    [HideInInspector] public int _currentHp;
    public float _deadTime;
    float _currentDeadTime;
    public float _gunAttackTime = 0.5f;
    float _currentGunAttackTime;
    // public float _punchAttackTime = 0.5f;
    // float _currentPunchAttackTime;
    public float _seePlayerLength;
    public float _gunLength;
    public float _punchLength;
    public List<GameObject> _gunPrefabs;
    public float _dropGunForce;

    bool _isAttack;
    bool _isGun = true;
    bool _isDead;

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
    }
    private void FixedUpdate()
    {
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
                _gunColl.gameObject.SetActive(false);
                if (_isGun)
                    DropGun(0);
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
            Ray ray = new(transform.position, (_playerTrans.position - transform.position).normalized);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                if (hitInfo.collider.CompareTag("Player"))
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
            if (_isGun)
            {
                if (playerToLength > _gunLength * _gunLength)//移动
                    _rb.velocity = transform.forward * _speed;
                _currentGunAttackTime += Time.deltaTime;
                if (_currentGunAttackTime >= _gunAttackTime)//攻击
                {
                    _currentGunAttackTime = 0;
                    Instantiate(_bulletObj, _shootTrans.position, Quaternion.LookRotation(transform.forward, Vector3.up));
                    _animator.SetTrigger("tGunAttack");
                }
            }
            //空手
            else
            {
                if (playerToLength > _punchLength * _punchLength)//移动
                    _rb.velocity = transform.forward * _speed;
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
        gunRb.AddForce(transform.forward * _dropGunForce, ForceMode.Impulse);
        gunRb.AddTorque(new Vector3(0, 3, 0), ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            _currentHp -= 1;
            Vector3 seeDir = new Vector3(other.transform.position.x - transform.position.x, 0, other.transform.position.z - transform.position.z).normalized;
            if (seeDir.sqrMagnitude > 0.001f)
                _bloodParticle.transform.rotation = Quaternion.LookRotation(seeDir, Vector3.up);
            _bloodParticle.Play();
        }
        if (_isGun && !_isDead && other.gameObject.CompareTag("Item_brokeGun"))
        {
            _isGun = false;
            _gunColl.gameObject.SetActive(false);
            _animator.SetTrigger("tHand");
            //丢出枪械
            DropGun(0);
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
