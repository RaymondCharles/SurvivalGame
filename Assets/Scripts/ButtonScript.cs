using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class ButtonScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Image img;
    public Sprite default_img;
    public Sprite pressed_img;
    public AudioClip compressed;
    public AudioClip uncompressed;
    public AudioSource source;


    public string sceneToLoad;
    public float loadDelay = 1f;

    public void OnPointerDown(PointerEventData eventData)
    {
        img.sprite = pressed_img;
        source.PlayOneShot(compressed);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        img.sprite = default_img;
        source.PlayOneShot(uncompressed);
        StartCoroutine(LoadSceneWithDelay());
    }

    private IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(sceneToLoad);
    }
}
