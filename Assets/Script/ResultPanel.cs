using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI curBetText;
    [SerializeField] private List<Image> curResultList;
    [SerializeField] private List<Image> curBetList;
    [SerializeField] private List<TextMeshProUGUI> curResultTextList;
    [SerializeField] private List<GameObject> xList;
    [SerializeField] private List<GameObject> oList;
    [SerializeField] private TextMeshProUGUI getBPText;

    public void SetResultPanel(int _curBet, List<MonsterController> _curReultList, List<MonsterController> _curBetList, int _getBP)
    {
        curBetText.text = $"Betting : {_curBet}";

        for (int i = 0; i < _curReultList.Count; i++)
        {
            curResultList[i].sprite = _curReultList[i].GetIcon();
        }

        for (int i = 0; i < _curBetList.Count; i++)
        {
            curBetList[i].sprite = _curBetList[i].GetIcon();

            if (_curReultList[i] == _curBetList[i])
            {
                if(i == 0 || (i > 0 && curResultTextList[i-1].text != "X0"))
                {
                    curResultTextList[i].text = $"X{i + 2}";
                    curResultTextList[i].gameObject.SetActive(true);
                }
                else
                {
                    curResultTextList[i].text = $"X0";
                    curResultTextList[i].gameObject.SetActive(i == 0);
                }

                xList[i].SetActive(false);
                oList[i].SetActive(true);
            }
            else
            {
                curResultTextList[i].text = $"X0";
                curResultTextList[i].gameObject.SetActive(i == 0);

                xList[i].SetActive(true);
                oList[i].SetActive(false);
            }
        }

        getBPText.text = _getBP == 0 ? $"-{_curBet}BP" : $"+{_getBP}BP";

        gameObject.SetActive(true);
    }
}
