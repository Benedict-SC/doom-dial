using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour{

	public bool moving = true;

	protected Vector3 thetas; //x is position, y is velocity, z is acceleration
	protected Vector3 radii; //x is position, y is velocity, z is acceleration
	
	protected float maxHP=1;
	protected float hp=1;
	
	protected int mode=0;
	protected int level=4;
	
	RectTransform rt;
	
	public virtual void Start(){
		thetas = new Vector3(0f,0.00f,0.000f);
		radii = new Vector3(Dial.FULL_LENGTH,0f,0f);
	}
	void OnEnable(){
		rt = (RectTransform)transform;
	}
	public virtual void Update(){
		if(Pause.paused)
			return;
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
		rt.anchoredPosition = new Vector2(Mathf.Cos(thetas.x)*radii.x,Mathf.Sin (thetas.x)*radii.x);
		transform.eulerAngles = new Vector3(0,0,thetas.x*Mathf.Rad2Deg-90);
		
		HandleModeStuff();
	}
	public void TakeDamage(float damage){
		if(damage >= hp){
			hp = 0;
			Die ();
		}else{
			hp -= damage;
		}
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
	public virtual void Die(){
		Destroy (gameObject);
	}
	public virtual void OnTriggerEnter2D(Collider2D coll){ //this is said system.
		//Debug.Log ("a collision happened!");
		if (coll.gameObject.tag == "Bullet") //if it's a bullet
		{
			Bullet bc = coll.gameObject.GetComponent<Bullet> ();
			if (bc != null) {
				if (bc.CheckActive()) //if we get a Yes, this bullet/trap/shield is active
				{
					if (bc.isSplitBullet && bc.timerElapsed || !bc.isSplitBullet)
					{
						bc.enemyHit = this.gameObject;
						hp -= bc.dmg + bc.arcDmg;
					}
					if (!bc.isSplitBullet)
					{
						bc.Collide();
					}
					else if (bc.isSplitBullet)
					{
						if (bc.timerElapsed)
						{
							bc.Collide ();
						}
					}
					if(hp <= 0){
						Die ();
					}
				}
			}
		}
		else if (coll.gameObject.tag == "Trap") //if it's a trap
		{
			Trap tc = coll.gameObject.GetComponent<Trap> ();
			if (tc != null) {
				if (tc.CheckActive()) //if we get a Yes, this bullet/trap/shield is active
				{
					tc.enemyHit = this.gameObject;
					hp -= tc.dmg;
					tc.Collide();
					if(hp <= 0){
						Die ();
					}
				}
			}
		}
		else if (coll.gameObject.tag == "Shield") //if it's a shield
		{
			//shield actions are handled in Dial
		}
		else if (coll.gameObject.tag == "AoE")
		{
			Debug.Log ("enemy collided with AoE");
			GameObject obj = coll.gameObject;
			AoE ac = obj.GetComponent<AoE>();
			if (ac.parent == "Bullet")
			{
				if (ac.aoeBulletCon.enemyHit != this.gameObject) //if this isn't the enemy originally hit
				{
					//Debug.Log ("parent is bullet@");
					Bullet bc = ac.aoeBulletCon;
					hp -= bc.dmg;
					if(hp <= 0){
						Die ();
					}
				}
			}
			else if (ac.parent == "Trap")
			{
				if (ac.aoeTrapCon.enemyHit != this.gameObject) //if this isn't the enemy originally hit
				{
					Trap tc = ac.aoeTrapCon;
					hp -= tc.dmg;
					if(hp <= 0){
						Die ();
					}
				}
			}
			
		}
		//other types of collision?
		
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
