using UnityEngine;

public class Dice : MonsterController
{
    private void Awake()
    {
        SetSpriteList(Gm.GetAnimSpriteList("Dice"));
    }

    public override void Start()
    {
        base.Start();

        SetStateMachine();
        SetSpeed(0);

        // 주사위 스킬 추가
        DicState.Add(MonsterState.Skill, new DiceSkill());
    }

    public override void Update()
    {
        if (Gm.Race == true && Line.endPoint.position.x - Gm.FirstMonsterPosX(true) < 0.2f && skill == false)
        {
            skill = true;

            if (Random.Range(0, 5) == 0)
                Sm.SetState(DicState[MonsterState.Skill]);
        }
    }

    public override void SetLine(Line line)
    {
        base.SetLine(line);
        SetSpeed(0);
        skill = false;
    }

    public override void SetSpeed(float speed)
    {
        return;
    }

    public override void SendMessage()
    {
        if (Gm.RankingList.Count == 5 || Random.Range(0, 10) == 0 && Gm.RankingList.Contains(this) == false)
        {
            Sm.SetState(DicState[MonsterState.Skill]);
        }
    }
}

public class DiceSkill : IMonsterState<MonsterController>
{
    private MonsterController controller;

    public void OperateEnter(MonsterController sender)
    {
        controller = sender;
        controller.Animator.SetBool("Skill", true);

        controller.transform.position = controller.Line.endPoint.position;
        controller.Gm.Goal(controller);
        controller.Cm.FocusCamera(controller);
    }

    public void OperateExit(MonsterController sender)
    {
        controller.Animator.SetBool("Skill", false);
    }

    public void OperateFixedUpdate(MonsterController sender) { }
}
