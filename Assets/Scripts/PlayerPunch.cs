using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPunch : MonoBehaviour
{
    public Player _main;
    public bool _isPunchAttack;
    private void Update()
    {
        if (_isPunchAttack != _main._isPunchAttack)
            _isPunchAttack = _main._isPunchAttack;
    }
}
