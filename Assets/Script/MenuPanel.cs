using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MenuPanel : MonoBehaviour
{
    private GameManager gm;
    private UserDataManager udm;

    [Inject]
    public void Construct(GameManager _gameManager) => gm = _gameManager;
    [Inject]
    public void Construct(UserDataManager _userDataManager) => udm = _userDataManager;

    [SerializeField] private Button bettingBtn;
    [SerializeField] private Button bankBtn;

    private Coroutine hintCo;

    private void OnEnable()
    {
        gm.hint += Hint;
        gm.hintTime = 0;
    }

    private void OnDisable()
    {
        gm.hint -= Hint;

        if(hintCo != null)
        {
            StopCoroutine(hintCo);
            hintCo = null;
        }

        bettingBtn.GetComponent<Image>().color = Color.white;
        bankBtn.GetComponent<Image>().color = Color.white;
    }

    private void Hint()
    {
        if (udm.GetData().GetBettingPoint() > 0)
            hintCo = StartCoroutine(gm.ColorChangeHint(bettingBtn));
        else
            hintCo = StartCoroutine(gm.ColorChangeHint(bankBtn));
    }
}
