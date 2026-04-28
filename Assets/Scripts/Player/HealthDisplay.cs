using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    public Sprite emptyHeart;
    public Sprite[] fullHeartFrames;
    public Sprite[] halfHeartFrames;
    [Tooltip("Hvor længe frame 0 (normal) vises inden pulse starter")]
    public float normalDuration = 1.5f;
    [Tooltip("Hastighed på pulse-frames (frame 1 og frem)")]
    public float fps = 8f;

    public Image[] hearts;
    public PlayerHealth playerHealth;

    private void Update()
    {
        float health = playerHealth.health;
        float maxHealth = playerHealth.maxHealth;

        Sprite fullSprite = GetPulseFrame(fullHeartFrames);
        Sprite halfSprite = GetPulseFrame(halfHeartFrames);

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < Mathf.FloorToInt(health))
                hearts[i].sprite = fullSprite;
            else if (i < health)
                hearts[i].sprite = halfSprite;
            else
                hearts[i].sprite = emptyHeart;

            hearts[i].enabled = i < maxHealth;
        }
    }

    private Sprite GetPulseFrame(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0) return null;
        if (frames.Length == 1) return frames[0];

        int pulseFrameCount = frames.Length - 1;
        float cycleDuration = normalDuration + pulseFrameCount / fps;
        float t = Time.time % cycleDuration;

        if (t < normalDuration) return frames[0];

        int pulseFrame = Mathf.FloorToInt((t - normalDuration) * fps);
        return frames[1 + Mathf.Clamp(pulseFrame, 0, pulseFrameCount - 1)];
    }
}
