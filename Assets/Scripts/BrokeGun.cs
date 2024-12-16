using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokeGun : MonoBehaviour
{
    public ParticleSystem _brokeParticle;
    public BoxCollider _coll;

    public float _lifeTime = 2;
    float _currentLifeTime;

    bool _isPlayParticle;

    private void Update()
    {
        if (!_brokeParticle.IsAlive())
        {
            _currentLifeTime += Time.deltaTime;
        }
        if (_currentLifeTime > _lifeTime)
        {
            _coll.enabled = false;
        }
        if (transform.position.y < -10)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!_isPlayParticle)
        {
            _brokeParticle.Play();
            _isPlayParticle = true;
        }
    }
}
