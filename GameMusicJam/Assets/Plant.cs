using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    private const float INITIAL_SIZE = 0.5f;
<<<<<<< HEAD
    private const float PITCH_MOD = 20f;
    private const float BURN_TIME = 0.2f;
=======
    private const float PITCH_MOD = 0.05f;
    private const float BURN_TIME = 0.1f;
    private const int CHOMP_COUNT = 3;
    private const float CHAR_TIME = 2f;

    public float twigChance;
    public float twigCurliness;
    public float twigLength;
>>>>>>> parent of c062139 (Levels)

    public GameObject burnParticle;
    public GameObject burnLight;
    public GameObject smokeParticle;
    public Gradient charredColor;

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

    private BurnZone[] burnZones;
    private List<GameObject> burnParticleList;

    private AudioSource audioSource;
    public bool isGrowing = false;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();
        burnZones = FindObjectsOfType<BurnZone>();
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
        if (hit == false && !nodes[nodes.Count -1].isBurning())
        {
            growTowards(lightSource.transform.position);
            playerDist = (Vector2)player.transform.position - endPos;
            audioSource.panStereo = Mathf.Clamp(-playerDist.x/20, -1, 1);
            isGrowing = true;
        }
        else
        {
            isGrowing = false;
        }
        if(nodes[0].isBurning() && nodes[nodes.Count - 1].isBurning())
        {
            StartCoroutine(DoChar());
        }
        CheckBurnZones();
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
        endPos += (Vector2)(endRotation * Vector2.up * growSpeed * Time.deltaTime);
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
    private void CheckBurnZones()
    {
        foreach (BurnZone z in burnZones)
        {
            for(int i = 0; i < nodes.Count; i++)
            {
                if (Vector2.Distance(z.GetPosition(), nodes[i].GetPos()) <= z.GetRadius())
                {
                    StartCoroutine(Burn(i));
                }
            }
            
        }
    }
    public IEnumerator Burn(int nodeIndex)
    {
        if (nodes[nodeIndex].Ignite())
        {
            Instantiate(burnParticle, nodes[nodeIndex].GetDrawPos(), Quaternion.identity, transform);
            Instantiate(burnLight, nodes[nodeIndex].GetDrawPos(), Quaternion.identity, transform);
            yield return new WaitForSeconds(BURN_TIME);
            if (nodeIndex < nodes.Count - 1)
            {
                StartCoroutine(Burn(nodeIndex + 1));
            }
            if (nodeIndex > 0)
            {
                StartCoroutine(Burn(nodeIndex - 1));
            }
        }
        bool alreadyBurning = nodes[nodeIndex].Ignite();
        yield return new WaitForSeconds(BURN_TIME);
    }
    public IEnumerator DoChar()
    {
        lineRenderer.colorGradient = charredColor;
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        yield return new WaitForSeconds(BURN_TIME);
        Destroy(gameObject);


    }
    private class Node
    {
        Vector2 position;
        Quaternion rotation;
        Vector2 drawPos;
        bool ignited;
        public Node(Vector2 position, Quaternion rotation, Vector2 drawPos)
        {
            this.position = position;
            this.rotation = rotation;
            this.drawPos = drawPos;
            this.ignited = false;
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
        public bool Ignite()
        {
            if(ignited == false)
            {
                ignited = true;
                return true;
            }
            else
            {
                return false;
            }
            
        }
        public bool isBurning()
        {
            return ignited;
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
