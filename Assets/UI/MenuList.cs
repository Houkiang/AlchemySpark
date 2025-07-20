using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Match3
{
    public class MenuList : MonoBehaviour
    {
        public GameObject menuList;//
        public GameObject inform;
        [SerializeField] private bool menuKeys = true;
        [SerializeField] private AudioSource bgmSound; //背景音乐暂停
        [SerializeField] private bool InformKeys = false;

        // Start is called before the first frame update
        // Update is called once per frame
        void Update()
        {
            if (menuKeys)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    menuList.SetActive(true);
                    menuKeys = false;
                    Time.timeScale = 0;//pause
                    bgmSound.Pause();    //sound pause
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                menuList.SetActive(false);
                menuKeys = true;
                Time.timeScale = 1;//restart
                bgmSound.Play();
            }
            if (InformKeys)
            {
                if (Input.GetKeyDown(KeyCode.H))
                {
                    inform.SetActive(true);
                    InformKeys = false;
                    Time.timeScale = 0;//pause

                }
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                inform.SetActive(false);
                InformKeys = true;
                Time.timeScale = 1;//restart

            }
        }
        public void Return()//返回游戏
        {
            menuList.SetActive(false);
            menuKeys = true;
            Time.timeScale = 1;//restart
            bgmSound.Play();
        }
        public void back()//返回游戏
        {
            inform.SetActive(false);
            InformKeys = true;
            Time.timeScale = 1;//restart

        }
        public void Restart()//重开
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        public void Exit()//退出游戏
        {
            SceneManager.LoadScene(1);
            Time.timeScale = 1;
        }

    }
}