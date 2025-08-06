using UnityEngine;

public class CookieRotator : MonoBehaviour
{
    //Animasi cookie berputar

    [SerializeField] private float rotationSpeed = 90f; 

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
