using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    public Camera followedCam;
    public float speed = 5f;
    public float rotationSpeed = 10f; // 회전 속도
    public float distanceThreshold = 10f; // 거리 임계값
    private float wallOffset = -0.5f; // 벽에서 떨어진 거리
    private List<Node> path;
    private Vector3 finalTargetPosition;
    private int targetIndex;
    public LayerMask floorLayer; // 바닥 레이어 마스크
    public LayerMask obstacleLayer; // 장애물 레이어 마스크

    private bool isMoving = false; // 캐릭터 이동 상태

    public AnimatorController m_animatorController;
    Character m_character;
    Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        m_character = GetComponent<Character>();
    }
    public void Init()
    {
        speed = 5f;
        rotationSpeed = 10f;
        distanceThreshold = 10f;
        wallOffset = 0.5f;
    }

    void FixedUpdate()
    {

        if (m_animatorController == null)
        {
            return;
        }
        HandleMouseInput();
        if (isMoving)
        {
            HandleMovement();
            m_animatorController.currentStatus = CharacterStatus.MOVE;
            m_character.Target = null;
        }
        else if (m_animatorController.currentStatus != CharacterStatus.ATTACK)
        {
            StopMovement();
            m_animatorController.currentStatus = CharacterStatus.IDLE;
        }

        if (Input.GetKey(KeyCode.S))
        {
            isMoving = false;
            StopMovement();
            m_animatorController.currentStatus = CharacterStatus.IDLE;
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButton(1)) // 오른쪽 마우스 버튼 클릭
        {
            Ray ray = followedCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorLayer | obstacleLayer))
            {
                SetTargetPosition(hit.point, hit);
            }
        }
        if (Input.GetMouseButton(0))
        {
            if (InGameManager.Instance.selectedCharacter.Target != null)
            {
                isMoving = false;
                StopMovement();
            }
        }
    }

    private void SetTargetPosition(Vector3 targetPosition, RaycastHit hit)
    {
        float characterY = transform.position.y;

        if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
        {
            // 충돌한 물체가 장애물일 경우, 벽 뒤에 있는 노드를 찾음
            Node closestNodeBehindWall = FindNodeBehindWall(hit.point, hit.normal);
            if (closestNodeBehindWall != null)
            {
                CalculatePathToNode(closestNodeBehindWall);
            }
            else
            {
                // 벽에서 일정 거리 떨어진 지점을 목표로 설정
                finalTargetPosition = hit.point - hit.normal * wallOffset;
                finalTargetPosition.y = characterY; // Y축 고정
                isMoving = true; // 이동 시작
            }
        }
        else
        {
            float distance = Vector3.Distance(transform.position, targetPosition);
            RaycastHit obstacleHit;
            bool hasObstacle = Physics.Raycast(transform.position, (targetPosition - transform.position).normalized, out obstacleHit, distance, obstacleLayer);

            if (distance <= distanceThreshold && !hasObstacle)
            {
                // 임계값 이하이고 장애물이 없는 경우 직선으로 이동
                finalTargetPosition = targetPosition;
                path = null; // A* 경로 초기화
                isMoving = true; // 이동 시작
            }
            else
            {
                // 임계값 초과이거나 장애물이 있는 경우 A* 알고리즘 사용
                CalculatePathToTarget(targetPosition);
            }
        }
    }

    private Node FindNodeBehindWall(Vector3 hitPoint, Vector3 hitNormal)
    {
        // 노드들을 가져와서 벽 뒤에 있는 가장 가까운 노드를 찾음
        Node[] nodes = FindObjectsOfType<Node>();
        float closestDistance = Mathf.Infinity;
        Node closestNode = null;

        foreach (Node node in nodes)
        {
            // 노드가 벽 뒤에 있는지 확인
            if (Vector3.Dot((node.Position - hitPoint).normalized, hitNormal) < 0)
            {
                float distance = Vector3.Distance(hitPoint, node.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNode = node;
                }
            }
        }

        return closestNode;
    }
    private void CalculatePathToNode(Node node)
    {
        AStarPathfinding pathfinding = GetComponent<AStarPathfinding>();
        path = pathfinding.FindPath(transform.position, node.Position);
        finalTargetPosition = node.Position; // 최종 목표 위치 설정
        finalTargetPosition.y = transform.position.y; // Y축 고정
        targetIndex = 0;
        isMoving = true; // 이동 시작
        //Debug.Log("Path found: " + path.Count + " nodes to node behind wall");
    }

    private void CalculatePathToTarget(Vector3 targetPosition)
    {
        AStarPathfinding pathfinding = GetComponent<AStarPathfinding>();
        Node closestNode = pathfinding.FindClosestNode(targetPosition);
        path = pathfinding.FindPath(transform.position, closestNode.Position);
        finalTargetPosition = targetPosition; // 최종 목표 위치 설정
        finalTargetPosition.y = transform.position.y; // Y축 고정
        targetIndex = 0;
        isMoving = true; // 이동 시작
        //Debug.Log("Path found: " + path.Count + " nodes to target position");
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = finalTargetPosition;

        // 경로가 있는 경우 A* 알고리즘 사용
        if (path != null && targetIndex < path.Count)
        {
            targetPosition = path[targetIndex].Position;
            targetPosition.y = transform.position.y; // Y축 고정
            //Debug.Log("Current Position: " + transform.position + " Target Position: " + targetPosition);
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f) // 허용 오차
            {
                targetIndex++;
                //Debug.Log("Reached target node, moving to next. Current Index: " + targetIndex);
            }
        }

        MoveTowards(targetPosition);

        if (Vector3.Distance(transform.position, finalTargetPosition) < 0.1f) // 허용 오차
        {
            isMoving = false; // 이동 완료
            StopMovement();
            //Debug.Log("Reached final target position");
        }
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    private void StopMovement()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
