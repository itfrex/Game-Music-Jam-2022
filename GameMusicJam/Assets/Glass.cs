using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glass : MonoBehaviour
{
    public Vector2 size;
    private void Awake()
    {
        size = GetComponent<BoxCollider2D>().size;
    }

    public bool CheckPoint(Vector2 point)
    {
        bool result = false;
        Vector2 temp = point - (Vector2)transform.position;
        if(Mathf.Abs(temp.x) < size.x && Mathf.Abs(temp.y) < size.y)
        {
            result = true;
        }

        return result;
    }
}
