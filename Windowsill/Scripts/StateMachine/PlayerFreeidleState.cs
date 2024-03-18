using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFreeidleState : PlayerBaseState 
{
    public PlayerFreeidleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) { }

    int randomNumber;

    public override void EnterState()
    {
        //Debug.Log("Free Idle State!");
        SpawnRandomNumber();                                         // ランダムな数字を生成して、その数字でアイドル動画の種類を決める
        Ctx.Animator.SetBool(Ctx.IsFreeidlingHash, true);            // Free Idle状態に入ったことを示すパラメーター
        Ctx.Animator.SetInteger(Ctx.RandomFreeidle, randomNumber);   // ランダムな数字をパラメーターに与える
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        
    }

    public override void ExitState() {
        Ctx.Animator.SetInteger(Ctx.RandomFreeidle, 0);               // アイドル動画の種類を決めるパラメーターを0で初期化する
    }

    public override void CheckSwitchStates()
    {
        if (Ctx.IsMovementPressed || Ctx.IsJumpPressed) {
            SwitchState(Factory.Idle());
        } else {
            MotionLoop();
        }
    }

    public override void InitializeSubState() { }

    public void SpawnRandomNumber() {
        randomNumber = Random.Range(1, 7); // 生成1到6之间的随机整数
        //randomNumber = 6;
        //Debug.Log("Random Number: " + randomNumber);
    }

    private void MotionLoop()
    {
        if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Sad Idle")) {
            if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 3) {
                SwitchState(Factory.Idle());
            }
        } 
        else if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Happy Idle")) {
            if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 3) {
                SwitchState(Factory.Idle());
            }
        }
        // Sit Up StateMachine
        else if (randomNumber == 3) {
            if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Sit Up")) {
                if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 3) {
                    Ctx.Animator.Play("Sit Up To Idle", 0, 0f);
                }
            } else if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Sit Up To Idle")) {
                  if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) {
                      SwitchState(Factory.Idle());
                  }
            }
        }
        // Push Up StateMachine
        else if (randomNumber == 4) {  
            if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Push Up")) {
                if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 3) {
                    Ctx.Animator.Play("Push Up To Idle", 0, 0f);
                }
            } else if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Push Up To Idle")) {
                if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) {
                    SwitchState(Factory.Idle());
                }
            }
        }
        // Look Around 1 & 2
        else if (randomNumber == 5 || randomNumber == 6) {
            if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) {
                SwitchState(Factory.Idle());
            }
        }
        
    }
}
