using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Match3.GameGrid;

namespace Match3
{

    public class Hud : MonoBehaviour
    {

        public Level level;
        public GameOver gameOver;
        public GameGrid _gamegrid;

        public Text remainingText;
        public Text targetText;
        public Text targetSubtext;
        public Text scoreText;
        public Text temp;

        public Image[] stars;
        public List<Text> herb = new List<Text> ();
        public List<Text> potion = new List<Text> ();

        public Text herbToNext;
        

        private int _starIndex = 0;




        private void Start ()
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].enabled = i == _starIndex;
            }
        }


        public void SetScore(int score)
        {
            scoreText.text = score.ToString();

            int visibleStar = 0;

            if (score >= level.score1Star && score < level.score2Star)
            {
                visibleStar = 1;
            }
            else if (score >= level.score2Star && score < level.score3Star)
            {
                visibleStar = 2;
            }
            else if (score >= level.score3Star)
            {
                visibleStar = 3;
            }

            for (int i = 0; i < stars.Length; i++)
            {
                if (i <= visibleStar)
                {
                    stars[i].enabled = true;
                }
                if (i > visibleStar)
                {
                    stars[i].enabled = false;
                }

            }

            _starIndex = visibleStar;


        }

        public void SetTarget(int target) => targetText.text = target.ToString();

        public void SetRemaining(int remaining) => remainingText.text = remaining.ToString();

        public void SetRemaining(string remaining) => remainingText.text = remaining;
        public void SetHerbToNext(string _string) => herbToNext.text = _string;

        public void SetRemainingherb(List<ColorClearCount> remain)
        {
            int len = remain.Count;
            Debug.Log("##############长度是" + len);
            //Debug.Log("Len=" + len);
            for (int i = 0; i < len; i++)
            {
                herb[0].text = remain[i].count.ToString();
                Debug.Log("temp.text" + herb[0].text);

            }
        }
        public void SetRemainingpotion(List<ColorClearCount> remain)
        {
            int len = remain.Count;
            for (int i = 0; i < len; i++)
            {
                potion[0].text = remain[i].count.ToString();
        
            }
        }


        public void OnGameWin(int score)
        {
            gameOver.ShowWin(score, _starIndex);
            if (_starIndex > PlayerPrefs.GetInt(SceneManager.GetActiveScene().name, 0))
            {
                PlayerPrefs.SetInt(SceneManager.GetActiveScene().name, _starIndex);
            }
        }

        public void OnGameLose() => gameOver.ShowLose();
    }
}
