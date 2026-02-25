using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextStage : MonoBehaviour
{
    [SerializeField] private string scenename;
    [SerializeField] private int nextstage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            //load next scene
            SceneManager.LoadScene(scenename);
            PlayerSave.Instance.SaveCurrentStage(nextstage);
        }
    }
}
