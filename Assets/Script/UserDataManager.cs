using System;
using UnityEngine;
using System.Collections;

using GooglePlayGames;
using GooglePlayGames.BasicApi;

using Firebase.Database;
using Firebase.Auth;
using Firebase;
using Firebase.Extensions;

public class UserDataManager : MonoBehaviour
{
    private UserData data;

    private DatabaseReference reference;
    private FirebaseAuth auth;

    private void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                PlayGamesPlatform.Instance.Authenticate(status => 
                {
                    if (status == SignInStatus.Success)
                    {

                    }
                    else
                    {
                        Debug.Log("구글 플레이 로그인 실패");
                    }
                });
            }
            else
                Debug.Log("연결실패");
        });

        // Unity 소셜 플랫폼을 사용한 로그인
        //PlayGamesPlatform.Activate();
        //Social.localUser.Authenticate(ProcessAuthentication);

        SetUserData();
    }

    private IEnumerator FireBaseLogin()
    {
        

        yield break;
    }

    public UserData GetData()
    {
        return data;
    }

    private void SetUserData()
    {
        data = new UserData("Player");
        data.Init();
    }
}

[Serializable]
public class UserData
{
    private string userName;      // 유저 이름
    private int bettingPoint;       // 유저 보유 BP
    private int saved;                 // 유저 저금 BP

    public UserData(string _userName) => userName = _userName;

    public void Init()
    {
        bettingPoint = 100;
        saved = 0;
    }

    public string GetUserName() { return userName; }
    public void SetUserName(string _userName) { userName = _userName; }

    public int GetBettingPoint() { return bettingPoint; }
    public void SetBettingPoint(int _bettingPoint) { bettingPoint = _bettingPoint; }
    public void AddBettingPoint(int _add) { bettingPoint += _add; }

    public int GetSaved() { return saved; }
    public void SetSaved(int _saved) {  saved = _saved; }
    public void AddSaved(int _add) {  saved += _add; }

    public int GetAllBP() { return bettingPoint + saved; }
}