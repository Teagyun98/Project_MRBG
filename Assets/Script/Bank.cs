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
        if (udm.GetData().GetSaved() > 0)
            takeAllText.text = "All";
        else
            takeAllText.text = "100BP";

        saveText.text = $"Saved:{udm.GetData().GetSaved()}BP";
        gm.SetBPText();
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
            int bp = udm.GetData().GetBettingPoint();

            udm.GetData().AddSaved(bp);
            udm.GetData().AddBettingPoint(-bp);
        }
        else if (udm.GetData().GetBettingPoint() >= num)
        {
            udm.GetData().AddSaved(num);
            udm.GetData().AddBettingPoint(-num);
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
        if(udm.GetData().GetBettingPoint() >= 100 && udm.GetData().GetSaved() <= 0)
        {
            gm.Warning("You exceeded the limit.");
            return;
        }

        if (num == -1)
            if (udm.GetData().GetSaved() <= 0)
            {
                udm.GetData().AddBettingPoint(100);
                udm.GetData().AddSaved(-100);
            }
            else
            {
                int saved = udm.GetData().GetSaved();

                udm.GetData().AddBettingPoint(saved);
                udm.GetData().AddSaved(-saved);
            }
        else
        {
            udm.GetData().AddBettingPoint(num);
            udm.GetData().AddSaved(-num);
        }

        SetText();
    }
}
