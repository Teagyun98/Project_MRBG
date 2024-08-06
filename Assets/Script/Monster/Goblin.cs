using UnityEngine;

public class Goblin : MonsterController
{
    public override void Start()
    {
        base.Start();

        SetStateMachine();

        // 지렁이 스킬 추가
        DicState.Add(MonsterState.Skill, new GoblinSkill());
    }

    public void Skill()
    {
        MonsterController first = Gm.FirstMonster();

        if(first == null || first == this)
            transform.position = transform.position + new Vector3(0.5f, 0, 0);
        else
        {
            float firstPosX = first.transform.position.x;

            first.transform.position = new Vector3(transform.position.x, first.transform.position.y, first.transform.position.z);
            transform.position = new Vector3(firstPosX, transform.position.y, transform.position.z);
        }
    }
}

public class GoblinSkill : IMonsterState<MonsterController>
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
