using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowScript : MonoBehaviour
{
    private const float OPEN_TIME = 1.5f;
    private const float OPENED_ANGLE = 145;
    private const float SWAY_AMT = 300;
    private const float SWAY_LOOP_TIME = 3;
    private AudioSource audioSource;
    public AnimationCurve openCurve;
    public AnimationCurve swayCurve;
    public bool beginOpened;
    public GameObject leftHinge;
    public GameObject rightHinge;
    private bool opened;
    private float swayTimer = 0;
    private BoxCollider2D boxCollider;
    private Vector2 winAreaLow;
    private Vector2 winAreaHigh;
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        winAreaLow = new Vector2(transform.position.x - transform.localScale.x / 2, transform.position.y - transform.localScale.y / 2);
        winAreaHigh = new Vector2(transform.position.x + transform.localScale.x / 2, transform.position.y + transform.localScale.y / 2);
        audioSource = GetComponent<AudioSource>();

        if (beginOpened)
        {
            StartCoroutine(SwingOpen());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (opened)
        {
            swayTimer += Time.deltaTime;
            if(swayTimer > SWAY_LOOP_TIME) { swayTimer -= SWAY_LOOP_TIME; }
            leftHinge.transform.rotation = Quaternion.Euler((OPENED_ANGLE + swayCurve.Evaluate(swayTimer / SWAY_LOOP_TIME) * SWAY_AMT) * Vector2.up);
            rightHinge.transform.rotation = Quaternion.Euler((OPENED_ANGLE + swayCurve.Evaluate(swayTimer / SWAY_LOOP_TIME) * SWAY_AMT) * Vector2.down);

        }
    }

    private IEnumerator SwingOpen()
    {
        float timer = 0;
        while(timer < OPEN_TIME)
        {
            leftHinge.transform.rotation = Quaternion.Euler(openCurve.Evaluate(timer/OPEN_TIME) * OPENED_ANGLE * Vector2.up);
            rightHinge.transform.rotation = Quaternion.Euler(openCurve.Evaluate(timer / OPEN_TIME) * OPENED_ANGLE * Vector2.down);
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        opened = true;
    }
    public bool CheckPoint(Vector2 point)
    {
        bool result = false;
        if (winAreaLow.x < point.x && point.x < winAreaHigh.x && winAreaLow.y < point.y && point.y < winAreaHigh.y)
        {
            audioSource.Play();
            result = true;
        }
        return result;
    }
}
