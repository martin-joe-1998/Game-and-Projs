using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState 
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) { }

    public override void EnterState() {
        Ctx.Animator.SetBool(Ctx.IsWalkingHash, false);
        Ctx.Animator.SetBool(Ctx.IsRunningHash, false);
        Ctx.Animator.SetBool(Ctx.IsFreeidlingHash, false);
        Ctx.AppliedMovementX = 0;
        Ctx.AppliedMovementZ = 0;

        Ctx.Timer.StartTimer();
    }

    public override void UpdateState() {
        CheckSwitchStates();
    }

    public override void ExitState() {
        Ctx.Timer.StopTimer();
    }

    public override void CheckSwitchStates() { 
        if (Ctx.IsMovementPressed && Ctx.IsRunPressed) {
            SwitchState(Factory.Run());
        } else if (Ctx.IsMovementPressed) {
            SwitchState(Factory.Walk());
        } else if (Ctx.Timer.currentTime >= 10) {
            SwitchState(Factory.Freeidle());
        }
    }

    public override void InitializeSubState() { }
}
