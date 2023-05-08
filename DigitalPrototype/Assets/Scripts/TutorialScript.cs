using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScript : MonoBehaviour
{
  public GameObject tutorial;
  private int counter = 0;
  private GameObject toKobeActive;
  private GameObject toKobePrevious;

  
  public void NextButton()
  {
    counter++;
    
    if (counter == 5)
    {
         toKobeActive.SetActive(false);

    }
     
    else {
        toKobeActive = tutorial.transform.GetChild(counter).gameObject;
        toKobePrevious = tutorial.transform.GetChild(counter-1).gameObject;
        toKobePrevious.SetActive(false);
        toKobeActive.SetActive(true);
    }
    
  }
}
