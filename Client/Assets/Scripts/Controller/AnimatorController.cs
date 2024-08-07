using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SharedCode.Model;

public class AnimatorController : MonoBehaviour
{
    public CharacterStatus currentStatus
    {
        get { return _currentStatus; }
        set
        {
            if (_currentStatus == value) return; // 같은 값이면 무시
            _currentStatus = value;
            OnStatusChanged();
        }
    }

    private Animator animator;
    private Character character;
    private WaitForSeconds wait = new WaitForSeconds(0.3f);
    private CharacterStatus _currentStatus;

    public void SetAnimator(Animator anim, Character charac)
    {
        animator = anim;
        character = charac;
    }
    private void OnStatusChanged()
    {
        Packet characterAnimation = new Packet();

        int length = 0;

        if (currentStatus == CharacterStatus.ATTACK)
        {
            length = 0x01 + 0x01 + Utils.GetLength(character.PlayerNum) + 0x04;
        }
        else
        {
            length = 0x01 + 0x01 + Utils.GetLength(character.PlayerNum);
        }

        characterAnimation.push((byte)InGameProtocol.GameInfo);
        characterAnimation.push(length);
        characterAnimation.push((byte)GameInfo.CharacterAnimationStatus);
        characterAnimation.push((byte)currentStatus);//애니메이션상태
        characterAnimation.push((byte)character.PlayerNum);//어떤 캐릭터의
        if (currentStatus == CharacterStatus.ATTACK)//만약 공격이면
        {
            characterAnimation.push(character.Target.GetComponent<Character>().PlayerNum);//그타겟의 플레이어 넘버 보내기
        }

        InGameTCPController.Instance.SendToInGameServer(characterAnimation);
        if (currentStatus == CharacterStatus.ATTACK)
        {
            SetAttackSpeed(character.CharacterData.AttackRate + (character.Level - 1) * character.CharacterData.AttackRatePerLv);
        }
        else
        {
            animator.speed = 1f; // 다른 상태에서는 기본 속도로 설정
        }
        animator.SetInteger("animation", (int)currentStatus);
        if(currentStatus == CharacterStatus.DAMAGED)
            HandleDamaged();
    }
    private void HandleDamaged()
    {
        // 애니메이션 이벤트나 코루틴을 통해 애니메이션이 끝난 후 상태를 IDLE로 변경
        StartCoroutine(WaitAndTransitionToIdle());
    }
    private IEnumerator WaitAndTransitionToIdle()
    {
        // 대기 시간을 애니메이션 길이로 설정
        yield return wait;
        currentStatus = CharacterStatus.IDLE;
    }
    public void SetAttackSpeed(float attackRate)
    {
        animator.speed = attackRate;
    }

    public void AddAnimationEvent(AnimationClip clip, string functionName)
    {
        AnimationEvent animationEvent = new AnimationEvent();
        animationEvent.time = 4f / clip.frameRate; // 5 프레임 이후에 이벤트 추가
        animationEvent.functionName = functionName;
        clip.AddEvent(animationEvent);
    }
    // 애니메이션 이벤트에서 호출할 함수
    public void AttackMotion()
    {
        character?.FireBullet();
    }

}
