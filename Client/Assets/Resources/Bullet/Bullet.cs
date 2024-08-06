using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float Damage;
    public GameObject Shooter;
    public GameObject Target;
    public float Speed;
    public bool IsHit;
    public GameObject flash; // 총알 발사 이펙트 프리팹
    public GameObject hit; // 타겟 적중 이펙트 프리팹

    public GameObject flashInstance;
    public GameObject hitInstance;
    private Coroutine targetingCoroutine;

    public GameObject BulletPool;

    private GameObject BulletTR;

    public float HitYOffset = 0.5f;
    public void Init()
    {
        // 이펙트 인스턴스를 미리 생성하고 비활성화
        if (flash != null)
        {
            flashInstance = Instantiate(flash, BulletPool.transform);
            flashInstance.SetActive(false);
        }

        if (hit != null)
        {
            hitInstance = Instantiate(hit, BulletPool.transform);
            hitInstance.SetActive(false);
        }
    }

    private void OnEnable()
    {
        IsHit = false; // 총알이 활성화될 때 초기화
        targetingCoroutine = StartCoroutine(Targeting());


    }

    private void OnDisable()
    {
        // 총알이 비활성화될 때 타겟 초기화하지 않음
        if (targetingCoroutine != null)
        {
            StopCoroutine(targetingCoroutine);
        }
    }
    WaitForSeconds wait = new WaitForSeconds(0.01f);
    private IEnumerator Targeting()
    {
        yield return wait;
        // Flash 이펙트 활성화
        if (flashInstance != null)
        {
            flashInstance.SetActive(false);
            flashInstance.transform.position = Shooter.GetComponentInChildren<BulletTR>().transform.position;
            flashInstance.SetActive(true);
            if (Target != null)
            {
                flashInstance.transform.LookAt(Target.transform);
            }

        }
        while (true)
        {
            if (Target == null)
            {
                // 타겟이 할당되지 않은 경우 즉시 반환
                Debug.LogWarning("Target is null, deactivating bullet.");
                gameObject.SetActive(false);
                yield break;
            }

            if (Target.activeInHierarchy)
            {
                // 타겟의 위치에 오프셋 추가 (예: y축으로 1.0f 만큼 위로 이동)
                Vector3 targetPositionWithOffset = Target.transform.position + new Vector3(0, HitYOffset, 0);

                // 타겟을 향해 이동
                Vector3 direction = (targetPositionWithOffset - transform.position).normalized;
                transform.position += direction * Speed * Time.deltaTime;
                transform.LookAt(targetPositionWithOffset);

                // 타겟에 도달했는지 확인
                //if (Vector3.Distance(transform.position, targetPositionWithOffset) < 0.1f)
                //{
                //    OnHitTarget();
                //}
            }
            else
            {
                // 타겟이 비활성화된 경우 총알을 비활성화
                Debug.LogWarning("Target is inactive, deactivating bullet.");
                gameObject.SetActive(false);
                yield break;
            }

            yield return null; // 다음 프레임까지 대기
        }
    }

    private Vector3 hitPosition; // 충돌 위치를 저장할 변수

    private void OnTriggerEnter(Collider other)
    {
        // 적과 충돌했는지 확인
        if (other.gameObject == Target)
        {
            // 충돌 위치 저장
            hitPosition = other.ClosestPointOnBounds(transform.position);
            OnHitTarget();
        }
    }

    private void OnHitTarget()
    {
        // 타겟에 충돌했을 때 처리
        IsHit = true;

        // Hit 이펙트 활성화
        if (hitInstance != null)
        {
            hitInstance.SetActive(false);
            // 저장한 충돌 위치를 사용
            hitInstance.transform.position = hitPosition;
            hitInstance.SetActive(true);
            Target.GetComponent<Character>().Damaged(Shooter.GetComponent<Character>().PlayerNum);
            // 일정 시간 후에 비활성화
        }

        gameObject.SetActive(false); // 총알 비활성화
    }
    WaitForSeconds deActivated = new WaitForSeconds(0.5f);
}
