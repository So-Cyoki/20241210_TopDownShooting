using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject _playerObj;
    public GameObject _gameStartUI;
    public GameObject _gameOverUI;
    public RectTransform _hpMask;
    float _hpMaskRate;

    private void Start()
    {
        _gameStartUI.SetActive(true);
        _gameOverUI.SetActive(false);
        _playerObj.SetActive(false);
        _hpMaskRate = _hpMask.localScale.x;
    }
    private void Update()
    {
        //Game Start UI
        if (_gameStartUI.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _gameStartUI.SetActive(false);
                _playerObj.SetActive(true);
            }
        }
        //血量遮罩效果
        Vector3 maskVector = new(_hpMaskRate, _hpMaskRate, _hpMaskRate);
        _hpMask.localScale = Vector3.Lerp(_hpMask.localScale, maskVector, 0.1f);
    }

    void HPMask(int playerHp, int playerMaxHp)
    {
        float hpRate = (float)playerHp / (float)playerMaxHp;
        _hpMaskRate = Mathf.Lerp(1, 3, hpRate);
        if (hpRate == 1f)
            _hpMaskRate = 50;
    }



    private void OnEnable()
    {
        Player.OnHpChange += HPMask;
    }
    private void OnDisable()
    {

        Player.OnHpChange -= HPMask;
    }
}
