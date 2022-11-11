using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject orbitCenter;

    public float baseAngleStep;
    private float angleStep;

    public float angleChangeStep;
    private float angleChangeAcc = 0;
    void Start()
    {
        angleStep = baseAngleStep;
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(orbitCenter.transform.position, Vector3.forward, angleStep*Time.deltaTime);
        angleStep = Mathf.Max(Mathf.Abs(Mathf.Sin(angleChangeAcc)), 0.5f)* baseAngleStep;
        angleChangeAcc += angleChangeStep*Time.deltaTime;
    }

}
