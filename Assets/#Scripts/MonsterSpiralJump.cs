using UnityEngine;

public class MonsterSpiralJump : MonoBehaviour
{
    public Transform target; // ������ ��ġ�� �޾ƿ��� ���� ����
    public float jumpForce = 10.0f; // ���� ��
    public float forwardForce = 5.0f; // ���ư��� ���� �������� ��
    public float rotationSpeed = 100.0f; // ȸ�� �ӵ�

    private bool isJumping = false;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!isJumping && target != null)
        {
            JumpTowardsPlayer();
        }
    }

    private void JumpTowardsPlayer()
    {
        Vector3 jumpDirection = (target.position - transform.position).normalized;
        rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
        rb.AddForce(Vector3.forward * forwardForce, ForceMode.Impulse);
        isJumping = true;
    }

    private void FixedUpdate()
    {
        if (isJumping)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }
    }
}
