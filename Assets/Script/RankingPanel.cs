using System.Collections.Generic;
using TMPro;
using UnityEngine;

class RankingCell { }

public class RankingPanel : MonoBehaviour
{
    // 로딩 활성화 하고 랭킹 데이터 불러오기
    // 랭킹 데이터를 불러오면 그에 맞추어 패널 세팅

    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject panel;
    [SerializeField] private List<RankingCell> cellList;

    [SerializeField] private TextMeshProUGUI myRanking;
    [SerializeField] private TextMeshProUGUI myBP;
    [SerializeField] private List<GameObject> myMedalList;

    private void OnEnable()
    {
        
    }
}
