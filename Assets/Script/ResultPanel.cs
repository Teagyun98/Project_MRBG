using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI curBetText;
    [SerializeField] private List<Image> curResultList;
    [SerializeField] private List<Image> curBetList;
    [SerializeField] private TextMeshProUGUI getBPText;

    public void SetResultPanel(int _curBet, List<Sprite> _curReultList, List<Sprite> _curBetList, int _getBP)
    {
        curBetText.text = $"Betting : {_curBet}";

        for (int i = 0; i < _curReultList.Count; i++)
        {
            curResultList[i].sprite = _curReultList[i];
        }

        for (int i = 0; i < _curBetList.Count; i++)
        {
            curBetList[i].sprite = _curBetList[i];
        }

        getBPText.text = $"+{_getBP}BP";

        gameObject.SetActive(true);
    }
}
