using TMPro;
using UnityEngine;
using Zenject;

public class Bank : MonoBehaviour
{
    private GameManager gm;
    private UserDataManager udm;

    [Inject]
    public void Construct(GameManager _gameManager) => gm = _gameManager;
    [Inject]
    public void Construct(UserDataManager _userDataManager) => udm = _userDataManager;

    [SerializeField] private TextMeshProUGUI saveText;
    [SerializeField] private TextMeshProUGUI takeAllText;

    private void OnEnable()
    {
        SetText();
    }

    private void SetText()
    {
        if (udm.UserData.GetSaved() > 0)
            takeAllText.text = "All";
        else
            takeAllText.text = "100BP";

        saveText.text = $"Saved:{udm.UserData.GetSaved()}BP";
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
        {
            int bp = udm.UserData.GetBettingPoint();

            udm.UserData.AddSaved(bp);
            udm.UserData.AddBettingPoint(-bp);
        }
        else if (udm.UserData.GetBettingPoint() >= num)
        {
            udm.UserData.AddSaved(num);
            udm.UserData.AddBettingPoint(-num);
        }

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
        if(udm.UserData.GetBettingPoint() >= 100 && udm.UserData.GetSaved() <= 0)
        {
            gm.Warning("You exceeded the limit.");
            return;
        }

        if (num == -1)
            if (udm.UserData.GetSaved() <= 0)
            {
                udm.UserData.AddBettingPoint(100);
                udm.UserData.AddSaved(-100);
            }
            else
            {
                int saved = udm.UserData.GetSaved();

                udm.UserData.AddBettingPoint(saved);
                udm.UserData.AddSaved(-saved);
            }
        else
        {
            udm.UserData.AddBettingPoint(num);
            udm.UserData.AddSaved(-num);
        }

        SetText();
    }
}
