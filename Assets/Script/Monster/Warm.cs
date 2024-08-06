using System.Collections.Generic;
using UnityEngine;

public class Warm : MonsterController
{
    private void Awake()
    {
        SetSpriteList(Gm.GetAnimSpriteList("Warm"));

    }

    public override void Start()
    {
        base.Start();

        SetStateMachine();

        // 지렁이 스킬 추가
        DicState.Add(MonsterState.Skill, new WarmSkill());
    }

    public override void Skill_2()
    {
        base.Skill_2();

        transform.position = transform.position + new Vector3(1f, 0, 0);
    }
}

public class WarmSkill : IMonsterState<MonsterController>
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
