using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Match3
{
    public class GameOver : MonoBehaviour
    {
        public GameObject screenParent;
        public GameObject scoreParent;
        public Text loseText;
        public Text scoreText;
        public Image[] stars;

        private void Start ()
        {
            screenParent.SetActive(false);

            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].enabled = false;
            }
        }

        public void ShowLose()
        {
            Debug.Log("ShouldShowLose");
            screenParent.SetActive(true);
            scoreParent.SetActive(false);

            Animator animator = GetComponent<Animator>();

            if (animator)
            {
                animator.Play("GameOverShow");
            }
        }

        public void ShowWin(int score, int starCount)
        {
            //Debug.Log("ShouldShowWin");
            screenParent.SetActive(true);
            loseText.enabled = false;

            scoreText.text = score.ToString();
            scoreText.enabled = false;

            Animator animator = GetComponent<Animator>();

            if (animator)
            {
                animator.Play("GameOverShow");
            }

            StartCoroutine(ShowWinCoroutine(starCount));
        }

        private IEnumerator ShowWinCoroutine(int starCount)
        {
            //Debug.Log($"Star Count: {starCount}");
            yield return new WaitForSeconds(0.5f);


                for (int i = 0; i < starCount; i++)
                {
                    stars[i].enabled = true;



                    yield return new WaitForSeconds(0.2f);
                }
            

            scoreText.enabled = true;
        }

        public void OnReplayClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void OnDoneClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelect");
        }

    }
}
