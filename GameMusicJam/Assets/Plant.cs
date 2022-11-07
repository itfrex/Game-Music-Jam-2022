using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    private const float INITIAL_SIZE = 0.5f;
    private const float PITCH_MOD = 20f;
    public float growSpeed;
    public float rotateSpeed;
    private List<Node> nodes;
    public Vector2 endPos;
    public Quaternion endRotation;
    private Vector2 drawPosVariance = Vector2.zero;
    public float maxlinkLength = 1;
    public GameObject player;
    private Vector2 playerDist;
    public GameObject lightSource;
    public float nodePosVariance;
    public LineRenderer lineRenderer;
    public LayerMask layers;

    private AudioSource audioSource;
    private bool audioTrigger = false;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();
        audioSource.pitch = 0.8f + (growSpeed)*PITCH_MOD;

        nodes = new List<Node>();
        lineRenderer.positionCount += 1;
        addNode(transform.position, transform.rotation, Vector2.zero);
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
            playerDist = (Vector2)player.transform.position - endPos;
            audioSource.panStereo = Mathf.Clamp(-playerDist.x/20, -1, 1);
            if (!audioSource.isPlaying) {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying) { audioSource.Stop(); }
        }


    }

    public void growTowards(Vector3 position)
    {
        float percentOfLink = Mathf.InverseLerp(0, maxlinkLength, Vector2.Distance(endPos, nodes[nodes.Count - 1].GetPos()));
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPos + (drawPosVariance * percentOfLink));
        Vector2 targetVector = (Vector2)position - endPos;
        Quaternion targetRotation = Quaternion.LookRotation(-targetVector, Vector3.forward);
        endRotation = Quaternion.Slerp(endRotation, targetRotation, Time.deltaTime * rotateSpeed);
        endRotation.y = 0;
        endRotation.x = 0;
        endPos += (Vector2)(endRotation * Vector2.up * growSpeed);
        if (Vector2.Distance(endPos, nodes[nodes.Count - 1].GetPos()) >= maxlinkLength){
            addNode(endPos, endRotation, drawPosVariance);
            drawPosVariance = new Vector2(Random.Range(-nodePosVariance, nodePosVariance), Random.Range(-nodePosVariance, nodePosVariance));
            percentOfLink = 0;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPos + (drawPosVariance * percentOfLink));
        }
    }
    private void addNode(Vector2 position, Quaternion rotation, Vector2 offset){
        nodes.Add(new Node(position, rotation, position + offset));
        lineRenderer.SetPosition(lineRenderer.positionCount-1, position + offset);
        lineRenderer.positionCount += 1;
    }
    public void Burn(int startLink)
    {

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
