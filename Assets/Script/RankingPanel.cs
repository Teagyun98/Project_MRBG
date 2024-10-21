using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

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
    [SerializeField] private GameObject reLoadBtn;

    [Header("RankingPanel")]
    [SerializeField] private List<RankingCell> cellList;
    [SerializeField] private TextMeshProUGUI my_Rank_UserName_BP;
    [SerializeField] private Image my_rankIcon;
    [SerializeField] private List<Sprite> medalIcons;

    [Header("RenamePanel")]
    [SerializeField] private TextMeshProUGUI nowNameText;
    [SerializeField] private TMP_InputField inputField;

    private Ranking ranking;

    private void OnEnable()
    {
        ActiveLoadingPanel();
    }

    public void ActiveRankingPanel()
    {
        loadingPanel.SetActive(false);
        renamePanel.SetActive(false);

        // 랭킹 정보 정렬
        // 내 랭킹 정보 세팅

        ranking.ranking.Sort();

        for (int i = 0; i < cellList.Count; i++)
        {
            if (ranking.ranking.Count > i)
            {
                Sprite icon = null;

                if(i < medalIcons.Count)
                    icon = medalIcons[i];

                cellList[i].Init(ranking.GetRanking(ranking.ranking[i].userId), ranking.ranking[i].userName, ranking.ranking[i].bettingPoint, icon);
            }
            else
                cellList[i].gameObject.SetActive(false);
        }

        int myRaning = ranking.GetRanking(udm.GetUserId());

        if (myRaning < medalIcons.Count)
        {
            my_rankIcon.sprite = medalIcons[myRaning];
        }

        my_Rank_UserName_BP.text = $"{myRaning+1}. {udm.GetData().GetUserName()} : {udm.GetData().GetAllBP()}";

        rankingPanel.SetActive(true);
    }

    public void ActiveLoadingPanel()
    {
        rankingPanel.SetActive(false);
        renamePanel.SetActive(false);

        loadingPanel.SetActive(true);
        reLoadBtn.gameObject.SetActive(false);

        loadingText.text = "Loading...";

        udm.LoadRanking((_ranking) =>
        {
            if (_ranking != null)
            {
                ranking = _ranking;

                bool already = false;

                foreach (RankingData data in ranking.ranking)
                {
                    if (udm.GetUserId() == data.userId)
                    {
                        ActiveRankingPanel();
                        already = true;
                        break;
                    }
                }

                if(already == false)
                    ActiveRenamePanel();
            }
            else
            {
                loadingText.text = "Can't get data";
                reLoadBtn.gameObject.SetActive(true);
            }
        });
    }

    public void ActiveRenamePanel()
    {
        loadingPanel.SetActive(false);
        rankingPanel.SetActive(false);

        nowNameText.text = $"Now Name : {udm.GetData().GetUserName()}";
        inputField.text = string.Empty;

        renamePanel.SetActive(true);
    }

    public void Rename()
    {
        // 바뀐 이름으로 데이터 수정 및 저장
        // 바뀐 이름으로 랭킹 데이터 수정 및 저장
        string newName = inputField.text;

        // 인풋 필드의 텍스트 중 닉네임으로 사용할 수 없는 이름이거나 중복된 닉네임의 경우 다시 설정하게 하기
        if(newName.Length == 0 || newName.Length > 15)
        {
            gm.Warning("Please enter 0~15 char");
            ActiveRenamePanel();
        }

        string censor = newName.Replace(" ", "");
        censor.ToLower();

        if(censor.Contains("sex") || censor.Contains("fuck") || censor.Contains("pussey") || censor.Contains("penis"))
        {
            gm.Warning("Use the other words");
            ActiveRenamePanel();
        }


        udm.GetData().SetUserName(newName);

        bool changeData = false;
        
        foreach(RankingData data in ranking.ranking)
        {
            if(udm.GetUserId() == data.userId)
            {
                data.userName = newName;
                changeData = true;
                break;
            }    
        }

        if (changeData == false)
            ranking.AddRanking(udm.GetUserId(), udm.GetData());

        udm.SaveFirebaseDatabase();
        udm.SaveRanking();

        ActiveRankingPanel();
    }

}
