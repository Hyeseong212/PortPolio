using UnityEngine;

public class Node : MonoBehaviour
{
    public Vector3 Position { get { return transform.position; } }
    public Node Parent;
    public float G; // Cost from start to current node
    public float H; // Heuristic cost from current node to end
    public float F { get { return G + H; } } // Total cost
}