using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BettingPanle : MonoBehaviour
{
    private GameManager gm;
    private UserData userData;

    [Inject]
    public void Construct(GameManager _gameManager) => gm = _gameManager;
    [Inject]
    public void Construct(UserData _userdata) => userData = _userdata;

    [SerializeField] private List<BettingCard> cardList;
    [SerializeField] private TextMeshProUGUI betBP;
    [SerializeField] private Button raceBtn;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    private int nowBet;

    private MonsterController first;
    private MonsterController second;
    private MonsterController third;

    public void Set()
    {
        // 패널 초기화
        int i = 0;

        foreach(BettingCard card in cardList)
        {
            card.icon.sprite = gm.ReadyMonsterList[i++].GetIcon();

            foreach (GameObject check in card.checkList)
                check.SetActive(false);
        }

        first = null;
        second = null;
        third = null;

        nowBet = 0;
        SetBetBP();

        ActiveRaceButton();
    }

    public void RankingCheck(int num)
    {
        if (gm.Race == true)
        {
            gm.Warning("It can't be done during a race.");
            return;
        }

        int card = num / 10;
        int rank = num % 10;

        // 패널 현황 갱신
        for(int i = 0;  i< cardList.Count; i++) 
        {
            if (i != card)
                cardList[i].checkList[rank].SetActive(false);
            else
                for(int j = 0; j < cardList[i].checkList.Count; j++)
                    cardList[i].checkList[j].SetActive(j == rank ? true : false);
        }

        switch (rank)
        {
            case 0:
                first = gm.ReadyMonsterList[card];
                second = gm.ReadyMonsterList[card] == second ? null : second;
                third = gm.ReadyMonsterList[card] == third ? null : third;
                break;
            case 1:
                first = gm.ReadyMonsterList[card] == first ? null : first;
                second = gm.ReadyMonsterList[card];
                third = gm.ReadyMonsterList[card] == third ? null : third;
                break;
            case 2:
                first = gm.ReadyMonsterList[card] == first ? null : first;
                second = gm.ReadyMonsterList[card] == second ? null : second;
                third = gm.ReadyMonsterList[card];
                break;
        }

        ActiveRaceButton();
    }

    public void Betting(int num)
    {
        if (gm.Race == true)
        {
            gm.Warning("It can't be done during a race.");
            return;
        }

        if (userData.BettingPoint < nowBet + num)
        {
            gm.Warning("Your not enough BP");
            return;
        }

        if (num == -1)
            nowBet = userData.BettingPoint;
        else
            nowBet += num;

        SetBetBP();

        ActiveRaceButton();
    }

    public void BetReset()
    {
        if (gm.Race == true)
        {
            gm.Warning("It can't be done during a race.");
            return;
        }

        nowBet = 0;
        SetBetBP();

        ActiveRaceButton();
    }

    private void SetBetBP()
    {
        betBP.text = $"Bet:{nowBet}BP";
    }

    public void GetResult()
    {
        int reward = nowBet;

        if (gm.RankingList[0] == first)
            reward *= 2;
        else
            reward = 0;

        if (gm.RankingList[1] == second)
        {
            reward *= 3;

            if (gm.RankingList[2] == third)
                reward *= 4;
        }

        resultText.text = reward == 0 ? $"-{nowBet}BP" : $"+{reward}BP";
        gm.Warning(resultText.text);
        resultPanel.SetActive(true);

        userData.SetBP(reward);
    }

    private void ActiveRaceButton()
    {
        if (first == null || second == null || third == null || nowBet == 0)
            raceBtn.gameObject.SetActive(false);
        else
            raceBtn.gameObject.SetActive(true);
    }

    public void BuyTicket()
    {
        userData.SetBP(-nowBet);
    }
}
