using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameClearCollider : MonoBehaviour
{
    public GameObject _gameClearUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _gameClearUI.SetActive(true);
        }
    }
}
