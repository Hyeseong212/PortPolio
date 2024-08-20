using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Character : MonoBehaviour
{
    public long Uid; // 사용하는 유저
    public int PlayerNum;
    public int CharacterId; // 어떤 캐릭터인지
    public int Level; // 캐릭터 레벨
    public int BulletCount;
    public float TotalHP; // 캐릭터 HP
    public float CurrentHP; // 캐릭터 HP
    public float TotalMP; // 캐릭터 MP
    public float CurrentMP; // 캐릭터 MP
    public bool IsHit; // 캐릭터가 맞았는지
    public GameObject Target; // 선택된 오브젝트를 저장할 변수
    public GameObject BulletPoolObject;
    public GameObject BulletTR;
    public CharacterData CharacterData;
    public Material outlineSelected;
    public Material outlineClicked;

    private float rotationSpeed = 15f; // 회전 속도
    private bool isDead = false;
    private List<GameObject> bulletPool = new List<GameObject>();

    private Material[] originalMaterials;
    private Renderer renderers;
    private AnimatorOverrideController overrideController;
    private SkinnedMeshRenderer skinnedMeshRenderer;

    public Character()
    {
        Uid = 0;
        PlayerNum = -1;
        CharacterId = -1;
        CurrentHP = 0;
        TotalHP = 0;
        CurrentMP = 0;
        TotalMP = 0;
        IsHit = false;
        Level = 1;
        CharacterData = new CharacterData();
    }
    private void Start()
    {

    }
    void Update()
    {
        RotateTowardsTarget();
    }
    public void Attack()
    {
        if (Target != null)
        {
            GetComponentInChildren<AnimatorController>().CurrentStatus = CharacterStatus.ATTACK;
        }
        else
        {
            Debug.LogWarning("No target to attack.");
        }
    }

    private void RotateTowardsTarget()
    {
        if (Target != null && GetComponentInChildren<AnimatorController>().CurrentStatus == CharacterStatus.ATTACK)
        {
            Vector3 direction = (Target.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }
    public void SetCharacterModel(int characterID)
    {
        // 모델을 로드할 경로 설정
        string modelPath = $"Character/Character{characterID}";

        // Resources 폴더에서 모델 로드
        GameObject characterPrefab = Resources.Load<GameObject>(modelPath);

        if (characterPrefab != null)
        {
            // 기존 자식 오브젝트 삭제 (필요한 경우)
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            // 모델 인스턴스화 및 현재 게임 오브젝트의 자식으로 설정
            GameObject characterInstance = Instantiate(characterPrefab, transform);
            SetCharacterAnimator(characterID);
            // AnimatorController 설정
            Animator animator = characterInstance.GetComponent<Animator>();
            var animatorControllerPath = SetCharacterAnimator(characterID);
            RuntimeAnimatorController runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(animatorControllerPath);
            if (animator != null && runtimeAnimatorController != null)
            {
                overrideController = new AnimatorOverrideController(runtimeAnimatorController);
                animator.runtimeAnimatorController = overrideController;

                // AnimatorController 추가 및 설정
                AnimatorController animatorController = characterInstance.AddComponent<AnimatorController>();
                animatorController.SetAnimator(animator, this);
                // 애니메이션 이벤트 설정
                AnimationClip[] clips = overrideController.animationClips;
                foreach (var clip in clips)
                {
                    SetCharacterAttackAnimation(clip, animatorController);
                }
            }
            else
            {
                Debug.LogWarning("Animator component or RuntimeAnimatorController not found.");
            }

            Debug.Log($"Character model with ID {characterID} has been loaded and set.");
        }
        else
        {
            Debug.LogError($"Character model with ID {characterID} could not be found in path: {modelPath}");
        }
        BulletTR = GetComponentInChildren<BulletTR>().gameObject;
        GetComponentInChildren<AnimatorController>().CurrentStatus = CharacterStatus.IDLE;
        outlineSelected = new Material(Shader.Find("Draw/OutlineShaderSelected"));
        outlineClicked = new Material(Shader.Find("Draw/OutlineShaderClicked"));
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        renderers = skinnedMeshRenderer.GetComponent<Renderer>();
        originalMaterials = renderers.sharedMaterials;
    }
    private string SetCharacterAnimator(int characterID)
    {
        switch (characterID)
        {
            case 0:
                return $"Animation/Character{characterID}/Rabby_Animations_Check";
            case 1:
                return $"Animation/Character{characterID}/Rabby_Animations_Check";
            case 2:
                return $"Animation/Character{characterID}/Beez_Animations_Check";
            case 3:
                return $"Animation/Character{characterID}/Flower Dryad_Animations_Check";
            case 4:
                return $"Animation/Character{characterID}/Planta_Animations_Check";
            default:
                return "not thing";
        }
    }
    private void SetCharacterAttackAnimation(AnimationClip clip , AnimatorController animatorController)
    {
        if (clip.name == "Anim_Rabby_Attack")
        {
            animatorController.AddAnimationEvent(clip, "AttackMotion");
        }
        else if (clip.name == "Anim_Beez_Attack")
        {
            animatorController.AddAnimationEvent(clip, "AttackMotion");
        }
        else if (clip.name == "Planta_Attack")
        {
            animatorController.AddAnimationEvent(clip, "AttackMotion");
        }
        else if (clip.name == "Anim_Dryad_Aatack_Beam_Cannon")
        {
            animatorController.AddAnimationEvent(clip, "AttackMotion");
        }
    }

    public void SetCharacterData(CharacterData characterData)
    {
        CharacterData = characterData;
        CharacterId = characterData.CharacterId;
        TotalHP = characterData.TotalHP + (Level - 1 * characterData.HPPerLv);
        SetCurrentHP(TotalHP);
        TotalMP = characterData.TotalMP + (Level - 1 * characterData.MPPerLv);
        SetCurrentMP(TotalMP);
        CharacterData.Bullet = CharacterDataManager.Instance.GetBullet(characterData.BulletId);
        for (int i = 0; i < BulletCount; i++)
        {
            var bullet = Instantiate(CharacterData.Bullet, BulletPoolObject.transform);
            bullet.transform.position = new Vector3(BulletTR.transform.position.x, BulletTR.transform.position.y + 0.25f, BulletTR.transform.position.z);
            var bulletStat = bullet.GetComponent<Bullet>();
            bulletStat.BulletPool = this.BulletPoolObject;
            bulletStat.Shooter = this.gameObject;
            bulletStat.Damage = CharacterData.Attack + (CharacterData.AttackPerLv * (Level - 1));
            bulletStat.Init();
            bullet.SetActive(false);
            bulletPool.Add(bullet);
        }
    }
    public void SetLevel(int level)
    {
        this.Level = level;
        this.CurrentHP = TotalHP + ((level - 1) * CharacterData.HPPerLv);
        this.TotalHP = TotalHP + ((level - 1) * CharacterData.HPPerLv);
        this.CurrentMP = TotalMP + ((level - 1) * CharacterData.MPPerLv);
        this.TotalMP = TotalMP + ((level - 1) * CharacterData.MPPerLv);
    }
    public void SetCurrentHP(float currentHP)
    {
        CurrentHP = (int)currentHP;
    }

    public void SetCurrentMP(float currentMP)
    {
        CurrentMP = (int)currentMP;
    }

    public void FireBullet()
    {
        GameObject bullet = bulletPool.Find(b => !b.activeInHierarchy);
        if (bullet != null)
        {
            bullet.SetActive(true);
            bullet.transform.position = new Vector3(BulletTR.transform.position.x , BulletTR.transform.position.y, BulletTR.transform.position.z);
            var bulletStat = bullet.GetComponent<Bullet>();
            bulletStat.Target = Target; // 타겟 설정
            bulletStat.BulletPool = this.BulletPoolObject;
            bulletStat.IsHit = false; // 초기화
            // 다시 한 번 타겟 설정 로그 추가
        }
        else
        {
            Debug.LogWarning("No inactive bullets in the pool.");
        }
    }


    public void ApplyOutlineShader(Material outlineShader)
    {
        Material[] materials = new Material[originalMaterials.Length + 1];
        Array.Copy(originalMaterials, materials, originalMaterials.Length);
        for (int i = originalMaterials.Length; i > 1; i--)
        {
            materials[i] = materials[i - 1];
        }
        materials[1] = outlineShader;
        renderers.materials = materials;
    }

    public void RemoveOutlineShader()
    {
        List<Material> materials = new List<Material>(renderers.sharedMaterials);
        materials.Remove(outlineSelected);
        materials.Remove(outlineClicked);
        renderers.materials = materials.ToArray();
    }

    private void OnMouseEnter()
    {
        if (!isDead)
        {
            ApplyOutlineShader(outlineSelected);
        }
    }

    private void OnMouseExit()
    {
        if (!isDead)
        {
            RemoveOutlineShader();
        }
    }
    private void OnMouseDown()
    {
        if (!isDead)
        {
            ApplyOutlineShader(outlineClicked);
        }
    }
    private void OnMouseUp()
    {
        if (!isDead)
        {
            RemoveOutlineShader();
            ApplyOutlineShader(outlineSelected);
        }
    }
    public void Damaged(int ShooterPlayerNumber)
    {
        GetComponentInChildren<AnimatorController>().CurrentStatus = CharacterStatus.DAMAGED;

        if(ShooterPlayerNumber == InGameManager.Instance.selectedCharacter.PlayerNum)
        {
            int length = 0x01 + Utils.GetLength(ShooterPlayerNumber) + Utils.GetLength(PlayerNum);

            Packet imHit = new Packet();
            imHit.push((byte)InGameProtocol.GameInfo);
            imHit.push(length);
            imHit.push((byte)GameInfo.IsHit);
            imHit.push(ShooterPlayerNumber);
            imHit.push(PlayerNum);//맞았을때 처리 하도록

            InGameTCPController.Instance.SendToInGameServer(imHit);
        }
    }

}
