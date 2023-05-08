using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void loadOnClick()
    {
        SceneManager.LoadScene("TestScene");
    }

    public void loadTutorial(){
        SceneManager.LoadScene("Tutorial");

    }

    public void loadCredits(){
        SceneManager.LoadScene("Credits");
    }
}
