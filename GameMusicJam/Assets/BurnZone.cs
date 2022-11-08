using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BurnZone : MonoBehaviour
{
    [SerializeField]
    private Vector2 offset;
    [SerializeField]
    private float radius;

    public Vector2 GetPosition()
    {
        return (Vector2)transform.position + offset;
    }
    public float GetRadius() { return radius; }
}
