using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    public Camera followedCam;
    public float speed = 5f;
    public float rotationSpeed = 10f; // ȸ�� �ӵ�
    public float distanceThreshold = 10f; // �Ÿ� �Ӱ谪
    private float wallOffset = -0.5f; // ������ ������ �Ÿ�
    private List<Node> path;
    private Vector3 finalTargetPosition;
    private int targetIndex;
    public LayerMask floorLayer; // �ٴ� ���̾� ����ũ
    public LayerMask obstacleLayer; // ��ֹ� ���̾� ����ũ

    private bool isMoving = false; // ĳ���� �̵� ����

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
        if (Input.GetMouseButton(1)) // ������ ���콺 ��ư Ŭ��
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
            // �浹�� ��ü�� ��ֹ��� ���, �� �ڿ� �ִ� ��带 ã��
            Node closestNodeBehindWall = FindNodeBehindWall(hit.point, hit.normal);
            if (closestNodeBehindWall != null)
            {
                CalculatePathToNode(closestNodeBehindWall);
            }
            else
            {
                // ������ ���� �Ÿ� ������ ������ ��ǥ�� ����
                finalTargetPosition = hit.point - hit.normal * wallOffset;
                finalTargetPosition.y = characterY; // Y�� ����
                isMoving = true; // �̵� ����
            }
        }
        else
        {
            float distance = Vector3.Distance(transform.position, targetPosition);
            RaycastHit obstacleHit;
            bool hasObstacle = Physics.Raycast(transform.position, (targetPosition - transform.position).normalized, out obstacleHit, distance, obstacleLayer);

            if (distance <= distanceThreshold && !hasObstacle)
            {
                // �Ӱ谪 �����̰� ��ֹ��� ���� ��� �������� �̵�
                finalTargetPosition = targetPosition;
                path = null; // A* ��� �ʱ�ȭ
                isMoving = true; // �̵� ����
            }
            else
            {
                // �Ӱ谪 �ʰ��̰ų� ��ֹ��� �ִ� ��� A* �˰��� ���
                CalculatePathToTarget(targetPosition);
            }
        }
    }

    private Node FindNodeBehindWall(Vector3 hitPoint, Vector3 hitNormal)
    {
        // ������ �����ͼ� �� �ڿ� �ִ� ���� ����� ��带 ã��
        Node[] nodes = FindObjectsOfType<Node>();
        float closestDistance = Mathf.Infinity;
        Node closestNode = null;

        foreach (Node node in nodes)
        {
            // ��尡 �� �ڿ� �ִ��� Ȯ��
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
        finalTargetPosition = node.Position; // ���� ��ǥ ��ġ ����
        finalTargetPosition.y = transform.position.y; // Y�� ����
        targetIndex = 0;
        isMoving = true; // �̵� ����
        //Debug.Log("Path found: " + path.Count + " nodes to node behind wall");
    }

    private void CalculatePathToTarget(Vector3 targetPosition)
    {
        AStarPathfinding pathfinding = GetComponent<AStarPathfinding>();
        Node closestNode = pathfinding.FindClosestNode(targetPosition);
        path = pathfinding.FindPath(transform.position, closestNode.Position);
        finalTargetPosition = targetPosition; // ���� ��ǥ ��ġ ����
        finalTargetPosition.y = transform.position.y; // Y�� ����
        targetIndex = 0;
        isMoving = true; // �̵� ����
        //Debug.Log("Path found: " + path.Count + " nodes to target position");
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = finalTargetPosition;

        // ��ΰ� �ִ� ��� A* �˰��� ���
        if (path != null && targetIndex < path.Count)
        {
            targetPosition = path[targetIndex].Position;
            targetPosition.y = transform.position.y; // Y�� ����
            //Debug.Log("Current Position: " + transform.position + " Target Position: " + targetPosition);
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f) // ��� ����
            {
                targetIndex++;
                //Debug.Log("Reached target node, moving to next. Current Index: " + targetIndex);
            }
        }

        MoveTowards(targetPosition);

        if (Vector3.Distance(transform.position, finalTargetPosition) < 0.1f) // ��� ����
        {
            isMoving = false; // �̵� �Ϸ�
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
