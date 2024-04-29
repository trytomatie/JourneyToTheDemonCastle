using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SystemMessageManager : MonoBehaviour
{
    public CanvasGroup systemMessageCavasGroup;
    public TextMeshProUGUI systemMessageText;

    // Singelton
    public static SystemMessageManager Instance;
    // Start is called before the first frame update
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        ProcessingFinished();
    }


    public static void DisplayProcessingMessage(string message)
    {
        Instance.systemMessageText.text = message;
        Instance.systemMessageCavasGroup.alpha = 1;
    }

    public static void ProcessingFinished()
    {
        Instance.systemMessageCavasGroup.alpha = 0;
    }
}
