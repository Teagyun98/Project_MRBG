using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ImageLoad : MonoBehaviour
{
    private GameManager gm;

    [Inject]
    public void Construct(GameManager _gameManager) => gm = _gameManager;

    private Image img;
    [SerializeField] private string customKey;

    private void Start()
    {
        img = GetComponent<Image>();

        if(img.sprite != null)
        {
            string key = $"Assets/Image/{img.sprite.name}.png";

            if(customKey != string.Empty)
                key = customKey;

            Sprite load = gm.GetSprite(key);

            if (load != null)
            {
                img.sprite = load;
                Debug.Log(key);
            }
        }
    }
}
