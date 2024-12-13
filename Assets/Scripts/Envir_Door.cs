using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Envir_Door : MonoBehaviour
{
    public Transform _doorTrans;
    public List<Envir_Door_bottom> _bottomList;
    public float _speed;

    bool _isOpen;

    private void LateUpdate()
    {
        //按钮判断
        bool isAllDoorOpem = false;
        foreach (var cs in _bottomList)
        {
            isAllDoorOpem = cs._isOpen;
        }
        if (isAllDoorOpem)
            _isOpen = true;
        //Door移动
        if (_isOpen)
        {
            if (_doorTrans.position.y > -1.1f)
                _doorTrans.position -= new Vector3(0, _speed * Time.deltaTime, 0);
            else
                _doorTrans.position = new Vector3(_doorTrans.position.x, -1.1f, _doorTrans.position.z);
        }
    }
}
