using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float Damage;
    public GameObject Shooter;
    public GameObject Target;
    public float Speed;
    public bool IsHit;
    public GameObject flash; // �Ѿ� �߻� ����Ʈ ������
    public GameObject hit; // Ÿ�� ���� ����Ʈ ������

    public GameObject flashInstance;
    public GameObject hitInstance;
    private Coroutine targetingCoroutine;

    public GameObject BulletPool;

    private GameObject BulletTR;

    public float HitYOffset = 0.5f;
    public void Init()
    {
        // ����Ʈ �ν��Ͻ��� �̸� �����ϰ� ��Ȱ��ȭ
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
        IsHit = false; // �Ѿ��� Ȱ��ȭ�� �� �ʱ�ȭ
        targetingCoroutine = StartCoroutine(Targeting());


    }

    private void OnDisable()
    {
        // �Ѿ��� ��Ȱ��ȭ�� �� Ÿ�� �ʱ�ȭ���� ����
        if (targetingCoroutine != null)
        {
            StopCoroutine(targetingCoroutine);
        }
    }
    WaitForSeconds wait = new WaitForSeconds(0.01f);
    private IEnumerator Targeting()
    {
        yield return wait;
        // Flash ����Ʈ Ȱ��ȭ
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
                // Ÿ���� �Ҵ���� ���� ��� ��� ��ȯ
                Debug.LogWarning("Target is null, deactivating bullet.");
                gameObject.SetActive(false);
                yield break;
            }

            if (Target.activeInHierarchy)
            {
                // Ÿ���� ��ġ�� ������ �߰� (��: y������ 1.0f ��ŭ ���� �̵�)
                Vector3 targetPositionWithOffset = Target.transform.position + new Vector3(0, HitYOffset, 0);

                // Ÿ���� ���� �̵�
                Vector3 direction = (targetPositionWithOffset - transform.position).normalized;
                transform.position += direction * Speed * Time.deltaTime;
                transform.LookAt(targetPositionWithOffset);

                // Ÿ�ٿ� �����ߴ��� Ȯ��
                //if (Vector3.Distance(transform.position, targetPositionWithOffset) < 0.1f)
                //{
                //    OnHitTarget();
                //}
            }
            else
            {
                // Ÿ���� ��Ȱ��ȭ�� ��� �Ѿ��� ��Ȱ��ȭ
                Debug.LogWarning("Target is inactive, deactivating bullet.");
                gameObject.SetActive(false);
                yield break;
            }

            yield return null; // ���� �����ӱ��� ���
        }
    }

    private Vector3 hitPosition; // �浹 ��ġ�� ������ ����

    private void OnTriggerEnter(Collider other)
    {
        // ���� �浹�ߴ��� Ȯ��
        if (other.gameObject == Target)
        {
            // �浹 ��ġ ����
            hitPosition = other.ClosestPointOnBounds(transform.position);
            OnHitTarget();
        }
    }

    private void OnHitTarget()
    {
        // Ÿ�ٿ� �浹���� �� ó��
        IsHit = true;

        // Hit ����Ʈ Ȱ��ȭ
        if (hitInstance != null)
        {
            hitInstance.SetActive(false);
            // ������ �浹 ��ġ�� ���
            hitInstance.transform.position = hitPosition;
            hitInstance.SetActive(true);
            Target.GetComponent<Character>().Damaged(Shooter.GetComponent<Character>().PlayerNum);
            // ���� �ð� �Ŀ� ��Ȱ��ȭ
        }

        gameObject.SetActive(false); // �Ѿ� ��Ȱ��ȭ
    }
    WaitForSeconds deActivated = new WaitForSeconds(0.5f);
}
