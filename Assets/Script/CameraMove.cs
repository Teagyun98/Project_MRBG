using UnityEngine;
using UnityEngine.U2D;
using Zenject;

public class CameraMove : MonoBehaviour
{
    private GameManager gm;

    [Inject]
    public void Construct(GameManager _gameManager)
    {
        gm = _gameManager;
    }

    private PixelPerfectCamera pc;
    public MonsterController Target { get; private set; }
    private float focusTime;
    private int camSize;
    private int zoomSize;

    private void Start()
    {
        pc = GetComponent<PixelPerfectCamera>();

        float rate = Screen.width / 1080f;
        camSize = (int)(pc.assetsPPU * rate);

        pc.assetsPPU = camSize;

        pc.refResolutionX = Screen.width;
        pc.refResolutionY = Screen.height;

        focusTime = 0;
    }

    //0~2.4
    private void FixedUpdate()
    {
        if(focusTime > 0 && Target != null)
        {
            if (gm.GetGameScreen().transform.localScale.x != 3)
                gm.GetGameScreen().transform.localScale = Vector3.Lerp(gm.GetGameScreen().transform.localScale, new Vector3(3, 3, 3), Time.fixedDeltaTime * 5 * gm.GameSpeed);

            Vector3 movePos = new Vector3(Target.transform.position.x, Target.transform.position.y - 1f, -10);
            transform.position = Vector3.Lerp(transform.position, movePos, Time.fixedDeltaTime * 5 * gm.GameSpeed);
            focusTime -= Time.fixedDeltaTime * gm.GameSpeed;

            if (focusTime <= 0)
                Target = null;
        }
        else if (gm.Race == true)
        {
            if (gm.GetGameScreen().transform.localScale.x != 1)
                gm.GetGameScreen().transform.localScale = Vector3.Lerp(gm.GetGameScreen().transform.localScale, new Vector3(1, 1, 1), Time.fixedDeltaTime * 5 * gm.GameSpeed);

            Vector3 movePos = new Vector3(gm.FirstMonsterPosX(), 0, -10);
            transform.position = Vector3.Lerp(transform.position, movePos, Time.fixedDeltaTime * 5 * gm.GameSpeed);
        }
        else
        {
            if (gm.GetGameScreen().transform.localScale.x != 1)
                gm.GetGameScreen().transform.localScale = Vector3.Lerp(gm.GetGameScreen().transform.localScale, new Vector3(1, 1, 1), Time.fixedDeltaTime * 5 * gm.GameSpeed);

            transform.position = new Vector3(0, 0, -10);
        }
    }

    public void FocusCamera(MonsterController monster)
    {
        Target = monster;
        focusTime = 2;
    }
}
