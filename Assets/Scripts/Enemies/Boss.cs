using UnityEngine;

public class Boss : MonoBehaviour{

	public bool moving = true;

	protected Vector3 thetas; //x is position, y is velocity, z is acceleration
	protected Vector3 radii; //x is position, y is velocity, z is acceleration
	
	protected float maxHP=1;
	protected float hp=1;
	
	protected int mode=0;
	protected int level=4;
	
	public virtual void Start(){
		thetas = new Vector3(0f,0.00f,0.000f);
		radii = new Vector3(DialController.FULL_LENGTH,0f,0f);
	}
	public virtual void Update(){
		if (!moving)
			return;
		//Debug.Log(thetas.x + ", " + thetas.y + ", " + thetas.z);
		//kinematic movement
		thetas.x += thetas.y;
		thetas.y += thetas.z;
		radii.x += radii.y;
		radii.y += radii.z;
		//cap theta position
		if(thetas.x > 2*Mathf.PI){
			thetas.x -= 2*Mathf.PI;
		}else if(thetas.x < 0){
			thetas.x += 2*Mathf.PI;
		}
		//set x/y position based on theta and r
		transform.position = new Vector3(Mathf.Cos(thetas.x)*radii.x,Mathf.Sin (thetas.x)*radii.x,transform.position.z);
		transform.eulerAngles = new Vector3(0,0,thetas.x*Mathf.Rad2Deg-90);
		
		HandleModeStuff();
	}
	public virtual void HandleModeStuff(){ //override if boss has different transitions
		if(level == 1)
			return;
		else if(level == 2){
			float healthRatio = hp/maxHP;
			if(healthRatio <= .5)
				mode = 1;
		}else if(level == 3){
			float healthRatio = hp/maxHP;
			if(healthRatio <= .333)
				mode = 2;
			else if(healthRatio <= .666)
				mode = 1;
		}else if(level >= 4){
			float healthRatio = hp/maxHP;
			if(healthRatio <= .25)
				mode = 3;
			else if(healthRatio <= .5)
				mode = 2;
			else if(healthRatio <= .75)
				mode = 1;	
		}
	}
	
	public float GetAngle(){
		return thetas.x;
	}
	public float GetAngularVelocity(){
		return thetas.y;
	}
	public float GetAngularAcceleration(){
		return thetas.z;
	}
	public void SetAngle(float f){
		thetas.x = f;
	}
	public void SetAngularVelocity(float f){
		thetas.y = f;
	}
	public void SetAngularAcceleration(float f){
		thetas.z = f;
	}
	public float GetRadius(){
		return radii.x;
	}
	public void SetRadius(float f){
		radii.x = f;
	}
}
