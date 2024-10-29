using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

[Serializable]
public struct Line
{
    public Transform startPoint;
    public Transform endPoint;
}

public class GameManager : MonoBehaviour
{
    private UserDataManager udm;
    [Inject]
    public void Construct(UserDataManager _userDataManager) => udm = _userDataManager;

    [SerializeField] private GameObject gameScreen;
    [SerializeField] private List<Line> lines;
    [SerializeField] private List<MonsterController> monsters;
    [SerializeField] private Display display;
    [SerializeField] private BettingPanel bp;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private TextMeshProUGUI bettingPointText;
    [SerializeField] private GameObject kickPanel;

    public List<MonsterController> ReadyMonsterList { get; private set; }
    public List<MonsterController> RankingList { get; private set; }

    public bool Race { get; private set; }
    public int GameSpeed { get; private set; }

    public float hintTime;
    public event UnityAction hint;

    private void Start()
    {
        if (PlayerPrefs.HasKey("GameSpeed") == true)
        {
            GameSpeed = PlayerPrefs.GetInt("GameSpeed");
            speedText.text = $"X{GameSpeed}";
        }
        else
            GameSpeed = 1;

        GameSet();
        SetBPText();

        hintTime = 0f;
    }

    private void Update()
    {
        if(hintTime >= 0f && Race == false)
            hintTime += Time.deltaTime;

        if(hintTime > 3f)
        {
            if(hint != null)
                hint?.Invoke();

            hintTime = -1f;
        }
    }

    public void GameSet()
    {
        // 모든 경주 몬스터 비활성화
        foreach (MonsterController monster in monsters)
        {
            if (monster.Sm != null)
                monster.Sm.SetState(monster.DicState[MonsterState.Idle]);

            monster.gameObject.SetActive(false);
        }

        // 리스트 초기화
        ReadyMonsterList = new List<MonsterController>();
        RankingList = new List<MonsterController>();

        // 경기 시작 bool값 초기화
        Race = false;

        display.Reset();

        // 경주에 참여할 랜덤 몬스터 뽑기
        MonsterController randMonsger;

        while(ReadyMonsterList.Count != 6)
        {
            randMonsger = monsters[UnityEngine.Random.Range(0, monsters.Count)];

            if(ReadyMonsterList.Contains(randMonsger) == false)
                ReadyMonsterList.Add(randMonsger);
        }

        // 뽑은 몬스터에 레일 할당
        for(int i = 0; i< ReadyMonsterList.Count; i++) 
            ReadyMonsterList[i].SetLine(lines[i]);

        // 베팅 패널 세팅은 경주에 참가하는 몬스터를 뽑고 해야함
        bp.Init();
    }

    public void RaceStart()
    {
        foreach (MonsterController monster in ReadyMonsterList)
            monster.Go();

        Race = true;
        bp.BuyTicket();

        // 은행 이자
        udm.GetData().AddSaved(udm.GetData().GetSaved()/20);

        udm.SaveFirebaseDatabase((complete) => 
        {
            if (complete == false)
                ActiveKickPanel();
        });
        udm.SaveRanking((complete) => 
        {
            if (complete == false)
                ActiveKickPanel();
        });
    }

    public float FirstMonsterPosX(bool dice = false)
    {
        float posX = 0;

        foreach (MonsterController monster in ReadyMonsterList)
            if(RankingList.Contains(monster) == false)
                if (posX < monster.transform.position.x)
                    posX = monster.transform.position.x;

        if (dice)
            return posX;

        // 모든 몬스터가 골 라인에 들어온 경우 레이스 결과를 확인할 수 있게 x값 유지
        if(ReadyMonsterList.Count == RankingList.Count)
            posX = RankingList[0].transform.position.x;

        return posX < 2.4f ? posX : 2.4f;
    }

    public MonsterController FirstMonster()
    {
        MonsterController result = null;

        foreach (MonsterController monster in ReadyMonsterList)
            if(RankingList.Contains(monster) == false)
                if (result == null || (result != null && result.transform.position.x < monster.transform.position.x))
                    result = monster;

        return result;
    }

    public List<MonsterController> OtherMonsterList(MonsterController me)
    {
        return ReadyMonsterList.Where(p => p != me && RankingList.Contains(p) == false).ToList();
    }

    // 현재 도착 하지 않고 달리고 있는 몬스터 반환
    public List<MonsterController> RunningMonsterList()
    {
        return ReadyMonsterList.Where(p => RankingList.Contains(p) == false).ToList();
    }

    public void Goal(MonsterController monster)
    {
        if (RankingList.Contains(monster) == true)
            return;

        RankingList.Add(monster);
        display.Set(RankingList.Count - 1, monster.GetIcon());

        if (RankingList.Count == ReadyMonsterList.Count)
            bp.GetResult(RankingList);
        else
        {
            foreach (MonsterController other in ReadyMonsterList)
                if (RankingList.Contains(other) == false)
                    other.SendMessage();
        }
    }

    public void SetGameSpeed()
    {
        if (GameSpeed == 3)
            GameSpeed = 1;
        else
            GameSpeed++;

        speedText.text = $"X{GameSpeed}";
        PlayerPrefs.SetInt("GameSpeed", GameSpeed);

        foreach (MonsterController monster in ReadyMonsterList)
            monster.Animator.speed = GameSpeed;
    }

    public void Warning(string msg)
    {
        warningText.text = msg;
        warningPanel.SetActive(true);
    }

    public GameObject GetGameScreen()
    {
        return gameScreen;
    }

    public void SetBPText()
    {
        bettingPointText.text = $"{udm.GetData().GetBettingPoint()}BP";
    }

    public IEnumerator ColorChangeHint(Button btn)
    {
        bool red = false;
        Image img = btn.GetComponent<Image>();
        Color32 color = img.color;

        while(true)
        {
            if(red == false)
            {
                img.color = new Color32(255, 0, 0, 255);
                red = true;
            }
            else
            {
                img.color = color;
                red = false;
            }

            if (Race == true)
                yield break;

            yield return new WaitForSeconds(0.2f);
        }
    }

    public IEnumerator ColorChangeHint(Image img)
    {
        bool red = false;
        Color32 color = img.color;

        while (true)
        {
            if (red == false)
            {
                img.color = new Color32(255, 0, 0, 255);
                red = true;
            }
            else
            {
                img.color = color;
                red = false;
            }

            if (Race == true)
                yield break;

            yield return new WaitForSeconds(0.2f);
        }
    }

    public void ActiveKickPanel()
    {
        kickPanel.SetActive(true);
    }

    public void MoveTitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
