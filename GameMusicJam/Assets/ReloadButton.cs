using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ReloadButton : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private LevelLoader levelLoader;
    [SerializeField]
    private Button button;
    void Start()
    {
        levelLoader = FindObjectOfType<LevelLoader>();
        button = GetComponent<Button>();
        button.onClick.AddListener(delegate { levelLoader.ForceRestart(); });
    }
}
