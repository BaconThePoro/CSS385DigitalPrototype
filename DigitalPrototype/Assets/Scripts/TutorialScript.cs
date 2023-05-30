using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialScript : MonoBehaviour
{
    public void NextButton()
    {

    }

    public void PrevButton()
    {

    }

    public void ExitButton()
    {
        SceneManager.UnloadSceneAsync("TutorialScene");
    }

}
