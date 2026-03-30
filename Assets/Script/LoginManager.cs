using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking;

public class LoginManager : MonoBehaviour
{
    public Action onLoginSuccess; //登入成功事件

    private void Start()
    {
        //開啟遊戲時，嘗試從PlayerPrefs讀取Token值
        string token = PlayerPrefs.GetString("token", "");
        Debug.Log($"讀取Token: {token}");

        if (string.IsNullOrEmpty(token))
        {
            return;
        }

        //如有Token，便進行Token驗證流程
        StartCoroutine(QueryMe(token));
    }

    //GraphQL request標準格式
    [System.Serializable]
    public class GraphQLRequest
    {
        public string query;

        public GraphQLRequest(string query)
        {
            this.query = query;
        }
    }

    [System.Serializable]
    public class LoginResponse
    {
        public Data data;
    }

    [System.Serializable]
    public class Data
    {
        public Login login;
    }

    [System.Serializable]
    public class Login
    {
        public string token;
    }

    //登入流程
    public IEnumerator LoginCheck(string account, string password, System.Action<string> onLoginResult)
    {
        yield return null;

        //GraphQL query
        //(這是根據提供的GraphQL API網址，經查詢後得出的請求格式)
        string query = $@"
        mutation {{
            login(username: ""{account}"", password: ""{password}"") {{
                token
            }}
        }}";

        //包成JSON格式
        string json = JsonUtility.ToJson(new GraphQLRequest(query));
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        //建立GraphQL request
        string url = "https://interview-api.join-playmeow.com/graphql";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        try
        {
            //發送請求
            yield return request.SendWebRequest();

            Debug.Log(request.downloadHandler.text);

            //請求結果錯誤
            if (request.result != UnityWebRequest.Result.Success)
            {
                onLoginResult?.Invoke($"連線失敗: {request.error}");
                yield break;
            }

            //請求結果成功
            
            string responseText = request.downloadHandler.text;
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(responseText);
            string token = response?.data?.login?.token;

            //以請求結果成功後回傳的內容中是否能取得Token來判斷GraphQL是否登入成功

            //登入失敗
            if (string.IsNullOrEmpty(token))
            {
                onLoginResult?.Invoke("帳號或密碼錯誤");
                yield break;
            }

            //登入成功
            LoginSuccess(token);
            onLoginResult?.Invoke(string.Empty);            
        }
        finally
        {
            //與Native相關的資源不會自動釋放，需要手動釋放，否則會堆積並報錯
            request.Dispose();
        }        
    }

    //Token驗證流程
    IEnumerator QueryMe(string token)
    {
        yield return null;

        //GraphQL query
        string query = @"
        query {
            me {
                id
                username
            }
        }";

        //包成JSON格式
        string json = JsonUtility.ToJson(new GraphQLRequest(query));
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        //建立GraphQL request
        string url = "https://interview-api.join-playmeow.com/graphql";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        try 
        {
            yield return request.SendWebRequest();

            Debug.Log(request.downloadHandler.text);

            //請求結果錯誤
            if (request.result != UnityWebRequest.Result.Success)
            {
                //清除Token
                PlayerPrefs.DeleteKey("token");
                yield break;
            }

            //請求結果成功

            //登入成功
            LoginSuccess(token);
        }
        finally 
        {
            //與Native相關的資源不會自動釋放，需要手動釋放，否則會堆積並報錯
            request.Dispose(); 
        }
    }

    //登入成功的後續執行流程
    void LoginSuccess(string token)
    {
        Debug.Log($"登入成功");

        //將Token值儲存至PlayerPrefs，以便往後開啟遊戲時，可以從PlayerPrefs讀取Token值以進行自動登入
        PlayerPrefs.SetString("token", token);
        PlayerPrefs.Save();

        //觸發登入成功事件
        onLoginSuccess?.Invoke();
    }
}
