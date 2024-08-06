using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    public float neighbourRadius = 1.5f; // 노드 간 거리
    public LayerMask obstacleLayer; // 장애물 레이어 마스크

    public List<Node> FindPath(Vector3 start, Vector3 end)
    {
        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        Node startNode = FindClosestNode(start);
        //Debug.Log("Start Node: " + startNode.Position);

        Node endNode = FindClosestNode(end);
        //Debug.Log("End Node: " + endNode.Position);

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].F < currentNode.F || openList[i].F == currentNode.F && openList[i].H < currentNode.H)
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == endNode)
            {
                //Debug.Log("Path found");
                return RetracePath(startNode, currentNode);
            }

            foreach (Node neighbour in GetNeighbours(currentNode))
            {
                if (closedList.Contains(neighbour) || IsPathObstructed(currentNode.Position, neighbour.Position))
                {
                    continue;
                }

                float newCostToNeighbour = currentNode.G + Vector3.Distance(currentNode.Position, neighbour.Position);
                if (newCostToNeighbour < neighbour.G || !openList.Contains(neighbour))
                {
                    neighbour.G = newCostToNeighbour;
                    neighbour.H = Vector3.Distance(neighbour.Position, endNode.Position);
                    neighbour.Parent = currentNode;

                    if (!openList.Contains(neighbour))
                    {
                        openList.Add(neighbour);
                        //Debug.Log("Added neighbour to open list: " + neighbour.Position);
                    }
                }
            }
        }

        Debug.Log("Path not found");
        return null;
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    private List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        Collider[] colliders = Physics.OverlapSphere(node.Position, neighbourRadius);
        foreach (Collider collider in colliders)
        {
            Node neighbourNode = collider.GetComponent<Node>();
            if (neighbourNode != null && neighbourNode != node)
            {
                neighbours.Add(neighbourNode);
            }
        }
        return neighbours;
    }

    private bool IsPathObstructed(Vector3 start, Vector3 end)
    {
        Ray ray = new Ray(start, end - start);
        float distance = Vector3.Distance(start, end);
        return Physics.Raycast(ray, distance, obstacleLayer);
    }

    public Node FindClosestNode(Vector3 position)
    {
        Node[] nodes = FindObjectsOfType<Node>();
        Node closestNode = null;
        float closestDistance = Mathf.Infinity;

        foreach (Node node in nodes)
        {
            float distance = Vector3.Distance(position, node.Position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }
}
