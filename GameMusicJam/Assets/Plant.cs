using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Linq;

public class Plant : MonoBehaviour
{
    private const float INITIAL_SIZE = 0.5f;
    private const float PITCH_MOD = 0.05f;
    private const float BURN_TIME = 0.1f;
    private const float CHAR_TIME = 2f;

    public float twigChance;
    public float twigCurliness;
    public float twigLength;

    public GameObject burnParticle;
    public GameObject burnLight;
    public GameObject twigObject;
    public Gradient charredColor;

    public float growSpeed;
    public float rotateSpeed;
    private List<Node> nodes;
    private Vector2 endPos;
    private Quaternion endRotation;
    private Vector2 drawPosVariance = Vector2.zero;
    [SerializeField]
    private float maxlinkLength;
    [SerializeField]
    private GameObject player;
    private Vector2 playerDist;
    [SerializeField]
    private GameObject[] lightSources;

    public float nodePosVariance;
    private LineRenderer lineRenderer;
    public LayerMask layers;

    private BurnZone[] burnZones;

    private Material material;

    private AudioSource audioSource;
    public bool isGrowing = false;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();
        burnZones = FindObjectsOfType<BurnZone>();
        player = GameObject.FindGameObjectWithTag("Player");
        lightSources = GameObject.FindGameObjectsWithTag("Light Source").Concat(new GameObject[1] { player }).ToArray();
        audioSource.pitch = 0.8f + (growSpeed*PITCH_MOD);
        material = lineRenderer.material;
        material.SetFloat("_step", 1);

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
        foreach(GameObject light in lightSources)
        {
            RaycastHit2D hit = Physics2D.Linecast(light.transform.position, endPos, layers);
            if (hit == false && !nodes[nodes.Count - 1].isBurning())
            {
                growTowards(light.transform.position);
                playerDist = (Vector2)player.transform.position - endPos;
                audioSource.panStereo = Mathf.Clamp(-playerDist.x / 2, -1, 1);
                audioSource.volume = Mathf.Clamp((1/playerDist.magnitude), 0, 1);
                if (!audioSource.isPlaying) { audioSource.Play(); }
            }
            else if(audioSource.isPlaying) { audioSource.Pause(); }
        }
        if(nodes[0].isBurning() && nodes[^1].isBurning())
        {
            BurnAway();
        }
        CheckBurnZones();
    }

    public void growTowards(Vector2 position)
    {
        float percentOfLink = Mathf.InverseLerp(0, maxlinkLength, Vector2.Distance(endPos, nodes[nodes.Count - 1].GetPos()));
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPos + (drawPosVariance * percentOfLink));
        Vector2 targetVector = position - endPos;
        Quaternion targetRotation = Quaternion.LookRotation(-targetVector, Vector3.forward);
        endRotation = Quaternion.RotateTowards(endRotation, targetRotation, Time.deltaTime * rotateSpeed);
        endRotation.y = 0;
        endRotation.x = 0;
        endPos += (Vector2)(endRotation * Vector2.up * growSpeed * Time.deltaTime);
        if (Vector2.Distance(endPos, nodes[^1].GetPos()) >= maxlinkLength){
            addNode(endPos, endRotation, drawPosVariance);
            if(Random.Range(0, 1f) < twigChance) { addTwig(endPos+drawPosVariance, endRotation); }
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
    private void addTwig(Vector2 position, Quaternion rotation)
    {
        GameObject twig = Instantiate(twigObject, position, rotation, transform);
        twig.GetComponent<Twig>().BeginGrowth(twigCurliness, position, rotation, material, twigLength, lineRenderer.colorGradient);
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
    public void BurnAway()
    {
        lineRenderer.colorGradient = charredColor;        
        foreach (ParticleSystem p in GetComponentsInChildren<ParticleSystem>())
        {
            p.Stop();
        }
        foreach(Light2D l in GetComponentsInChildren<Light2D>())
        {
            l.intensity = 0;
        }
        foreach (Twig t in GetComponentsInChildren<Twig>())
        {
            t.SetColor(charredColor);
        }
        StartCoroutine(DoChar());
    }
    public IEnumerator DoChar()
    {
        for(float timer = CHAR_TIME; timer > 0; timer -= Time.deltaTime)
        {
            material.SetFloat("_step", timer / CHAR_TIME);
            yield return 0;
        }
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
