using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

class RankingCell { }

public class RankingPanel : MonoBehaviour
{
    private UserDataManager udm;
    private GameManager gm;
    [Inject]
    public void Construct(UserDataManager _userDataManager) => udm = _userDataManager;
    public void Construct(GameManager _gameManager) => gm = _gameManager;

    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private GameObject renamePanel;

    [Header("LoadingPanel")]
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("RankingPanel")]
    [SerializeField] private List<RankingCell> cellList;

    [SerializeField] private TextMeshProUGUI myRanking;
    [SerializeField] private TextMeshProUGUI myBP;
    [SerializeField] private List<GameObject> myMedalList;

    [Header("RenamePanel")]
    [SerializeField] private TextMeshProUGUI nowNameText;
    [SerializeField] private InputField inputField;

    private Ranking ranking;

    private void OnEnable()
    {
        // 랭킹 로드
        // 랭킹에 유저의 랭킹이 등록되어 있지 않으면 이름 바꾸기 패널로 이동
        // 랭킹이 등록되어 있으면 랭킹 패널로 이동

        loadingPanel.SetActive(true);
        rankingPanel.SetActive(false);
        renamePanel.SetActive(false);

        udm.LoadRanking( (_ranking) => 
        {
            if(_ranking != null)
            {
                ranking = _ranking;

                foreach(RankingData data in ranking.ranking)
                {
                    if(udm.GetUserId() == data.userId)
                    {
                        loadingPanel.SetActive(false);
                        rankingPanel.SetActive(true);
                    }
                    else
                    {
                        loadingPanel.SetActive(false);
                        renamePanel?.SetActive(true);
                    }
                }
            }
        });
    }

    private void InitRankingPanel()
    {

    }

    private void InitRenamePanel()
    {
        // 현재 자기 닉네임 보여주기
        // 인풋 필드에 자기 닉네임 적어주기
    }

    public void ActiveRenamePanel()
    {
        rankingPanel.SetActive(false);
        InitRenamePanel();
        renamePanel.SetActive(true);
    }

    public void Rename()
    {
        // 바뀐 이름으로 데이터 수정 및 저장
        // 바뀐 이름으로 랭킹 데이터 수정 및 저장

        renamePanel.SetActive(false);
        InitRankingPanel();
        rankingPanel.SetActive(true);
    }

}
