using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Plant[] plants;
    private bool levelComplete;
    private bool transitioning;
    private LevelLoader levelLoader;
    void Start()
    {
        plants = FindObjectsOfType<Plant>();
        levelLoader = FindObjectOfType<LevelLoader>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transitioning) { return; }
        levelComplete = true;
        foreach(Plant p in plants)
        {
            if(p == null)
            {
                levelComplete = false;
                StartCoroutine(delayedRestart());
            }else if (!p.IsByWindow())
            {
                levelComplete = false;
            }
        }
        if (levelComplete)
        {
            levelLoader.LoadNextLevel();
        }
    }
    private IEnumerator delayedRestart()
    {
        transitioning = true;
        yield return new WaitForSeconds(1);
        StartCoroutine(levelLoader.RestartLevel());
    }
}
