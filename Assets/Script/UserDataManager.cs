using System;
using UnityEngine;

using GooglePlayGames;
using GooglePlayGames.BasicApi;

using Firebase.Database;
using Firebase.Auth;
using Firebase;
using Firebase.Extensions;
using UnityEngine.Events;
using System.Collections.Generic;

public class UserDataManager : MonoBehaviour
{
    private UserData data;

    private FirebaseAuth auth;
    private DatabaseReference databaseReference;

    private bool test;

    private void Start()
    {
        test = false;

        // 구글 플레이 게임즈 환경 세팅
        PlayGamesPlatform.InitializeInstance(new PlayGamesClientConfiguration.Builder()
            .RequestIdToken()
            .RequestEmail()
            .Build()
            );

        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
                CustomDebug.SendLog("연결실패");
        });
    }

    public void SignInGPGS(UnityAction<bool> action)
    {
        CustomDebug.SendLog("Login");
        
        // 매번 로그인 채크
        Social.localUser.Authenticate(status =>
        {
            if (status == true)
            {
                CustomDebug.SendLog("GooglePlayLoginSuccese");

                auth = FirebaseAuth.DefaultInstance;

                string idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();

                Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

                auth.SignInWithCredentialAsync(credential).ContinueWith((task) =>
                {
                    CustomDebug.SendLog("LoginFirebase");

                    if (task.IsCanceled || task.IsFaulted)
                    {
                        CustomDebug.SendLog($"Firebase Login Fail : {task.Exception}");
                        action?.Invoke(false);
                    }
                    else
                        LoadFirebaseDatabase(action);
                });
            }
            else
            {
                CustomDebug.SendLog($"GPGS 로그인 실패 : {status}");
                action?.Invoke(false);
            }
        });        
    }

    private void LoadFirebaseDatabase(UnityAction<bool> action)
    {
        FirebaseUser user = auth.CurrentUser;

        if(user != null)
        {
            string userId = user.UserId;

            databaseReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task => 
            {
                if(task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    if(snapshot.Exists)
                    {
                        // 데이터 불러오기
                        data = JsonUtility.FromJson<UserData>(snapshot.GetRawJsonValue());
                    }
                    else
                    {
                        SetUserData();
                        SaveFirebaseDatabase();
                    }

                    action?.Invoke(true);
                }
                else
                {
                    CustomDebug.SendLog("Load Fail");
                    action?.Invoke(false);
                }
            });
        }
    }

    public void SaveFirebaseDatabase()
    {
        if (test == true)
            return;

        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            string userId = user.UserId;
            string json = JsonUtility.ToJson(data, true);

            databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    CustomDebug.SendLog("저장 완료");
                else
                    CustomDebug.SendLog("저장 실패 : ");
            });
        }
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

    public void SetTestData()
    {
        data = new UserData("Player");
        data.Init();
        test = true;
    }

    public void LoadRanking(UnityAction<Ranking> action)
    {
        if (test == true)
        {
            action?.Invoke(null);
            return;
        }

        FirebaseUser user = auth.CurrentUser;

        Ranking ranking = new Ranking();

        if (user != null)
        {
            string userId = user.UserId;

            databaseReference.Child("ranking").GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    if (snapshot.Exists)
                    {
                        // 데이터 불러오기
                        ranking = JsonUtility.FromJson<Ranking>(snapshot.GetRawJsonValue());
                    }

                    action?.Invoke(ranking);
                }
                else
                {
                    action?.Invoke(null);
                }
            });
        }
    }

    public void SaveRanking(Ranking ranking, UnityAction<bool> action)
    {
        if (test == true)
        {
            action?.Invoke(false);
            return;
        }

        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            string userId = user.UserId;
            string json = JsonUtility.ToJson(ranking, true);

            databaseReference.Child("ranking").SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    action?.Invoke(true);
                else
                    action?.Invoke(false);
            });
        }
    }

    public string GetUserId()
    {
        return Social.localUser.id;
    }
}

[Serializable]
public class UserData
{
    [SerializeField] private string userName;      // 유저 이름
    [SerializeField] private int bettingPoint;       // 유저 보유 BP
    [SerializeField] private int saved;                 // 유저 저금 BP

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

[Serializable]
public class RankingData
{
    public string userId;
    public string userName;
    public int bettingPoint;
}

[Serializable]
public class Ranking
{
    public List<RankingData> ranking;

    public Ranking()
    {
        ranking = new List<RankingData>();
    }
}