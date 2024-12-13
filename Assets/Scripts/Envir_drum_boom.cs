using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Envir_drum_boom : MonoBehaviour
{
    public Envir_Drum _mainCS;
    private void OnTriggerEnter(Collider other)
    {
        //判断是否撞墙
        Ray ray = new(transform.position, (other.transform.position - transform.position).normalized);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            if (hitInfo.collider == other)
            {
                _mainCS.BoomStart(other);
            }
        }
    }
}
