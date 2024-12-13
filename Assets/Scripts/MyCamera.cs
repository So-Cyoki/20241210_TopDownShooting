using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCamera : MonoBehaviour
{
    public Transform _playerTrans;

    public Vector3 _offsetPos;

    private void LateUpdate()
    {
        Vector3 cameraPos = _playerTrans.position + _offsetPos;
        transform.position = cameraPos;
    }
}
