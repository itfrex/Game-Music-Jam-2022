using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    // Start is called before the first frame update
    public List<string> levelNameList;
    public GameObject levelButton;
    public GameObject levelListPanel;
    void Start()
    {
        InitializeLevelList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeLevelList()
    {
        foreach(string levelName in levelNameList)
        {
            GameObject temp = Instantiate(levelButton);
            temp.transform.SetParent(levelListPanel.transform, false);
            temp.GetComponent<Button>().onClick.AddListener(delegate { loadLevel(levelName); });
            temp.GetComponentInChildren<TextMeshProUGUI>().text = levelName;
        }
    }

    public void loadLevel(string levelName)
    {
        try
        {
            SceneManager.LoadScene(levelName);
        }
        catch 
        {
            Debug.Log("Not exist");
        }
    }
}
