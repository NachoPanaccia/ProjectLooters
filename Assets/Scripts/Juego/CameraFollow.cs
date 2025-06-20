using UnityEngine;
using Photon.Pun;

public class CameraFollow : MonoBehaviourPunCallbacks
{
    private Transform target;

    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    private void Start()
    {
        foreach (var controller in FindObjectsOfType<MonoBehaviourPun>())
        {
            if (controller.photonView != null && controller.photonView.IsMine)
            {
                target = controller.transform;
                break;
            }
        }
        if (target == null)
        {
            Debug.Log("No se encontro ningun jugador");
        }
    }

    private void Update()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothed = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothed;
    }

    public void SetTarget(Transform t) => target = t;
}
