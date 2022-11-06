using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    private const float INITIAL_SIZE = 0.1f;
    public float growSpeed;
    public float rotateSpeed;
    private List<Node> nodes;
    public Vector2 endPos;
    public Quaternion endRotation;
    public float maxlinkLength = 1;
    public GameObject lightSource;
    public float nodePosVariance;
    public LineRenderer lineRenderer;
    public LayerMask layers;
    private int maxSize = 128;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        nodes = new List<Node>();
        addNode(transform.position, transform.rotation, false);
        lineRenderer.SetPosition(0, transform.position);
        endPos = (Vector2)transform.position + (Vector2)(transform.position * Vector2.up * INITIAL_SIZE);
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPos);
        endRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.Linecast(lightSource.transform.position, endPos, layers);
        if (hit == false)
        {
            growTowards(lightSource.transform.position);
        }
    }
    public void growTowards(Vector3 position)
    {
        Vector2 targetVector = (Vector2)position - endPos;
        Quaternion targetRotation = Quaternion.LookRotation(-targetVector, Vector3.forward);
        endRotation = Quaternion.Slerp(endRotation, targetRotation, Time.deltaTime * rotateSpeed);
        endRotation.y = 0;
        endRotation.x = 0;
        endPos += (Vector2)(endRotation * Vector2.up * growSpeed);
        if (Vector2.Distance(endPos, nodes[nodes.Count - 1].GetPos()) >= maxlinkLength){
            addNode(endPos, transform.rotation);
        }
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPos);
    }
    private void addNode(Vector2 position, Quaternion rotation, bool doVariance = true){
        Vector2 drawPosVariance = doVariance ? new Vector2(Random.Range(-nodePosVariance, nodePosVariance), Random.Range(-nodePosVariance, nodePosVariance)) : Vector2.zero;
        nodes.Add(new Node(position, rotation, position + drawPosVariance));
        lineRenderer.SetPosition(lineRenderer.positionCount-1, position + drawPosVariance);
        lineRenderer.positionCount += 1;
    }
    private class Node
    {
        Vector2 position;
        Quaternion rotation;
        Vector2 drawPos;
        public Node(Vector2 position, Quaternion rotation, Vector2 drawPos)
        {
            this.position = position;
            this.rotation = rotation;
            this.drawPos = drawPos;
        }
        public Vector2 GetPos()
        {
            return position;
        }
        public Vector2 GetDrawPos()
        {
            return drawPos;
        }
        public Quaternion GetRotation()
        {
            return rotation;
        }
    }
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.2f);
        if (nodes != null && nodes.Count > 1)
        {
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Gizmos.DrawLine(nodes[i].GetDrawPos(), nodes[i + 1].GetDrawPos());
            }
            Gizmos.DrawLine(nodes[nodes.Count - 1].GetDrawPos(), endPos);
        }
    }
}
