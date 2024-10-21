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
using System.Linq;

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
        data = new UserData();
        data.Init();
    }

    public void SetTestData()
    {
        data = new UserData();
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

                    CustomDebug.SendLog("랭킹 불러오기 성공");
                    action?.Invoke(ranking);
                }
                else
                {
                    CustomDebug.SendLog("랭킹 불러오기 실패");
                    action?.Invoke(null);
                }
            });
        }
    }

    public void SaveRanking()
    {
        if (test == true || data.GetUserName() == string.Empty)
            return;

        LoadRanking((rankingData)=> 
        {
            Ranking ranking = null;

            if (rankingData != null)
                ranking = rankingData;

            ranking.AddRanking(GetUserId(), data);
            ranking.SortByBP();

            FirebaseUser user = auth.CurrentUser;

            if (user != null)
            {
                // 랭킹 정리
                string json = JsonUtility.ToJson(ranking, true);

                databaseReference.Child("ranking").SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                        CustomDebug.SendLog("랭킹 저장 완료");
                });
            }
        });
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

    public void Init()
    {
        userName = string.Empty;
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

    public void SortByBP()
    {
        ranking = ranking.OrderByDescending(data => data.bettingPoint).ToList();
    }

    public void AddRanking(string _userId, UserData _userData)
    {
        for(int i = 0; i < ranking.Count; i++)
        {
            if (ranking[i].userId == _userId)
            {
                CustomDebug.SendLog("랭킹 갱신");

                if (ranking[i].userName != _userData.GetUserName())
                    ranking[i].userName = _userData.GetUserName();

                if (ranking[i].bettingPoint != _userData.GetAllBP())
                    ranking[i].bettingPoint = _userData.GetAllBP();

                return;
            }
        }

        ranking.Add(new RankingData()
        {
            userId = _userId,
            userName = _userData.GetUserName(),
            bettingPoint = _userData.GetAllBP()
        });
    }

    public int GetRanking(string _userId)
    {
        for (int i = 0; i < ranking.Count; i++)
        {
            if (ranking[i].userId == _userId)
                return i;
        }

        return 0;
    }
}