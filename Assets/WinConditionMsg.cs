
using TMPro;
using UnityEngine;

public class WinConditionMsg : MonoBehaviour
{
    TextMeshProUGUI my_textMesh;

    void Start()
    {
        my_textMesh = GetComponent<TextMeshProUGUI>();
        switch (GameManager.instance.actualwin)
        {
            case wincondition.tiempo:
                my_textMesh.text = "Time ran out, sun is shining and so are you";
                break;
            case wincondition.lootpermadead:
                my_textMesh.text = "Looters Died";
                break;
            case wincondition.copdead:
                my_textMesh.text = "The Cop was killed by the Looters";
                break;
            case wincondition.quota:
                my_textMesh.text = "Looters reached quota";
                break;
            default:
                Debug.Log("ERROR ID NO RECONOCIDO");
                break;

        }
    }


}
