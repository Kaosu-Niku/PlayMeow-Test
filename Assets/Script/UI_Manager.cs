using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    [SerializeField] LoginManager loginManager;

    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject mainPanel;

    private void OnEnable()
    {
        StartCoroutine(wait());
    }

    IEnumerator wait()
    {
        yield return new WaitUntil(() => loginManager != null);
        loginManager.onLoginSuccess += OnLoginSuccess;
    }

    private void OnDisable()
    {
        if(loginManager != null)
        {
            loginManager.onLoginSuccess -= OnLoginSuccess;
        }
    }

    //登入成功時觸發
    void OnLoginSuccess()
    {
        //關閉登入介面，開啟遊戲主介面
        loginPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    //開啟登入介面
    public void OnOpenLogin()
    {
        //關閉遊戲主介面，開啟登入介面
        mainPanel.SetActive(false);
        loginPanel.SetActive(true);        
    }
}
