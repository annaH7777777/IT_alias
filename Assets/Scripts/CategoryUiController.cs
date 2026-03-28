using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CategoryUiController : MonoBehaviour
{
    private const string PracticeScene = "WordScene";
    private const string ChallengeScene = "ChallengeScene";

    [SerializeField] private TMP_Dropdown categoryDropdown;
    [SerializeField] Button startButton;

    void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveAllListeners();
    }

    private void OnStartClicked()
    {
        if (categoryDropdown.value == 0)
        {
            SceneManager.LoadScene(PracticeScene);
            Debug.Log("Selected practice mode");
        }
        else
        {
            SceneManager.LoadScene(ChallengeScene);
            Debug.Log("Selected challenge mode");
        }
    }
}
