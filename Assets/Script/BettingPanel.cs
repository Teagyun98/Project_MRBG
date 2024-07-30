using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BettingPanel : MonoBehaviour
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

    public void Init()
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

    // 베팅 카드 번호, 예측 순위를 두자리의 int형으로 받는 함수
    public void RankingCheck(int num)
    {
        // 레이싱 중에는 불가능
        if (gm.Race == true)
        {
            gm.Warning("It can't be done during a race.");
            return;
        }

        // 카드 번호와 예측 순위 분리
        int card = num / 10;
        int rank = num % 10;

        // 패널 현황 갱신
        for(int i = 0;  i< cardList.Count; i++) 
        {
            // 이전에 같은 순위를 예측한 다른 카드가 있다면 그 카드의 예측을 초기화 한다.
            if (i != card)
                cardList[i].checkList[rank].SetActive(false);
            // 선택한 카드의 다른 순위가 예측되어 있으면 이번에 예측한 순위를 제외하고 모두 초기화 한다.
            else
                for(int j = 0; j < cardList[i].checkList.Count; j++)
                    cardList[i].checkList[j].SetActive(j == rank ? true : false);
        }

        // 다른 순위에 이번에 예측한 몬스터가 있을 경우 이전에 예측한 순위를 초기화 하고 이번 예측을 저장한다.
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

        // 경기를 시작할 조건이 되는지 확인
        ActiveRaceButton();
    }

    public void Betting(int num)
    {
        // 경기중에는 베팅 할 수 없다.
        if (gm.Race == true)
        {
            gm.Warning("It can't be done during a race.");
            return;
        }

        // BP가 부족할 때
        if (userData.BettingPoint < nowBet + num)
        {
            gm.Warning("Your not enough BP");
            return;
        }

        // AllIn체크
        if (num == -1)
            nowBet = userData.BettingPoint;
        else
            nowBet += num;

        // 현재 베팅한 금액 텍스트 초기화
        SetBetBP();

        // 경기를 시작할 조건이 되는지 확인
        ActiveRaceButton();
    }

    // 현재까지 베팅한 금액 초기화
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

    // 베팅 금액 텍스트 초기화 함수
    private void SetBetBP()
    {
        betBP.text = $"Bet:{nowBet}BP";
    }

    // 경기의 결과를 보여주는 함수
    public void GetResult()
    {
        int reward = nowBet;

        // 1,2,3위를 모두 맞추면 베팅한 금액의 24배를 받을 수 있다.

        // 1등을 맞추었을 경우 2배
        if (gm.RankingList[0] == first)
            reward *= 2;
        // 못 맞추었다면 0
        else
            reward = 0;

        // 이 후 2위를 맞추었다면 3배 
        if (gm.RankingList[1] == second)
        {
            reward *= 3;

            // 2위도 맞추고 3위도 맞추면 4배
            if (gm.RankingList[2] == third)
                reward *= 4;
        }

        // 결과를 알려주는 팝업 활성화
        resultText.text = reward == 0 ? $"-{nowBet}BP" : $"+{reward}BP";
        resultPanel.SetActive(true);

        // 경고 메세지로 한번 더 알려줌
        gm.Warning(resultText.text);

        // 결과를 데이터에 저장
        userData.SetBP(reward);
    }

    // 경기를 시작할 수 있는지 확인하는 함수
    private void ActiveRaceButton()
    {
        // 예측을 완료하고 베팅도 했다면 버튼 활성화
        if (first == null || second == null || third == null || nowBet == 0)
            raceBtn.gameObject.SetActive(false);
        else
            raceBtn.gameObject.SetActive(true);
    }

    // 경기가 시작되면 베팅 금액이 빠져나가도록 하는 함수
    public void BuyTicket()
    {
        userData.SetBP(-nowBet);
    }
}
