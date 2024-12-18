using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Envir_Door_bottom : MonoBehaviour
{
    MeshRenderer _meshRenderer;
    public Material _upMaterial;
    public Material _downMaterial;

    public bool _isOpen;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("PlayerPunch") || other.gameObject.CompareTag("Item_brokeGun"))
        {
            //_isOpen = !_isOpen;
            _isOpen = true;
            if (_isOpen)
                _meshRenderer.material = _downMaterial;
            else
                _meshRenderer.material = _upMaterial;
        }
    }
}
