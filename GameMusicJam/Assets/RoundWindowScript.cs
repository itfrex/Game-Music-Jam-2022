using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundWindowScript : MonoBehaviour
{
    private const float OPEN_TIME = 1.5f;
    private const float OPENED_ANGLE = 145;
    private const float SWAY_AMT = 40;
    private const float SWAY_LOOP_TIME = 3;
    public AnimationCurve openCurve;
    public AnimationCurve swayCurve;
    public bool beginOpened;
    public GameObject leftHinge;
    private bool opened;
    private float swayTimer = 0;
    private bool plantByWindow;
    void Start()
    {
        if (beginOpened)
        {
            StartCoroutine(SwingOpen());
        }
        plantByWindow = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (opened)
        {
            swayTimer += Time.deltaTime;
            if (swayTimer > SWAY_LOOP_TIME) { swayTimer -= SWAY_LOOP_TIME; }
            leftHinge.transform.rotation = Quaternion.Euler((OPENED_ANGLE + swayCurve.Evaluate(swayTimer / SWAY_LOOP_TIME) * SWAY_AMT) * Vector2.up);

        }
    }

    private IEnumerator SwingOpen()
    {
        float timer = 0;
        while (timer < OPEN_TIME)
        {
            leftHinge.transform.rotation = Quaternion.Euler(openCurve.Evaluate(timer / OPEN_TIME) * OPENED_ANGLE * Vector2.up);
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        opened = true;
    }
    public bool CheckPoint(Vector2 point)
    {
        bool result = false;
        Debug.Log("almost Made it");

        if (!plantByWindow && (point - (Vector2)transform.position).magnitude < transform.localScale.x/2)
        {
            Debug.Log("Made it");
            plantByWindow = true;
            result = true;
        }
        return result;
    }
}
