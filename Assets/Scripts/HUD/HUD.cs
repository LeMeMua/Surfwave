using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    TMP_Text text;

    void Start()
    {
        text = GetComponent<TMP_Text>();
        UpdateStarsText();
    }

    void Update()
    {
        UpdateStarsText();
    }

    void UpdateStarsText()
    {
        if (text != null && GameManager.instance != null)
        {
            text.text = $"{GameManager.instance.stars}/5";
        }
    }
}