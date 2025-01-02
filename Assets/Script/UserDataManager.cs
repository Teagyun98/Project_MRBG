using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using GooglePlayGames;
using GooglePlayGames.BasicApi;

using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;
using System.Threading.Tasks;

public class UserDataManager : MonoBehaviour
{
    // Json 파일 저장 경로 C:\Users\story\AppData\LocalLow\DefaultCompany\CatMergeTown
    private readonly string gameDataFileName = "UserData.json";

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
                CustomDebug.SendLog("GPGS 로그인 성공");

                auth = FirebaseAuth.DefaultInstance;

                string idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();

                Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

                auth.SignInWithCredentialAsync(credential).ContinueWith((task) =>
                {
                    CustomDebug.SendLog("LoginFirebase");

                    if (task.IsCanceled || task.IsFaulted)
                    {
                        CustomDebug.SendLog($"Firebase login fail");
                        action?.Invoke(false);
                    }
                    else
                        LoadGameData(action);
                });
            }
            else
            {
                LoadGameData(action);
                CustomDebug.SendLog($"GPGS 로그인 실패 : {status}");
                action?.Invoke(false);
            }
        });        
    }

    // 로컬 데이터 불러오기
    public void LoadGameData(UnityAction<bool> login)
    {
        // Json으로 저장된 파일 경로를 string값으로 저장
        string filePath = Application.persistentDataPath + "/" + gameDataFileName;

        // File.Exists 지정된 경로에 파일이 있는지 확인
        if (File.Exists(filePath))
        {
            // 파일이 있을경우 string값으로 모든 텍스트 불러오기
            string FromJsonData = File.ReadAllText(filePath);
            // 유저 데이터에 Json파일에서 가져온 string값 적용
            data = JsonUtility.FromJson<UserData>(FromJsonData);

            Debug.Log("Json Data 불러오기 완료");
            login?.Invoke(true);
        }
        // 지정된 경로에 로컬 저장된 게임 데이터가 없고 유저가 Firebase로그인이 되어 있다면 database에 유저 정보가 있는지 확인
        else if (auth != null && auth.CurrentUser != null)
        {
            LoadFirebaseDatabase(login);
        }
        else
        {
            SetUserData();
            SaveGameData();
            login?.Invoke(true);
        }
    }

    // 로컬 데이터 저장
    public void SaveGameData()
    {
        // 유저 데이터 string값으로 직렬화
        string toJsonData = JsonUtility.ToJson(data, true);
        string filePath = Application.persistentDataPath + "/" + gameDataFileName;

        // Json파일에 직렬화한 string값 저장
        File.WriteAllText(filePath, toJsonData);

        Debug.Log("Json Data 저장 완료");

        // 파이어 베이스 데이터 저장
        SaveFirebaseDatabase();
    }

    private void LoadFirebaseDatabase(UnityAction<bool> action)
    {
        if (auth == null || auth.CurrentUser == null)
        {
            CustomDebug.SendLog("Load Fail");
            action?.Invoke(false);
            return;
        }

        string userId = auth.CurrentUser.UserId;

        databaseReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    // 데이터 불러오기
                    data = JsonUtility.FromJson<UserData>(snapshot.GetRawJsonValue());
                }
                else
                {
                    SetUserData();
                    SaveGameData();
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

    private void SaveFirebaseDatabase()
    {
        if (auth == null || auth.CurrentUser == null)
        {
            CustomDebug.SendLog("저장 실패");
            return;
        }

        string userId = auth.CurrentUser.UserId;
        string json = JsonUtility.ToJson(data, true);

        databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                CustomDebug.SendLog("저장 완료");
            }
            else
            {
                CustomDebug.SendLog("저장 실패");
            }
        });
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


    public void LoadRanking(UnityAction<Ranking> action)
    {
        if (auth == null || auth.CurrentUser == null)
        {
            CustomDebug.SendLog("랭킹 불러오기 실패");
            action?.Invoke(null);
            return;
        }

        Ranking ranking = new Ranking();

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

    public void SaveRanking(UnityAction<bool> action)
    {
        if (auth == null || auth.CurrentUser == null || data.GetUserName() == string.Empty)
            return;

        LoadRanking((rankingData) =>
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
                    {
                        CustomDebug.SendLog("랭킹 저장 완료");
                        action?.Invoke(true);
                    }
                    else
                    {
                        CustomDebug.SendLog("랭킹 저장 실패");
                        action?.Invoke(false);
                    }
                });
            }
            else
            {
                CustomDebug.SendLog("랭킹 저장 실패");
                action?.Invoke(false);
            }
        });
    }

    public void ConnectCheck(UnityAction<bool> action)
    {
        if (auth == null | auth.CurrentUser == null)
        {
            action?.Invoke(false);
        }

        FirebaseDatabase.DefaultInstance.GetReference(".info/connected").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && (bool)task.Result.Value)
            {
                action?.Invoke(true);
            }
            else
            {
                action?.Invoke(false);
            }
        });
    }

    public bool LoginCheck()
    {
        if (auth == null || auth.CurrentUser == null)
        {
            return false;
        }

        return true;
    }

    public string GetUserId()
    {
        return Social.localUser.id;
    }

    // 구글 플레이 게임즈 로그아웃
    public void GoogleLogOut()
    {
        // filePath 경로에 파일이 있으면 삭제하고 유저 데이터를 초기화한다.
        string filePath = Application.persistentDataPath + "/" + gameDataFileName;
        File.Delete(filePath);
        SetUserData();

        auth.SignOut();
        ((PlayGamesPlatform)Social.Active).SignOut();

        StartCoroutine(LoadScene("TitleScene"));
    }

    // 회원 탈퇴 함수
    public IEnumerator WithdrawUser()
    {
        FirebaseUser user = auth.CurrentUser;

        if (user == null)
        {
            GoogleLogOut();

            yield break;
        }
        else
        {
            // 데이터 지우기
            Task task_1 = databaseReference.Child("users").Child(user.UserId).RemoveValueAsync();

            // 회원탈퇴
            Task task_2 = user.DeleteAsync();

            yield return new WaitUntil(() => task_1.IsCompleted && task_2.IsCompleted);

            if (task_1.IsCanceled || task_1.IsFaulted)
                yield break;

            if (task_2.IsCanceled || task_2.IsFaulted)
                yield break;

            //로그아웃
            GoogleLogOut();

            // 다음 씬 로드
            StartCoroutine(LoadScene("TitleScene"));
        }
    }

    public IEnumerator LoadScene(string sceneName, bool destroy = false)
    {
        AsyncOperation asyncOper = SceneManager.LoadSceneAsync(sceneName);

        // 로딩 화면 켜기
        yield return new WaitUntil(() => asyncOper.isDone);
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
