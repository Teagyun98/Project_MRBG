using UnityEngine;

public enum MonsterState
{
    Idle,
    Move,
    Hit,
    Skill
}

public interface IMonsterState<T>
{
    void OperateEnter(T sender);
    void OperateExit(T sender);
    void OperateFixedUpdate(T sender);
}

public class StateMachine<T>
{
    private T m_sender;

    public IMonsterState<T> CurState { get; set; }

    public StateMachine(T sender, IMonsterState<T> state)
    {
        // 생성되며 m_sender변수와 기본 상태 세팅
        m_sender = sender;
        SetState(state);
    }

    // 상태 설정 함수
    public void SetState(IMonsterState<T> state)
    {
        // StateMachine이 생성되지 않았거나 이미 바뀔 상태와 같다면 반환
        if (m_sender == null || CurState == state)
            return;

        // 다른 상태에 있었다면 바뀌기 전 상태에서 빠져나오기 위해 Exit함수 실행
        if (CurState != null)
            CurState.OperateExit(m_sender);

        // 상태 변경
        CurState = state;

        // 상태가 변경되었으면 변경된 상태의 Enter 함수 실행
        if (CurState != null)
            CurState.OperateEnter(m_sender);
    }

    public void DoOperateFixedUpdate()
    {
        if (m_sender == null || CurState == null)
            return;

        CurState.OperateFixedUpdate(m_sender);
    }
}

public class MonsterIdle : IMonsterState<MonsterController>
{
    private MonsterController controller;

    public void OperateEnter(MonsterController sender)
    {
        controller = sender;
        controller.Animator.SetBool("Idle", true);
    }

    public void OperateExit(MonsterController sender)
    {
        controller.Animator.SetBool("Idle", false);
    }

    public void OperateFixedUpdate(MonsterController sender) { }
}

public class MonsterMove : IMonsterState<MonsterController>
{
    private MonsterController controller;

    public void OperateEnter(MonsterController sender)
    {
        controller = sender;
        controller.Animator.SetBool("Move", true);
    }

    public void OperateExit(MonsterController sender)
    {
        controller.Animator.SetBool("Move", false);
    }

    public void OperateFixedUpdate(MonsterController sender)
    {
        // 각 몬스터들은 자신이 가지고 있는 endPoint를 향해 나아감
        controller.transform.position = Vector3.MoveTowards(controller.transform.position, controller.Line.endPoint.position, controller.Speed * controller.Gm.GameSpeed);

        // 정해진 지점을 통과 할 때 마다 스킬의 발동 여부와 속도 재설정
        if (controller.transform.position.x > controller.Line.endPoint.position.x - (float)(Mathf.Abs(controller.Line.startPoint.position.x) + Mathf.Abs(controller.Line.endPoint.position.x)) / 3f)
        {
            //3/2
            if (controller.skill == false && controller.Cm.Target == null)
            {
                controller.SetSpeed(Random.Range(0.001f, 0.0015f));
                controller.skill = true;
                if (Random.Range(0, 3) == 0)
                    controller.Sm.SetState(controller.DicState[MonsterState.Skill]);
            }
        }
        else if (controller.transform.position.x > controller.Line.endPoint.position.x - (float)(Mathf.Abs(controller.Line.startPoint.position.x) + Mathf.Abs(controller.Line.endPoint.position.x)) / 2f)
        {
            // 2/1
            if (controller.skill == true)
            {
                controller.SetSpeed(Random.Range(0.001f, 0.0015f));
                controller.skill = false;
            }
        }
        else if(controller.transform.position.x > controller.Line.startPoint.position.x + (float)(Mathf.Abs(controller.Line.startPoint.position.x) + Mathf.Abs(controller.Line.endPoint.position.x)) / 3f)
        {
            // 3/1지점
            if(controller.skill == false && controller.Cm.Target == null)
            {
                controller.SetSpeed(Random.Range(0.001f, 0.0015f));
                controller.skill = true;
                if (Random.Range(0, 3) == 0)
                    controller.Sm.SetState(controller.DicState[MonsterState.Skill]);
            }
        }

        if (controller.transform.position.x == controller.Line.endPoint.position.x)
            controller.Goal();
    }
}

public class MonsterHit : IMonsterState<MonsterController>
{
    private MonsterController controller;

    float hitTime;

    public void OperateEnter(MonsterController sender)
    {
        controller = sender;
        controller.Animator.SetBool("Hit", true);

        hitTime = 10;
    }

    public void OperateExit(MonsterController sender)
    {
        controller.Animator.SetBool("Hit", false);
    }

    public void OperateFixedUpdate(MonsterController sender)
    {
        // 공격을 받으면 일정시간 동안 멈추어야하고 스킬을 사용 중이였다면 풀린다.
        if (hitTime < 0)
            controller.Sm.SetState(controller.DicState[MonsterState.Move]);
        else
            hitTime -= Time.fixedDeltaTime * controller.Gm.GameSpeed;
    }
}


// 각 몬스터마다 구현하게 될 스킬 상태 베이스
public class MonsterSkill : IMonsterState<MonsterController>
{
    private MonsterController controller;

    public void OperateEnter(MonsterController sender)
    {
        controller = sender;
        controller.Animator.SetBool("Skill", true);
    }

    public void OperateExit(MonsterController sender)
    {
        controller.Animator.SetBool("Skill", false);
    }

    public void OperateFixedUpdate(MonsterController sender) { }
}
