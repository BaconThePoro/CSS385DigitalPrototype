using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreditScript : MonoBehaviour
{
    // Start is called before the first frame update
    
    // Update is called once per frame
    private float scrollSpeed = 50f;
    private TextMeshProUGUI openingCrawlText;

    public GameObject KobeMoveUp;
    
    private void Start()
    {
        openingCrawlText = GetComponent<TextMeshProUGUI>();
    }
    
 
    private void Update()
    {
        Vector3 position = openingCrawlText.rectTransform.position;
        position += Vector3.up * scrollSpeed * Time.deltaTime;
        openingCrawlText.rectTransform.position = position;
    }
}
