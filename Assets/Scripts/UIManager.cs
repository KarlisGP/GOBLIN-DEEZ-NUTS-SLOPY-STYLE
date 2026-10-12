using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Popup")]
    public GameObject popupPanel;
    public TMP_Text popupText;

    [Header("Popup Settings")]
    public float popupDuration = 2f;

    private Coroutine popupCoroutine;

    void Start()
    {
        popupPanel.SetActive(false);
    }

    public void ShowPopup(string message)
    {
        popupText.text = message;

        popupPanel.SetActive(true);

        // Restart timer if another popup is triggered
        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
        }

        popupCoroutine = StartCoroutine(HidePopupAfterTime());
    }

    private IEnumerator HidePopupAfterTime()
    {
        yield return new WaitForSeconds(popupDuration);

        popupPanel.SetActive(false);

        popupCoroutine = null;
    }
}