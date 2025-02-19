using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        renderer.material = new Material(renderer.material);
        
        var randomHue = Random.Range(0f, 1f);
        renderer.material.SetFloat("_Hue2", randomHue);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
