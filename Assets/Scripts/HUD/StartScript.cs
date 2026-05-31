using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    TMP_Text text;
    Player_Actions _inputactions;

    bool start_button;

    void Awake()
    {
        _inputactions = new Player_Actions();

        _inputactions.UI.Enable();
        _inputactions.UI.Start.performed += StartGame;
        _inputactions.UI.Start.canceled += StartGame;
    }

    void Start()
    {
        text = GetComponent<TMP_Text>();
        StartCoroutine(BlinkText());
    }

    void OnDestroy()
    {
        if (_inputactions != null)
        {
            _inputactions.UI.Start.performed -= StartGame;
            _inputactions.UI.Start.canceled -= StartGame;

            _inputactions.UI.Disable();
            _inputactions.Dispose();
        }
    }

    private void StartGame(InputAction.CallbackContext context)
    {
        start_button = context.ReadValue<float>() > 0f;
    }

    private void Update()
    {
        if (start_button)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("OceanWaves");
        }
    }

    IEnumerator BlinkText()
    {
        while (true)
        {
            text.enabled = !text.enabled;
            yield return new WaitForSeconds(1f);
        }
    }
}