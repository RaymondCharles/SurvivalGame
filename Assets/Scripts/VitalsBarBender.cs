using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class VitalsBarBinder : MonoBehaviour
{
    public float hp = 1.0f;
    public float hunger = 1.0f;

    public float hungerDec = 0.5f;
    public float healthDec = 0.1f;

    public UnityEvent<float> onHpChanged;
    public UnityEvent<float> onHungerChanged;

    public GameObject gameOverPanel;
    //public Button restartButton;

    private bool isGameOver = false;

    private void Start()
    {
        // Set initial values first
        hp = 1.0f;
        hunger = 1.0f;

        // Then invoke events so UI updates immediately
        if (onHpChanged != null)
        {
            onHpChanged.Invoke(hp);
        }
        if (onHungerChanged != null)
        {
            onHungerChanged.Invoke(hunger);
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.gameObject.SetActive(false);
        }
        //if (restartButton != null)
        //{
        //    restartButton.onClick.AddListener(RestartGame);
        //}


    }

    void Update()
    {
        if (isGameOver)
        {
            return;
        }
        hunger -= hungerDec * Time.deltaTime;
        hunger = Mathf.Clamp01(hunger);
        if (onHungerChanged != null)
        {
            onHungerChanged.Invoke(hunger);
        }

        if (hunger <= 0f)
        {
            hp -= 0.05f * Time.deltaTime;
            hp = Mathf.Clamp01(hp);
            if (onHpChanged != null)
            {
                onHpChanged.Invoke(hp);
            }

        }
        if (hp <= 0f)
        {
            GameOver();
        }

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (isGameOver)
        {
            return;
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            hp -= healthDec;
            hp = Mathf.Clamp01(hp);
            if (onHpChanged != null)
            {
                onHpChanged.Invoke(hp);
            }
            if (hp <= 0f)
            {
                GameOver();
            }
        }
    }
    public void AddHunger(float amount)
    {
        if (isGameOver)
        {
            return;
        }
        hunger += amount;
        hunger = Mathf.Clamp01(hunger);
        if (onHungerChanged != null)
        {
            onHungerChanged.Invoke(hunger);
        }
    }
    public void AddHealth(float amount)
    {
        if (isGameOver)
        {
            return;
        }
        hp += amount;
        hp = Mathf.Clamp01(hp);
        if (onHpChanged != null)
        {
            onHpChanged.Invoke(hp);
        }
    }
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        // Show Game Over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Start auto-restart coroutine
        StartCoroutine(AutoRestart(2f));
    }

    private IEnumerator AutoRestart(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
