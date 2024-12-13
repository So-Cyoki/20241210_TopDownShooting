using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Rigidbody _rb;
    Collider _coll;
    public Transform _playerTrans;
    public GameObject _bulletObj;
    public Transform _shootPos;
    public ParticleSystem _bloodParticle;
    public Collider _gunColl;

    public float _speed;
    public int _hp;
    [HideInInspector] public int _currentHp;
    public float _deadTime;
    float _currentDeadTime;
    public int _attackValue;
    public float _attackTime = 0.5f;
    float _currentAttackTime;
    public float _attackLength;
    public float _seePlayerLength;

    bool _isAttack;
    bool _isDead;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<Collider>();
    }
    private void Start()
    {
        _currentHp = _hp;
    }

    private void Update()
    {
        if (!_isDead)
        {
            if (_currentHp <= 0)
            {
                _currentHp = 0;
                _isDead = true;
            }
            Move();
        }
        else
        {
            _gunColl.enabled = false;
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
                    _isAttack = true;
            }
        }
        //攻击状态
        if (_isAttack)
        {
            //旋转
            Vector3 seeDir = new Vector3(_playerTrans.position.x - transform.position.x, 0, _playerTrans.position.z - transform.position.z).normalized;
            if (seeDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(seeDir, Vector3.up);
            //移动
            if (playerToLength > _attackLength * _attackLength)
                _rb.velocity = transform.forward * _speed;
            _currentAttackTime += Time.deltaTime;
            //攻击
            if (_currentAttackTime >= _attackTime)
            {
                _currentAttackTime = 0;
                Instantiate(_bulletObj, _shootPos.position, Quaternion.LookRotation(seeDir, Vector3.up));
            }
        }
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
