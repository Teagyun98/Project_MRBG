using UnityEngine;

public class BoxCat : MonsterController
{
    private void Awake()
    {
        SetSpriteList(Gm.GetAnimSpriteList("BoxCat"));
    }
    public override void Start()
    {
        base.Start();

        SetStateMachine();

        // 말 스킬 추가
        DicState.Add(MonsterState.Skill, new HorseSkill());
    }

    public override void SkillEnd() { }
}

public class BoxCatSkill : IMonsterState<MonsterController>
{
    private MonsterController controller;

    public void OperateEnter(MonsterController sender)
    {
        controller = sender;
        controller.Animator.SetBool("Skill", true);
        controller.SetSpeed(Random.Range(0.001f, 0.003f));
        controller.Cm.FocusCamera(controller);
    }

    public void OperateExit(MonsterController sender)
    {
        controller.Animator.SetBool("Skill", false);
        controller.SetSpeed(Random.Range(0.001f, 0.0015f));
    }

    public void OperateFixedUpdate(MonsterController sender)
    {
        controller.transform.position = Vector2.MoveTowards(controller.transform.position, controller.Line.endPoint.position, controller.Speed * controller.Gm.GameSpeed);

        if (controller.transform.position.x == controller.Line.endPoint.position.x)
            controller.Goal();
    }
}
