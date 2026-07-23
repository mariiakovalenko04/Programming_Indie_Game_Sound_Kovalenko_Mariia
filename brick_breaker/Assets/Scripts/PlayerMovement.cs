using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed=5;
    public float maxX =7.5f;

    float movementHorizontal;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    { 
        
    }

    // Update is called once per frame
    void Update()
    {
      movementHorizontal = Input.GetAxis("Horizontal");
      if((movementHorizontal>0 && transform.position.x<maxX) || (movementHorizontal<0 && transform.position.x > -maxX))
        {
            transform.position +=Vector3.right*movementHorizontal*speed*Time.deltaTime; 
        }
     
       
    }
} 
   