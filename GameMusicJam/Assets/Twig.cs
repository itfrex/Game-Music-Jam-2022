using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Twig : MonoBehaviour
{
    const float LINK_LENGTH = 0.3f;
    const float GROWSPEED = 3;
    const float DIR_CHANGE_CHANCE = 0.005f;
    private float curliness;
    private Vector2 position;
    private Quaternion rotation;
    private LineRenderer lineRenderer;
    private float length;
    private float remainingLength;
    private GameObject leaf;
    private Vector3 leafSize;
    void Update()
    {
        
    }
    public void BeginGrowth(float curliness, Vector2 position, Quaternion rotation, Material material, float length, Gradient gradient, GameObject leaf)
    {
        this.curliness = curliness;
        this.position = position;
        transform.position = position;
        this.rotation = rotation;
        transform.rotation = rotation;
        this.lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, position);
        lineRenderer.SetPosition(1, position);
        lineRenderer.material = material;
        lineRenderer.colorGradient = gradient;
        this.length = length;
        remainingLength = length;
        this.leaf = leaf;
        this.leaf.transform.SetParent(transform);
        this.leafSize = leaf.transform.localScale;
        StartCoroutine(Grow());
    }
    public IEnumerator Grow()
    {
        while(remainingLength > 0)
        {
            if(Random.Range(0, 1f) < DIR_CHANGE_CHANCE)
            {
                curliness *= -1;
            }
            rotation *= Quaternion.AngleAxis(curliness * (1/(1+remainingLength)), Vector3.forward);
            position += (Vector2)(rotation * Vector2.up * GROWSPEED * Time.deltaTime);
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
            if(Vector2.Distance(position, lineRenderer.GetPosition(lineRenderer.positionCount - 2)) > LINK_LENGTH)
            {
                addNode(position);
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
            }
            leaf.transform.position = position;
            leaf.transform.rotation = rotation;
            leaf.transform.localScale = (leafSize * (1 - remainingLength / length));
            remainingLength -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    private void addNode(Vector2 position)
    {
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
        lineRenderer.positionCount += 1;
    }
    public void SetColor(Gradient color)
    {
        lineRenderer.colorGradient = color;
        leaf.GetComponent<SpriteRenderer>().color = color.Evaluate(0);
    }
}
