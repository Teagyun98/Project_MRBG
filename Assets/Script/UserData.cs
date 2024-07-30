using TMPro;
using UnityEngine;

public class UserData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bettingPointText;
    public int BettingPoint { get; private set; }
    public int Saved { get; private set; }

    private void Start()
    {
        // 게임 시작 시 저장되 데이터가 있는지 확인한 후 데이터를 불러오거나 새로운 데이터 추가
        if (PlayerPrefs.HasKey("BP") == true)
            BettingPoint = PlayerPrefs.GetInt("BP");
        else
        {
            PlayerPrefs.SetInt("BP", 10);
            BettingPoint = 10;
        }

        if (PlayerPrefs.HasKey("Saved") == true)
            Saved = PlayerPrefs.GetInt("Saved");
        else
            Saved = 0;

        SetText();
    }

    public void SetBP(int num)
    {
        BettingPoint += num;
        PlayerPrefs.SetInt("BP", BettingPoint);
        SetText();
    }

    public void AddSavedBP(int num)
    {
        SetBP(-num);
        Saved += num;

        PlayerPrefs.SetInt("Saved", Saved);
    }

    public void RemoveSavedBP(int num)
    {
        SetBP(num);
        Saved -= num;

        PlayerPrefs.SetInt("Saved", Saved);
    }

    public void SetText()
    {
        bettingPointText.text = $"{BettingPoint}BP";
    }

    public void Interest()
    {
        Saved += Saved / 100;
    }
}
