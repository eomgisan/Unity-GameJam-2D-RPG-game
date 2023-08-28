using System.Collections;
using UnityEngine;

public class DDongControl : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject _DDONGObject;

    void Start()
    {

    }


    void OnEnable()
    {
        StartCoroutine(genDDong());
    }
    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator genDDong()
    {
        while (true)
        {
            Vector3 genPos = new Vector3(Random.Range(11.5f, -8f), 16, 0);
            Instantiate(_DDONGObject, genPos, _DDONGObject.transform.rotation);
            yield return new WaitForSeconds(0.2f);
        }
    }

}
