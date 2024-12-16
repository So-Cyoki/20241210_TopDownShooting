using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject _playerObj;
    public GameObject _gameStartUI;
    public TextMeshProUGUI _gameOver_enemyNum;
    public TextMeshProUGUI _gameClear_enemyNum;
    public RectTransform _hpMask;
    public GameObject _help3DUI;
    Player _playerCS;
    float _hpMaskRate;
    int _enemyDeadNum;

    private void Start()
    {
        _gameStartUI.SetActive(true);
        _gameOver_enemyNum.text = "0";
        _gameOver_enemyNum.transform.parent.gameObject.SetActive(false);
        _gameClear_enemyNum.text = "0";
        _gameClear_enemyNum.transform.parent.gameObject.SetActive(false);
        _playerObj.SetActive(false);
        _hpMaskRate = _hpMask.localScale.x;
        _playerCS = _playerObj.GetComponent<Player>();
        _help3DUI.SetActive(false);
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
        else
        {
            _help3DUI.SetActive(true);
        }
        //Game Over
        if (_playerCS._isDead)
        {
            _hpMask.gameObject.SetActive(false);
            _gameOver_enemyNum.transform.parent.gameObject.SetActive(true);
        }
        if (_gameOver_enemyNum.transform.parent.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                string currentSceneName = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene(currentSceneName);
                return;
            }
        }
        //Game Clear
        if (_gameClear_enemyNum.transform.parent.gameObject.activeSelf)
        {
            _hpMask.gameObject.SetActive(false);
            _playerCS.gameObject.SetActive(false);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                string currentSceneName = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene(currentSceneName);
                return;
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
    void EnemyDeadNumAdd()
    {
        _enemyDeadNum++;
        _gameOver_enemyNum.text = _enemyDeadNum + "";
        _gameClear_enemyNum.text = _enemyDeadNum + "";
    }



    private void OnEnable()
    {
        Player.OnHpChange += HPMask;
        Enemy.OnEnemyDead += EnemyDeadNumAdd;
    }
    private void OnDisable()
    {

        Player.OnHpChange -= HPMask;
        Enemy.OnEnemyDead -= EnemyDeadNumAdd;
    }
}
