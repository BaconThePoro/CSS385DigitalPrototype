using UnityEngine;
using TMPro;

public class OpeningCrawlAnimation : MonoBehaviour
{
    private float scrollSpeed = 50f;  // Adjust the scrolling speed
    private TextMeshProUGUI openingCrawlText;

    private void Start()
    {
        openingCrawlText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        // Scroll the text upwards along the Z-axis
        Vector3 position = openingCrawlText.rectTransform.position;
        position += Vector3.up * scrollSpeed * Time.deltaTime;
        openingCrawlText.rectTransform.position = position;
    }
}
