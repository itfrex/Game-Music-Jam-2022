using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class LevelLoader : MonoBehaviour
{
    private static LevelLoader _instance;
    public static LevelLoader Instance { get { return _instance; } }
    public Animator transition;
    public UnityEvent OnTransitionDone;

    public float timeToWait;
    public float restartSpeed;

    private float previousSpeed;
    
    public void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            previousSpeed = 1;
        }
        transition = GetComponent<Animator>();
        transition.speed = previousSpeed;
    }
    private void Update()
    {
        if (Input.GetKeyDown("r"))
        {
            StartCoroutine(RestartLevel());
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(LoadLevelByName("MainMenu"));
        }
    }

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel((SceneManager.GetActiveScene().buildIndex + 1)));
    }
    public IEnumerator LoadLevel(int levelIndex)
    {
        previousSpeed = 1;
        transition.speed = previousSpeed;
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(timeToWait);
        SceneManager.LoadScene(levelIndex);
    }
    public IEnumerator LoadLevelByName(string name)
    {
        previousSpeed = 1;
        transition.speed = previousSpeed;
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(timeToWait);
        SceneManager.LoadScene(name);
    }
    public IEnumerator RestartLevel()
    {
        previousSpeed = restartSpeed;
        transition.speed = previousSpeed;
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(timeToWait/restartSpeed);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ForceRestart()
    {
        Debug.Log("Start");
        StartCoroutine("RestartLevel");    
    }
}
