var myWheelCollider : WheelCollider; 
 
function Update () { 
transform.Rotate(myWheelCollider.rpm/60*360*Time.deltaTime,0,0); 
transform.localEulerAngles.z = myWheelCollider.steerAngle - transform.localEulerAngles.z; 
} 