using TMPro;
using UnityEngine;
using Zenject;

public class Bank : MonoBehaviour
{
    private GameManager gm;
    private UserData userData;

    [Inject]
    public void Construct(GameManager _gameManager) => gm = _gameManager;
    [Inject]
    public void Construct(UserData _userdata) => userData = _userdata;

    [SerializeField] private TextMeshProUGUI saveText;
    [SerializeField] private TextMeshProUGUI takeAllText;

    private void OnEnable()
    {
        SetText();
    }

    private void SetText()
    {
        if (userData.Saved > 0)
            takeAllText.text = "All";
        else
            takeAllText.text = "100BP";

        saveText.text = $"Saved:{userData.Saved}BP";
    }

    // BP를 저장하는 함수
    public void SaveMoney(int num)
    {
        // 레이스 중에는 불가능
        if (gm.Race == true)
        {
            gm.Warning("It can't be done during a race.");
            return;
        }

        if (num == -1)
            userData.AddSavedBP(userData.BettingPoint);
        else if(userData.BettingPoint >= num)
            userData.AddSavedBP(num);

        SetText();
    }

    // BP를 빌리는 함수
    public void OweMoney(int num)
    {
        if (gm.Race == true)
        {
            gm.Warning("It can't be done during a race.");
            return;
        }

        // 한 경기에 최대로 빌릴 수 있는 금액 제한
        if(userData.BettingPoint >= 100 && userData.Saved <= 0)
        {
            gm.Warning("You exceeded the limit.");
            return;
        }

        if (num == -1)
            if (userData.Saved <= 0)
                userData.RemoveSavedBP(100);
            else
                userData.RemoveSavedBP(userData.Saved);
        else
            userData.RemoveSavedBP(num);

        SetText();
    }
}
