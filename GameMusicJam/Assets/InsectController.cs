using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InsectController : MonoBehaviour
{
    private GameObject player;
    private GameObject[] lightSources;
    private Rigidbody2D rb2d;
    [SerializeField]
    private float eatRadius;

    public float speed;

    public LayerMask layers;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        lightSources = GameObject.FindGameObjectsWithTag("Light Source").Concat(new GameObject[1] { player }).ToArray();
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (GameObject light in lightSources)
        {
            RaycastHit2D hit = Physics2D.Linecast(light.transform.position, transform.position, layers);
            if (hit == false)
            {
                rb2d.AddForce(speed*(light.transform.position - transform.position).normalized);
            }

        }
    }
    public bool CheckInRange(Vector2 point)
    {
        if((point - (Vector2)transform.position).magnitude < eatRadius)
        {
            return true;
        }
        return false;
    }
}
