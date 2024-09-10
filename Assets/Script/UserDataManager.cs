using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public UserData UserData { get; private set; }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);

        // Unity 소셜 플랫폼을 사용한 로그인
        //PlayGamesPlatform.Activate();
        //Social.localUser.Authenticate(ProcessAuthentication);

        //SetUserData();
    }

    internal void ProcessAuthentication(SignInStatus status)
    {
        if(status == SignInStatus.Success)
        {
            Debug.Log("로그인 성공");
        }
        else
        {
            Debug.Log("로그인 실패");
        }
    }

    private void SetUserData()
    {
        UserData = new UserData();
        UserData.Init();
    }
}


public struct UserData
{
    private string userName;          // 유저 이름
    private int bettingPoint;           // 유저 보유 BP
    private int saved;                      // 유저 저금 BP

    public void Init()
    {
        bettingPoint = 10;
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