using System;
using UnityEngine;

using GooglePlayGames;
using GooglePlayGames.BasicApi;

using Firebase.Database;
using Firebase.Auth;
using Firebase;
using Firebase.Extensions;
using UnityEngine.Events;

public class UserDataManager : MonoBehaviour
{
    private UserData data;

    private FirebaseAuth auth;
    private DatabaseReference databaseReference;

    private void Start()
    {
        // 구글 플레이 게임즈 환경 세팅
        PlayGamesPlatform.InitializeInstance(new PlayGamesClientConfiguration.Builder()
            .RequestIdToken()
            .RequestEmail()
            .Build()
            );

        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        auth = FirebaseAuth.DefaultInstance;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
                Debug.Log("연결실패");
        });
    }

    public void SignInGPGSFirebase(UnityAction<bool> action)
    {
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == true)
            {
                string idToken = ((PlayGamesLocalUser)PlayGamesPlatform.Instance.localUser).GetIdToken();

                Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

                auth.SignInWithCredentialAsync(credential).ContinueWith((task) => 
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.Log("Firebase Login Fail");
                        action?.Invoke(false);
                    }
                    else
                        LoadFirebaseDatabase(action);
                });
            }
            else
            {
                Debug.Log("GPGS 로그인 실패");
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
                    Debug.Log("Load Fail");
                    action?.Invoke(false);
                }
            });
        }
    }

    public void SaveFirebaseDatabase()
    {
        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            string userId = user.UserId;
            string json = JsonUtility.ToJson(data, true);

            databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    Debug.Log("저장 완료");
                else
                    Debug.Log("저장 실패 : ");
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