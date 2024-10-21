using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingCell : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ranking_UserName_BP;
    [SerializeField] private Image rankIcon;

    public void Init(int ranking, string userName, int bp, Sprite icon = null)
    {
        ranking_UserName_BP.text = $"{ranking+1}. {userName} : {bp}";

        if (icon == null)
            rankIcon.gameObject.SetActive(false);
        else
        {
            rankIcon.sprite = icon;
            rankIcon.gameObject.SetActive(true);
        }

        gameObject.SetActive(true);
    }
}
