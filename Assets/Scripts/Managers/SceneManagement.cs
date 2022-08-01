using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Network;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers
{
    public class SceneManagement : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadePanel; //Fading effect panel
        [SerializeField] private Image loadPanel; // Panel that shows the percentage of scene loadd progress
        [SerializeField] private Slider progressBar; //Loading bar slider gui
        [SerializeField] private TextMeshProUGUI loadText; //Percentage text
        private Dictionary<string, int> sceneList; 
        private AsyncOperation operation;
        private float progress;
        private IEnumerator co;
        private bool hasRequest;

        private void Awake()
        {
            DontDestroyOnLoad(transform.parent.gameObject);
        
            sceneList = new Dictionary<string, int>();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                sceneList.Add(NameFromIndex(i), i);
            }
        }

        private void Update()
        {
            if (!loadPanel.gameObject.activeSelf || operation == null) return;
        
            progressBar.value = Mathf.Clamp01(operation.progress / .9f);
            loadText.text = $"Loading.. {progressBar.value *100} % ";
        }

        //Loads the scene according to given scene name
        IEnumerator SceneLoader(int sceneIndex, float waitBefore)
        {
            yield return new WaitForSeconds(waitBefore);

            yield return FadeOut(1);
            loadPanel.gameObject.SetActive(true);
            yield return FadeIn(2);
            
            operation = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
            progress = operation.progress;
            while (operation is {isDone: false})
            {
                yield return null;
            }
            
            yield return new WaitForSeconds(.5f);
            
            yield return FadeOut(1);
            loadPanel.gameObject.SetActive(false);
            yield return FadeIn(2);
            
            co = null;
            operation = null;
            progressBar.value = 0;
        }

        public void LoadScene(string key, float waitBefore = .5f)
        {
            if (PhotonNetwork.InRoom)
            {
                NetworkManager.Leave();
                return;
            }
            
            if (co != null)
            {
                Debug.LogWarning("There's already an operation ongoing!");
                return;
            }

            co = SceneLoader(sceneList[key], waitBefore);
            StartCoroutine(co);
        }
    
        private string NameFromIndex(int BuildIndex)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
            int slash = path.LastIndexOf('/');
            string name = path.Substring(slash + 1);
            int dot = name.LastIndexOf('.');
            return name.Substring(0, dot);
        }
        
        private IEnumerator FadeOut(float time)
        {
            while (fadePanel.alpha < 1)
            {
                fadePanel.alpha += Time.deltaTime / time;
                yield return null;
            }
        }

        private IEnumerator FadeIn(float time)
        {
            while (fadePanel.alpha > 0)
            {
                fadePanel.alpha -= Time.deltaTime / time;
                yield return null;
            }
        }
    }
}
