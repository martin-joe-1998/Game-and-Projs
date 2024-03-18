using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState, IRootState 
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        IsRootState = true;
    }

    public override void EnterState() {
        InitializeSubState();
        HandleGravity();
    }
    public override void UpdateState() { 
        CheckSwitchStates();
    }
    public override void ExitState() { }
    public override void CheckSwitchStates() { 
        // if player is grounded and jump is pressed, switch to jump state
        if (Ctx.IsJumpPressed && !Ctx.RequireNewJumpPress) {
            SwitchState(Factory.Jump()); 
        } 
        // if player is not grounded and jump is not pressed, switch to fall state
        else if (!Ctx.CharacterController.isGrounded) {
            SwitchState(Factory.Fall());
        }
    }
    public override void InitializeSubState() { 
        if (!Ctx.IsMovementPressed && !Ctx.IsRunPressed) {
            SetSubState(Factory.Idle());
        } else if (Ctx.IsMovementPressed && !Ctx.IsRunPressed) {
            SetSubState(Factory.Walk());
        } else {
            SetSubState(Factory.Run());
        }
    }

    public void HandleGravity()
    {
        Ctx.CurrentMovementY = Ctx.Gravity;
        Ctx.AppliedMovementY = Ctx.Gravity;
    }
}
