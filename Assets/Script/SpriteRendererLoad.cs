using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SpriteRendererLoad : MonoBehaviour
{
    private GameManager gm;

    [Inject]
    public void Construct(GameManager _gameManager) => gm = _gameManager;

    private SpriteRenderer spr;
    [SerializeField] private string customKey;

    private void Start()
    {
        spr = GetComponent<SpriteRenderer>();

        if(spr.sprite != null)
        {
            string key = $"Assets/Image/{spr.sprite.name}.png";

            if(customKey != string.Empty)
                key = customKey;

            Debug.Log(key);

            Sprite load = gm.GetSprite(key);

            if (load != null)
            {
                spr.sprite = load;
                Debug.Log(key);
            }
        }
    }
}
