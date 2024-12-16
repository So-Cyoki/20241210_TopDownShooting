using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Help_noBulletCollider : MonoBehaviour
{
    public GameObject _gunPrefab;
    public Transform _bornTrans;
    public float _bornTime = 1;
    float _currentBornTime;
    Player _playerCS;
    GameObject _bornGunPrefab;
    private void Start()
    {
        //_currentBornTime = _bornTime;
    }
    private void LateUpdate()
    {
        // if (_playerCS != null)
        // {
        //     if (_playerCS._playerState == Player.PlayerState.PUNCH
        //     && _bornGunPrefab == null)
        //     {
        //         _currentBornTime += Time.deltaTime;
        //         if (_currentBornTime > _bornTime)
        //         {
        //             _currentBornTime = 0;
        //             _bornGunPrefab = Instantiate(_gunPrefab, _bornTrans.position, Quaternion.Euler(0, 0, 90));
        //         }
        //     }
        // }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerCS = other.GetComponent<Player>();
            _playerCS._current_bulletNum = 0;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        _playerCS = null;
        _bornGunPrefab = null;
    }
}
