using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ImageLoad : MonoBehaviour
{
    private GameManager gm;

    [Inject]
    public void Construct(GameManager _gameManager) => gm = _gameManager;

    private Image img;

    private void Start()
    {
        img = GetComponent<Image>();

        if(img.sprite != null)
        {
            string key = $"Assets/Image/{img.sprite.name}.png";

            Sprite load = gm.GetSprite(key);

            if (load != null)
                img.sprite = load;
        }
    }
}
