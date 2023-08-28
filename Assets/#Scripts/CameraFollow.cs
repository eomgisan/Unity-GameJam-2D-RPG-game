using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector2 clampVec;
    private Vector3 offset = new Vector3(0f, 0f, -10f);
    private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;
    private bool clamped = false;

    [SerializeField] private Transform target;

    private void Update()
    {
        Vector3 targetPosition = target.position + offset;


        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        if (transform.position.x > clampVec.x)
            SetX(clampVec.x);
        else if (transform.position.x < -clampVec.x)
            SetX(-clampVec.x);
        /*
        if (targetPosition.y > clampVec.y)
            SetY(clampVec.y);
        */
        if (transform.position.y < -clampVec.y)
            SetY(-clampVec.y);

        void SetX(float axisX)
        {
            transform.position = new Vector3(axisX, transform.position.y, transform.position.z);
            clamped = true;
        }

        void SetY(float axisY)
        {
            transform.position = new Vector3(transform.position.x, axisY, transform.position.z);
            clamped = true;
        }

    }


}