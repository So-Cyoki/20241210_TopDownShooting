using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Envir_Drum : MonoBehaviour
{
    Animator _animator;
    Rigidbody _rb;
    Collider _coll;
    public Collider _boomColl;
    public ParticleSystem _drumParticle;
    public ParticleSystem _hpDownParticle;
    public ParticleSystem _boomParticle;

    public int _hp;
    [HideInInspector] public int _currentHp;
    public int _attackVaule;
    public float _boomForce;

    bool _isBoom;
    bool _isDead;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<Collider>();
    }
    private void Start()
    {
        _currentHp = _hp;
    }

    private void Update()
    {
        if (!_isBoom && _currentHp <= 0)
        {
            _currentHp = 0;
            _isBoom = true;
            ToBoom();
        }
        if (_isDead)
        {
            if (transform.position.y < -10)
            {
                Destroy(gameObject);
            }
        }
    }

    public void BoomStart(Collider coll)
    {
        Vector3 dir = Vector3.zero;
        switch (coll.gameObject.tag)
        {
            case "Player":
                coll.GetComponent<Player>()._currentHp -= _attackVaule;
                coll.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                dir = (coll.transform.position - transform.position).normalized;
                dir = (dir + Vector3.up).normalized;
                coll.GetComponent<Rigidbody>().AddForce(dir * _boomForce, ForceMode.VelocityChange);
                coll.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 10, 0), ForceMode.VelocityChange);
                break;
            case "Enemy":
                coll.GetComponent<Enemy>()._currentHp -= _attackVaule;
                coll.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                dir = (coll.transform.position - transform.position).normalized;
                dir = (dir + Vector3.up).normalized;
                coll.GetComponent<Rigidbody>().AddForce(dir * _boomForce, ForceMode.VelocityChange);
                coll.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 10, 0), ForceMode.VelocityChange);
                break;
            case "Envir_drum":
                coll.GetComponent<Envir_Drum>()._currentHp -= _attackVaule;
                break;
            case "Envir_cube":
                dir = (coll.transform.position - transform.position).normalized;
                dir = (dir + Vector3.up).normalized;
                coll.GetComponent<Rigidbody>().AddForce(dir * _boomForce, ForceMode.VelocityChange);
                coll.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 10, 0), ForceMode.VelocityChange);
                break;
        }
    }
    public void BoomDamageStart()//动画事件
    {
        _rb.isKinematic = true;
        _coll.enabled = false;
    }
    public void BoomDamageEnd()//动画事件
    {
        _boomColl.enabled = false;
    }

    public void ToBoom()
    {
        _animator.SetTrigger("tBoom");
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        //_boomColl.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        //_boomColl.transform.rotation = Quaternion.LookRotation(_boomColl.transform.forward, Vector3.up);
        _boomParticle.transform.rotation = Quaternion.identity;
        _boomParticle.Play();
    }
    public void ToDead()//动画事件
    {
        _isDead = true;
        _rb.isKinematic = false;
        _coll.enabled = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet") || other.gameObject.CompareTag("Item_brokeGun"))
        {
            //扣血
            _currentHp -= 1;
            //触发被击中粒子
            Vector3 seeDir = new Vector3(other.transform.position.x - transform.position.x, 0, other.transform.position.z - transform.position.z).normalized;
            if (seeDir.sqrMagnitude > 0.001f)
                _drumParticle.transform.rotation = Quaternion.LookRotation(seeDir, Vector3.up);
            _drumParticle.Play();
            //扣了血就触发着火粒子
            if (!_hpDownParticle.isPlaying)
                _hpDownParticle.Play();
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
                _drumParticle.transform.rotation = Quaternion.LookRotation(seeDir, Vector3.up);
            _drumParticle.Play();
            if (!_hpDownParticle.isPlaying)
                _hpDownParticle.Play();
        }
    }
}
