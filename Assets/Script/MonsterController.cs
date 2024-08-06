using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MonsterController : MonoBehaviour
{
    public GameManager Gm { get; private set; }
    public CameraMove Cm { get; private set; }

    [Inject]
    public void Construct(GameManager _gameManager) => Gm = _gameManager;
    [Inject]
    public void Construct(CameraMove _cameraMove) => Cm = _cameraMove;

    public StateMachine<MonsterController> Sm { get; private set; }
    public Dictionary<MonsterState, IMonsterState<MonsterController>> DicState { get; private set; }

    public Line Line { get; private set; }
    public float Speed { get; private set; }
    public Animator Animator { get; private set; }

    private SpriteRenderer spr;
    [SerializeField] private Sprite icon;

    public bool skill;

    public virtual void Start()
    {
        Animator = GetComponent<Animator>();
        spr = GetComponent<SpriteRenderer>();

        Animator.speed = Gm.GameSpeed;
    }

    public virtual void Update()
    {
        // 몬스터가 endLine에 도착하는 것보다 조금 빠른 시점에 레이스 도착 지점을 만들어야 하여 따로 레이스의 Goal지점을 설정
        if (Gm.Race == true && Gm.RankingList.Contains(this) == false && Line.endPoint.position.x - transform.position.x < 0.1f)
            Gm.Goal(this);
    }

    private void FixedUpdate()
    {
        Sm.DoOperateFixedUpdate();
    }

    public void SetStateMachine()
    {
        DicState = new Dictionary<MonsterState, IMonsterState<MonsterController>>
        {
            { MonsterState.Idle, new MonsterIdle()},
            { MonsterState.Move, new MonsterMove()},
            { MonsterState.Hit, new MonsterHit()}
        };

        Sm = new StateMachine<MonsterController>(this, DicState[MonsterState.Idle]);
    }

    public virtual void SetLine(Line line)
    {
        Line = line;
        skill = false;

        SetSpeed(Random.Range(0.001f, 0.002f));

        // 시작점으로 포지션 이동
        transform.position = Line.startPoint.position;

        if(Animator != null)
            Animator.speed = Gm.GameSpeed;

        // 캐릭터 활성화
        gameObject.SetActive(true);
    }

    public void Go()
    {
        Sm.SetState(DicState[MonsterState.Move]);
    }

    public virtual void SetSpeed(float speed)
    {
        Speed = speed;
    }

    public void Goal()
    {
        Sm.SetState(DicState[MonsterState.Idle]);
    }

    public virtual void SkillEnd()
    {
        Sm.SetState(DicState[MonsterState.Move]);
    }

    public Sprite GetIcon()
    {
        return icon;
    }

    public virtual void SendMessage() { }
}
