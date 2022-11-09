using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVisibility : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject showObj;
    public GameObject hideObj;

    public void setVisibility()
    {
        if(showObj!= null)
        {
            showObj.SetActive(true);
        }

        if(hideObj != null)
        {
            hideObj.SetActive(false);
        }
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
