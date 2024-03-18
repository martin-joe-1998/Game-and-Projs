using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterSound : MonoBehaviour
{
    private Animator animator;

    private float epsilon = 0.005f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Walk") || stateInfo.IsName("Run")) {
            float normalizedDecimal = stateInfo.normalizedTime - Mathf.Floor(stateInfo.normalizedTime);
            float normalizedTimeOne_Walk = Mathf.Abs(normalizedDecimal - 0.3f);
            float normalizedTimeTwo_Walk = Mathf.Abs(normalizedDecimal - 0.8f);

            Debug.Log(normalizedDecimal);
            if (normalizedTimeOne_Walk < epsilon || normalizedTimeTwo_Walk < epsilon) {
                float volume = 1f;
                SoundManager.Instance.PlayFootstepsSound(gameObject.transform.position, volume);
            }
        } 
    }
}
