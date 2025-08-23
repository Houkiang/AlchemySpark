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

        public GameGrid gameGrid;
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


                    bgmSound.Pause();    //sound pause
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                menuList.SetActive(false);
                menuKeys = true;


                bgmSound.Play();
            }
            if (InformKeys)
            {
                if (Input.GetKeyDown(KeyCode.H))
                {
                    inform.SetActive(true);
                    InformKeys = false;



                }
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                inform.SetActive(false);
                InformKeys = true;


            }
            if (gameGrid != null)
            {
                Debug.Log($"Inform Active: {inform.activeSelf}, Menu Active: {menuList.activeSelf}");
                if (inform.activeSelf == false && menuList.activeSelf == false)
                {
                    gameGrid.ResumeGame();
                }
                else
                {
                    gameGrid.PauseGame();
                }
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
            if(SceneManager.GetActiveScene().buildIndex==1)
            {
                // 在打包的应用中退出游戏
                Application.Quit();
        
                // 在Unity编辑器中停止播放模式
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
            }
            else
            {
                SceneManager.LoadScene(1);
            }
            Time.timeScale = 1;
        }

    }
}