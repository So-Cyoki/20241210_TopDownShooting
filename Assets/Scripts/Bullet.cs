using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Rigidbody _rb;
    Collider _coll;
    public ParticleSystem _boomParticle;

    public float _speed;
    public float _attackVoule;
    public float _lifeTime = 10;
    public float _bounceMineTime;

    float _currentLifeTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<Collider>();
    }
    private void FixedUpdate()
    {
        _rb.position += transform.forward * _speed;
    }
    private void LateUpdate()
    {
        //LifeTime
        _currentLifeTime += Time.deltaTime;
        if (_currentLifeTime > _lifeTime)
        {
            _currentLifeTime = 0;
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        //碰到Player和敌人就不要反弹了
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Item_brokeGun"))
        {
            Destroy(gameObject);
            return;
        }
        //反弹转向机制
        Vector3 normal = other.GetContact(0).normal;//获取碰撞点位置的法线
        Vector3 reflectDir = Vector3.Reflect(transform.forward, normal);//计算反射角度
        reflectDir.y = 0;
        transform.rotation = Quaternion.LookRotation(reflectDir.normalized); // 调整子弹朝向
        //反弹后的变化
        _boomParticle.Play();
        _currentLifeTime += _bounceMineTime;
    }
}
