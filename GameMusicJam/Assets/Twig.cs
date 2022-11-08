using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Twig : MonoBehaviour
{
    const float LINK_LENGTH = 0.3f;
    const float GROWSPEED = 2;
    const float DIR_CHANGE_CHANCE = 0.003f;
    private float curliness;
    private Vector2 position;
    private Quaternion rotation;
    private LineRenderer lineRenderer;
    private float length;
    private float remainingLength;
    void Update()
    {
        
    }
    public void BeginGrowth(float curliness, Vector2 position, Quaternion rotation, Material material, float length, Gradient gradient)
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
            rotation *= Quaternion.AngleAxis(curliness * (1 - Mathf.Pow(remainingLength / length, 3)), Vector3.forward);
            position += (Vector2)(rotation * Vector2.up * GROWSPEED * Time.deltaTime);
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
            if(Vector2.Distance(position, lineRenderer.GetPosition(lineRenderer.positionCount - 2)) > LINK_LENGTH)
            {
                addNode(position);
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
            }
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
    }
}
