using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class UI_Login : MonoBehaviour
{
    [SerializeField] LoginManager loginManager;
    [SerializeField] TMP_InputField accountInputField;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] TextMeshProUGUI errorHnitText;

    Coroutine LoginCoroutine;

    private void Start()
    {
        accountInputField.text = string.Empty;
        passwordInputField.text = string.Empty;
        errorHnitText.text = string.Empty;
    }

    public void OnClick_Login()
    {
        //防止使用者重複觸發登入流程
        if(LoginCoroutine != null)
        {
            return;
        }

        string account = accountInputField.text.Trim();
        string password = passwordInputField.text.Trim();

        //確認帳號輸入
        if (string.IsNullOrEmpty(account))
        {
            errorHnitText.text = $"帳號不可為空";
            return;
        }

        //確認帳號符合Email格式
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(account, pattern))
        {
            errorHnitText.text = $"帳號不符合Email格式";
            return;
        }

        //確認密碼輸入
        if (string.IsNullOrEmpty(password))
        {
            errorHnitText.text = $"密碼不可為空";
            return;
        }

        errorHnitText.text = string.Empty;

        //開始登入流程
        LoginCoroutine = StartCoroutine(loginManager.LoginCheck(account, password, OnLoginResult));
    }

    //登入流程結束事件觸發
    void OnLoginResult(string resultText)
    {
        errorHnitText.text = resultText;

        LoginCoroutine = null;
    }
}
