using System.Collections.Generic;
using UnityEngine;

public class Slime : MonsterController
{
    private void Awake()
    {
        SetSpriteList(Gm.GetAnimSpriteList("Slime"));
    }

    public override void Start()
    {
        base.Start();

        SetStateMachine();

        // 슬라임 스킬 추가
        DicState.Add(MonsterState.Skill, new WarmSkill());
    }

    public override void Skill_2()
    {
        base.Skill_2();

        if (Random.Range(0, 10) == 0)
        {
            foreach (MonsterController monster in Gm.OtherMonsterList(this))
                monster.transform.position = monster.Line.startPoint.transform.position;
        }
        else
        {
            List<float> posXList = new List<float>();

            foreach (MonsterController monster in Gm.RunningMonsterList())
                posXList.Add(monster.transform.position.x);

            foreach (MonsterController monster in Gm.RunningMonsterList())
            {
                float randPosX = posXList[Random.Range(0, posXList.Count)];
                monster.transform.position = new Vector3(randPosX, monster.transform.position.y, monster.transform.position.z);
                posXList.Remove(randPosX);
            }
        }
    }
}

public class SlimeSkill : IMonsterState<MonsterController>
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
