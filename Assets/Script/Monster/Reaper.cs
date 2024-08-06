public class Reaper : MonsterController
{
    public override void Start()
    {
        base.Start();

        SetStateMachine();

        SetSpriteList(Gm.GetAnimSpriteList("Reaper"));

        // 리퍼 스킬 추가
        DicState.Add(MonsterState.Skill, new ReaperSkill());
    }

    public override void Skill_2()
    {
        base.Skill_2();

        foreach (MonsterController monster in Gm.OtherMonsterList(this))
            monster.Sm.SetState(monster.DicState[MonsterState.Hit]);
    }
}

public class ReaperSkill : IMonsterState<MonsterController>
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
