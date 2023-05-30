using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialScript : MonoBehaviour
{
    GameObject[] slides;
    int currSlide = 0; 

    private void Start()
    {
        slides = new GameObject[20];
        for (int i = 0; i < slides.Length; i++)
        {
            slides[i] = transform.GetChild(i + 6).gameObject;
        }

        slides[0].SetActive(true);
    }

    public void NextButton()
    {
        if (currSlide != 19)
        {
            slides[currSlide].SetActive(false);
            currSlide = currSlide + 1;
            slides[currSlide].SetActive(true);
        }
    }

    public void PrevButton()
    {
        if (currSlide != 0)
        {
            slides[currSlide].SetActive(false);
            currSlide = currSlide - 1;
            slides[currSlide].SetActive(true);
        }
    }

    public void ExitButton()
    {
        SceneManager.UnloadSceneAsync("TutorialScene");
    }

}
