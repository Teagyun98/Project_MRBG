using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class TitleController : MonoBehaviour
{
    [SerializeField] private RectTransform title;
    [SerializeField] private PixelPerfectCamera pc;

    private Vector2 maxPos, minPos;
    private bool up;

    private void Start()
    {
        float rate = Screen.width / 1080f;
        pc.assetsPPU = (int)(pc.assetsPPU * rate);

        pc.refResolutionX = Screen.width;
        pc.refResolutionY = Screen.height;

        maxPos = title.anchoredPosition + new Vector2(0, 20);
        minPos = title.anchoredPosition - new Vector2(0, 20);

        up = true;
    }

    private void FixedUpdate()
    {
        if(up== true)
        {
            title.anchoredPosition = Vector2.MoveTowards(title.anchoredPosition, maxPos, 30 * Time.fixedDeltaTime);

            if (title.anchoredPosition.y >= maxPos.y-1)
                up = false;
        }
        else
        {
            title.anchoredPosition = Vector2.MoveTowards(title.anchoredPosition, minPos, 30 * Time.fixedDeltaTime);

            if (title.anchoredPosition.y <= minPos.y+1)
                up = true;
        }
    }
}
