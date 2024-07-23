using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Display : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> rankingList;

    public void Set(int num, Sprite icon)
    {
        rankingList[num].sprite = icon;
        rankingList[num].gameObject.SetActive(true);
    }

    public void Reset()
    {
        foreach (SpriteRenderer r in rankingList)
            r.gameObject.SetActive(false);
    }
}
