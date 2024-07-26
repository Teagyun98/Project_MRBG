using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class TitleController : MonoBehaviour
{
    [SerializeField] private GameObject title;
    [SerializeField] PixelPerfectCamera pc;

    private Vector3 maxPos, minPos;
    private bool up;

    private void Start()
    {
        float rate = Screen.width / 1080f;
        pc.assetsPPU = (int)(pc.assetsPPU * rate);

        pc.refResolutionX = Screen.width;
        pc.refResolutionY = Screen.height;

        maxPos = title.transform.position + new Vector3(0, 1, 0);
        minPos = title.transform.position - new Vector3(0, 1, 0);

        up = true;
    }

    private void FixedUpdate()
    {
        if(up== true)
        {
            title.transform.position = Vector3.MoveTowards(title.transform.position, maxPos, 0.1f);

            if (title.transform.position.y > maxPos.y)
                up = false;
        }
        else
        {
            title.transform.position = Vector3.MoveTowards(title.transform.position, minPos, 0.1f);

            if (title.transform.position.y < minPos.y)
                up = true;
        }

    }
}
