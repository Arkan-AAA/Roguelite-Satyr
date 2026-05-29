using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour {
    [Header("Transition")]
    public CanvasGroup fadePanel;
    public Transform player;
    public float doorX = 3f;
    public float walkSpeed = 2f;
    public float fadeDuration = 1f;

    private void Start() {
        if (GameManager.Instance != null) {
            GameManager.Instance.SetState(GameState.MainMenu);
        }
        GameInput.Instance?.DisableInput();
    }

    public void PlayGame() {
        StartCoroutine(PlayTransition());
    }

    public void OptionsMenu() {
        GameManager.Instance?.LoadOptionsMenu();
    }

    public void QuitGame() {
        GameManager.Instance?.QuitGame();
    }

    IEnumerator PlayTransition() {
        if (player != null) {
            Vector3 doorPos = new Vector3(doorX, player.position.y, 0);
            while (Vector3.Distance(player.position, doorPos) > 0.1f) {
                player.position = Vector3.MoveTowards(player.position, doorPos, walkSpeed * Time.deltaTime);
                yield return null;
            }
            player.gameObject.SetActive(false);
        }

        float t = 0f;
        while (t < fadeDuration) {
            t += Time.deltaTime;
            if (fadePanel != null) fadePanel.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        GameManager.Instance?.RestartLevel();
    }
}