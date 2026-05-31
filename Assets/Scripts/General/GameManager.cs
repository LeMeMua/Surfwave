using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    bool isPaused;

    public int stars;

    public bool gameover;
    public bool winning;

    public GameObject[] UI_canvas;
    public GameObject[] end_canvas;

    Player_Actions _inputActions;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        _inputActions = new Player_Actions();
        _inputActions.UI.Enable();

        _inputActions.UI.Start.performed += ContinueFromEnd;

        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (_inputActions != null)
        {
            _inputActions.UI.Start.performed -= ContinueFromEnd;

            _inputActions.UI.Disable();
            _inputActions.Dispose();
        }
    }

    void Start()
    {
        ResetGame();
    }

    void Update()
    {
        if (stars >= 5 && !winning)
        {
            WinGame();
        }

        if (stars < 0 && !gameover)
        {
            GameOver();
        }
    }

    void WinGame()
    {
        winning = true;
        Time.timeScale = 0f;

        HideUICanvas();

        if (end_canvas.Length > 0 && end_canvas[0] != null)
        {
            end_canvas[0].SetActive(true);
        }
    }

    void GameOver()
    {
        gameover = true;
        Time.timeScale = 0f;

        HideUICanvas();

        if (end_canvas.Length > 1 && end_canvas[1] != null)
        {
            end_canvas[1].SetActive(true);
        }
    }

    void HideUICanvas()
    {
        foreach (GameObject UI in UI_canvas)
        {
            if (UI != null)
            {
                UI.SetActive(false);
            }
        }
    }

    private void ContinueFromEnd(InputAction.CallbackContext context)
    {
        if (winning || gameover)
        {
            GoToMainMenu();
        }
    }

    void GoToMainMenu()
    {
        Time.timeScale = 1f;

        stars = 0;
        winning = false;
        gameover = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("MainMenu");
    }

    public void ResetGame()
    {
        Time.timeScale = 1f;

        isPaused = false;

        stars = 0;
        winning = false;
        gameover = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}