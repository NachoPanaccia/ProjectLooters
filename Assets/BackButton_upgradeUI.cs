
using UnityEngine;
using UnityEngine.UI;

public class BackButton_upgradeUI : MonoBehaviour
{
    public GameObject my_ui;


    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(BackToGame);
    }

    void BackToGame()
    {
        GetComponentInParent<UpgradeUI>().my_player.SetActive(true);
        my_ui.SetActive(false);
    }
}
