using UnityEngine;

public class Worm : MonsterController
{
    public override void Start()
    {
        base.Start();

        SetStateMachine();

        // 지렁이 스킬 추가
        DicState.Add(MonsterState.Skill, new WormSkill());
    }

    public void Skill()
    {
        transform.position = transform.position + new Vector3(1f, 0, 0);
    }
}

public class WormSkill : IMonsterState<MonsterController>
{
    private MonsterController controller;

    public void OperateEnter(MonsterController sender)
    {
        controller = sender;
        controller.Animator.SetBool("Skill", true);
        controller.Cm.FocusCamera(controller);
    }

    public void OperateExit(MonsterController sender)
    {
        controller.Animator.SetBool("Skill", false);
    }

    public void OperateFixedUpdate(MonsterController sender) { }
}
