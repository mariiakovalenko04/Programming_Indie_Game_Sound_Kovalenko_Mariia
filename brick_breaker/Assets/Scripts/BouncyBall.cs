using UnityEngine;
using TMPro;

public class BouncyBall : MonoBehaviour
{
    public float minY = -5.5f;
    public float maxVelocity = 15f;

    public TextMeshProUGUI scoreTxt;
    public GameObject[] livesImage;
    public GameObject gameOverPanel;
    public AudioSource backgroundMusic;

    [SerializeField] private AudioClip wallClip;
    [SerializeField] private AudioClip paddleClip;
    [SerializeField] private AudioClip brickClip;

    private Rigidbody2D rb;
    private AudioSource source;

    private int score = 0;
    private int lives = 5;
    private bool gameIsOver = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        source = GetComponent<AudioSource>();

        gameOverPanel.SetActive(false);
        scoreTxt.text = score.ToString("00000");
    }

    void Update()
    {
        if (gameIsOver)
        {
            return;
        }

        if (transform.position.y < minY)
        {
            lives--;

            if (lives >= 0 && lives < livesImage.Length)
            {
                livesImage[lives].SetActive(false);
            }

            if (lives <= 0)
            {
                GameOver();
            }
            else
            {
                transform.position = Vector3.zero;
                rb.linearVelocity = Vector2.zero;
            }
        }

        if (rb.linearVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity =
                Vector2.ClampMagnitude(rb.linearVelocity, maxVelocity);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameIsOver)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Brick"))
        {
            source.PlayOneShot(brickClip);

            Destroy(collision.gameObject);

            score += 10;
            scoreTxt.text = score.ToString("00000");
        }
        else if (collision.gameObject.CompareTag("Paddle"))
        {
            source.PlayOneShot(paddleClip);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            source.PlayOneShot(wallClip);
        }
    }

    void GameOver()
    {
        gameIsOver = true;

        rb.linearVelocity = Vector2.zero;

        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}