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

    public Sprite[] leafShapes;
    public Gradient leafShades;
    public float minLeafSize;
    public float maxLeafSize;
    public GameObject[] endLeaves;
    public Quaternion[] leafInitialRotations;
    public Vector2[] leafPosOffsets;

    private Vector2 avgLightDir;
    private int numLightsVisible;
    public float growSpeed;
    public float rotateSpeed;
    private List<Node> nodes;
    private Vector2 endPos;
    private Quaternion endRotation;
    private Vector2 drawPosVariance = Vector2.zero;
    private Vector2 prevDrawPos = Vector2.zero;
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
    public LayerMask collisionLayers;

    private BurnZone[] burnZones;

    private Material material;

    private WindowScript[] windows;
    private RoundWindowScript[] roundWindows; //ideally I just make this inherit from the same script but I am too lazy and I am not adding anymore windows
    private bool byWindow = false;

    private bool stuck;

    private AudioSource[] audioSource;
    [SerializeField]
    private float burnVolume;
    [SerializeField]
    private float growVolume;
    public bool isGrowing = false;
    private bool isBurning = false;
    private bool isCharred;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        windows = FindObjectsOfType<WindowScript>();
        roundWindows = FindObjectsOfType<RoundWindowScript>();

        audioSource = GetComponents<AudioSource>();

        burnZones = FindObjectsOfType<BurnZone>();
        player = GameObject.FindGameObjectWithTag("Player");
        lightSources = GameObject.FindGameObjectsWithTag("Light Source").Concat(new GameObject[1] { player }).ToArray();

        material = lineRenderer.material;
        material.SetFloat("_step", 1);

        nodes = new List<Node>();
        lineRenderer.positionCount += 1;
        addNode(transform.position, transform.rotation, Vector2.zero);
        lineRenderer.SetPosition(0, transform.position);
        endPos = (Vector2)transform.position + (Vector2)(transform.up * INITIAL_SIZE);
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPos);
        endRotation = transform.rotation;

        leafInitialRotations = new Quaternion[endLeaves.Length];
        leafPosOffsets = new Vector2[endLeaves.Length];
        for (int i = 0; i < endLeaves.Length; i++)
        {
            SpriteRenderer leafRenderer = endLeaves[i].GetComponent<SpriteRenderer>();
            leafRenderer.sprite = leafShapes[(int)Random.Range(0, leafShapes.Length)];
            leafRenderer.color = leafShades.Evaluate(Random.Range(0f, 1f));
            endLeaves[i].transform.localScale = Vector3.one * Random.Range(minLeafSize, maxLeafSize);
            leafInitialRotations[i] = endLeaves[i].transform.localRotation;
            leafPosOffsets[i] = endLeaves[i].transform.localPosition;
            leafRenderer.material = material;
            endLeaves[i].transform.position = endPos + leafPosOffsets[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        avgLightDir = Vector2.zero;
        numLightsVisible = 0;
        if (!byWindow && !stuck)
        {
            foreach (GameObject light in lightSources)
            {
                
                RaycastHit2D hit = Physics2D.Linecast(light.transform.position, endPos, layers);
                if (hit == false)
                {
                    avgLightDir += (Vector2)light.transform.position;
                    numLightsVisible++;
                }
            }
                    
                    RaycastHit2D hitSolids = Physics2D.Raycast(endPos, endRotation * Vector2.up, growSpeed * Time.deltaTime, collisionLayers);
                    if (numLightsVisible > 0 && !hitSolids && !nodes[nodes.Count - 1].isBurning())
                        {
                            growTowards(avgLightDir/numLightsVisible);
                            playerDist = (Vector2)player.transform.position - endPos;
                            audioSource[0].panStereo = Mathf.Clamp(-playerDist.x / 2, -1, 1);
                            audioSource[0].volume = Mathf.Clamp((1 / playerDist.magnitude), 0, 1);
                            
                            if (!audioSource[0].isPlaying) { audioSource[0].Play(); }
                        }
                        else if (audioSource[0].isPlaying) { audioSource[0].Pause(); }
            foreach (WindowScript w in windows)
            {
                if (w.CheckPoint(endPos))
                {
                    
                    byWindow = true;
                    audioSource[0].Stop();
                }
            } //make window a base class and iterate only through one array if you want
            foreach (RoundWindowScript w in roundWindows)
            {
                if (w.CheckPoint(endPos))
                {

                    byWindow = true;
                    audioSource[0].Stop();
                }
            }
        }
        if(!isCharred && nodes[0].isBurning() && nodes[^1].isBurning())
        {
            StartCoroutine(FadeAudio(audioSource[1], true, 0));
            isCharred = true;
            BurnAway();
        }
        CheckBurnZones();
    }

    public void growTowards(Vector2 position)
    {
        float percentOfLink = Mathf.InverseLerp(0, maxlinkLength, Vector2.Distance(endPos, nodes[nodes.Count - 1].GetPos()));
        Vector2 targetVector = position - endPos;
        Quaternion targetRotation = Quaternion.LookRotation(-targetVector, Vector3.forward);
        endRotation = Quaternion.RotateTowards(endRotation, targetRotation, Time.deltaTime * rotateSpeed);
        endRotation.y = 0;
        endRotation.x = 0;
        endPos += (Vector2)(endRotation * Vector2.up * growSpeed * Time.deltaTime);
        
        //endPos += hit.normal * (hit.point - endPos)*1.1f;
        for (int i = 0; i < endLeaves.Length; i++)
        {
            endLeaves[i].transform.position = endPos + (drawPosVariance * percentOfLink) + leafPosOffsets[i];
            endLeaves[i].transform.rotation = leafInitialRotations[i] * endRotation;
        }
            if (Vector2.Distance(endPos, nodes[^1].GetPos()) >= maxlinkLength){
            addNode(endPos, endRotation, drawPosVariance);
            if(Random.Range(0, 1f) < twigChance) { addTwig(endPos+drawPosVariance, endRotation); }
            prevDrawPos = drawPosVariance;
            drawPosVariance = new Vector2(Random.Range(-nodePosVariance, nodePosVariance), Random.Range(-nodePosVariance, nodePosVariance));
            percentOfLink = 0;
        }
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPos + Vector2.Lerp(prevDrawPos, drawPosVariance, percentOfLink));
    }
    private void addNode(Vector2 position, Quaternion rotation, Vector2 offset){
        nodes.Add(new Node(position, rotation, position + offset));
        lineRenderer.SetPosition(lineRenderer.positionCount-1, position + offset);
        lineRenderer.positionCount += 1;
    }
    private void addTwig(Vector2 position, Quaternion rotation)
    {
        GameObject twig = Instantiate(twigObject, position, rotation, transform);
        int dir = (Random.Range(0, 1) * 2) - 1;
        twig.GetComponent<Twig>().BeginGrowth(twigCurliness*dir, position, rotation, material, twigLength, lineRenderer.colorGradient, GenerateLeaf());
    }
    private void CheckBurnZones()
    {
        foreach (BurnZone z in burnZones)
        {
            for(int i = 0; i < nodes.Count; i++)
            {
                if (Vector2.Distance(z.GetPosition(), nodes[i].GetPos()) <= z.GetRadius())
                {
                    if (!isBurning) 
                    {
                        audioSource[1].volume = 0;
                        audioSource[1].Play();
                        StartCoroutine(FadeAudio(audioSource[1], false, burnVolume));
                        isBurning = true;
                    }
                    
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
    public IEnumerator FadeAudio(AudioSource a, bool fadeOut, float volume)
    {
        while ((fadeOut && a.volume > volume) || (!fadeOut && a.volume < volume))
        {
            if (fadeOut)
            {
                a.volume -= 0.05f;
            }
            else
            {
                a.volume += 0.05f;
            }
            yield return new WaitForSeconds(0.1f);
        }
        if (fadeOut) { a.Pause(); }
    }
    public void BurnAway()
    {
        lineRenderer.colorGradient = charredColor;
        for (int i = 0; i < endLeaves.Length; i++)
        {
            SpriteRenderer leafRenderer = endLeaves[i].GetComponent<SpriteRenderer>();
            leafRenderer.color = charredColor.Evaluate(0);
        }
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
    private GameObject GenerateLeaf()
    {
        GameObject result = new GameObject();
        SpriteRenderer renderer = result.AddComponent<SpriteRenderer>();
        renderer.sprite = leafShapes[Random.Range(0, leafShapes.Length)];
        renderer.color = leafShades.Evaluate(Random.Range(0f, 1f));
        renderer.material = material;
        result.transform.localScale = Vector3.one * Random.Range(minLeafSize, maxLeafSize) / 2;
        return result;
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
    public bool IsByWindow()
    {
        return byWindow;
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
