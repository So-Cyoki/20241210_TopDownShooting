using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Help_noBulletCollider : MonoBehaviour
{
    public GameObject _gunPrefab;
    public Transform _bornTrans;
    Player _playerCS;

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
    }
}
