using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeModel : Actor 
{
	// Pulibc
	[Header("Components")]
	public Transform   			CubeTransform;
	public MeshRenderer			CubeMeshRenderer;
	public Animator				CubeAnimator;

	[Space(5)] [Header("View")] 
	[SerializeField]
	private GameObject			Mesh;
	[SerializeField]
	private GameObject			ShadowObject;
	public  CubeMaterials 		Materials;
	public  MeshDissolver 		Dissolver;

	[Space(5)] [Header("Animation")] 
	public string				DespawnAnimationName;

	[Space(5)] [Header("Sounds")] 
	[SerializeField]
	public CubeSound			Sounds;

	[Space(5)] [Header("ParticleSystem")] 
	public ParticleSystem		SpawnParticleSystem;
	public ParticleSystem		DespawnParticleSystemMain;
	public ParticleSystem		DespawnParticleSystemAdditional;

	[Space(5)] [Header("Property")]
	public StringListProperty	ObjectsTagsBeforeNotBreak;
	public CurveInterpolation	MovementSpeed;
	public float				BreakDistance;
	public float 				DelayBeforeSpawn;
	public bool 				IsEmpty;
	public 	bool				IsFreeze;
	public bool 				IsIndividualMovement;

//	[HideInInspector]
	public Ramp					RampBottomCube;
	[HideInInspector]	
	public bool					IsStayOnGoal;
	[HideInInspector]
	public bool					IsAttachToFloor;
	[HideInInspector]
	public 	bool				IsCanBeUpdatedTargetPosition; 




	// Private
	[SerializeField]
	private CubeController		Controller;

	private Vector3				TargetPosition;
	private Vector3 			MovementDirection;

	private bool				IsNotBreaking; 

	// Unity Events
	void Start()
	{
		Controller = GetComponent<CubeController>();

		// Выключение
		CubeAnimator.enabled = false;

		Mesh.gameObject.SetActive(false);
		enabled = false;

		TargetPosition = CubeTransform.position;

		Sounds.SetAudio(GetComponent<AudioSource>());
	}

	void Update()
	{
		float curDist = Vector3.Distance(CubeTransform.position, TargetPosition);
	
		if(IsNotBreaking == true)
		{
			MoveToTargetPosition(MovementSpeed.GetLerpValue(0, 1, 0));
		}
		else
		{
			MoveToTargetPosition(MovementSpeed.GetLerpValue(BreakDistance, 0, curDist));
		}

		if(IsAttachToFloor == true)
		{
		//	CubeTransfom.position = new Vector3(CubeTransfom.position.x, GetFloorTop(CubeTransfom.position), CubeTransfom.position.z);
		}
	}
		
	// Spawn Methods
	public void StartSpawn()
	{
		Timer.CreateTimer(DelayBeforeSpawn, delegate()
		{
			Mesh.SetActive(true);
			CubeAnimator.enabled = true;	
		});
	}

	public void EX_OnSpawnAnimationEnded() // Анимация появления закончилась
	{
		if(IsEmpty == false)
			SpawnParticleSystem.Play();

		Sounds.PlaySpawnSound();
		
		// Заканчиваем спавн, после появления системы частиц 
		Timer.CreateTimer(SpawnParticleSystem.main.duration, delegate() {
			Controller.OnModelFinisedSpawn();	
		});
	}
		
	public void StartDespawn()
	{
		if(IsEmpty == false)
		{
			DespawnParticleSystemMain.startColor = Materials.DespawnParticleColorMain;
			DespawnParticleSystemAdditional.startColor = Materials.DespawnParticleColorAdditional;
			DespawnParticleSystemMain.Play();
		}

		CubeAnimator.Play(DespawnAnimationName);

		Sounds.PlayDespawnSound();
	}

	public void EX_OnDespawnAnimationEnded() // Анимация изчезновения закончилась
	{
		Mesh.SetActive(false);

		Controller.OnModelFinisedDespawn();
	}
		
	public void UpdateTargetPositionByDirection()
	{
		if(IsCanBeUpdatedTargetPosition == false) return;
			 	
		TargetPosition = FindTargetPosition(CubeTransform.position, MovementDirection);
		IsNotBreaking = IsNotBrakeObjectBottom(TargetPosition);

		Controller.UpdateTarget(Vector3.Distance(CubeTransform.position, TargetPosition));
	}

	public void UpdateTargetPositionByDirection(Vector3 direction)
	{
		if(IsCanBeUpdatedTargetPosition == false) return;

		MovementDirection = direction; 
		TargetPosition = FindTargetPosition(CubeTransform.position, direction);
		IsNotBreaking = IsNotBrakeObjectBottom(TargetPosition);

		Controller.UpdateTarget(Vector3.Distance(CubeTransform.position, TargetPosition));
	}

	public void UpdateTargetPosition(Vector3 target)
	{
		if(IsCanBeUpdatedTargetPosition == false) return;

		MovementDirection = Actor.GetDirectionBetweenTwoVector(target, CubeTransform.position);
		TargetPosition = target;
		IsNotBreaking = IsNotBrakeObjectBottom(TargetPosition);

		Controller.UpdateTarget(Vector3.Distance(CubeTransform.position, TargetPosition));
	}

	public void MoveToTargetPosition(float speed)
	{
		//Debug.Log("MovingToTargetPosition " + gameObject.name + " targetPosition " + TargetPosition + " current " + CubeTransform.position);
		CubeTransform.position = Vector3.MoveTowards(CubeTransform.position, TargetPosition, speed * Time.deltaTime);

		if(CubeTransform.position == TargetPosition)
		{
			enabled = false;

			IsStayOnGoal = IsGoalBottom(CubeTransform.position);

			Controller.ReachTarget();
		}
	}
		
	public void SetMaterial(CubeMaterials material)
	{
		if(IsEmpty == true) return;

		Material[] cachedMaterials = new Material[CubeMeshRenderer.materials.Length];
		cachedMaterials[1] = material.Main;
		cachedMaterials[0] = material.Additional;
		CubeMeshRenderer.materials = cachedMaterials;
		Materials = material;

		//StartCoroutine(LerpToNewMaterial(material));
	}
		
	IEnumerator LerpToNewMaterial(CubeMaterials material)
	{
		Color oldMainColor = Materials.Main.color;
		Color oldAdditionalColor = Materials.Additional.color;

		Color mainColor = material.Main.color;
		Color additionalColor = material.Additional.color;


		Material[] cachedMaterials = new Material[CubeMeshRenderer.materials.Length];
		cachedMaterials[1] = material.Main;
		cachedMaterials[0] = material.Additional;
		CubeMeshRenderer.materials = cachedMaterials;
		Materials = material;

		CubeMeshRenderer.materials[1].color = oldMainColor;
		CubeMeshRenderer.materials[0].color = oldAdditionalColor;

		while(CubeMeshRenderer.materials[1].color != mainColor)
		{
			CubeMeshRenderer.materials[1].color = Materials.LerpTo(CubeMeshRenderer.materials[1].color,  mainColor, Time.deltaTime);
			CubeMeshRenderer.materials[0].color = Materials.LerpTo(CubeMeshRenderer.materials[0].color,  additionalColor, Time.deltaTime);

			yield return new WaitForEndOfFrame();
		}
	}


	public void StartDissolver()
	{
		Dissolver.StartDissolveByDirection(-MovementDirection);
		ShadowObject.SetActive(false);
	}

	public void StartAppear()
	{
		Dissolver.StartAppearByDirection(MovementDirection);
		ShadowObject.SetActive(true);
	}




	// Private Method
	public Vector3 FindTargetPosition(Vector3 origin, Vector3 direction)
	{
		
		RaycastHit   	allHit;
		Collider[] 		cubesHitInBox; // Сюда будем класть кол-во кубиков вблизи нашего
		Ray 			rayTrace;
		Vector3         currentPosition 	= origin; // Округленная текущая позиция
		Vector3         upOffset        	= new Vector3(0, 0.5f, 0);
		Vector3 		targetPosition;
		int				cubesHitAmount; // Кол-во кубов перед нами по направлению движения
		float 			distToNextObstacle 	= 100; // Изначально обозначаем что препятствие бесконечно далеко

		// Округляем текущую позицию до целой координаты в зависимости от направления кубика. В дальнейшем все расчеты будут производится это этой целой координаты		
		if(direction == Vector3.right)
			currentPosition.x = Mathf.Ceil(currentPosition.x);
		if(direction == Vector3.left)
			currentPosition.x = Mathf.Floor(currentPosition.x);
		if(direction == Vector3.forward)
			currentPosition.z = Mathf.Ceil(currentPosition.z);
		if(direction == Vector3.back)
			currentPosition.z = Mathf.Floor(currentPosition.z);

		rayTrace = new Ray(currentPosition + upOffset, direction);

		// Проверяем, есть ли на пути препятствие
		if(Physics.Raycast(rayTrace, out allHit, 100, ~(1 << gameObject.layer) & ~(1 << 2) ) ) //~(1 << gameObject.layer) - получаем битовую маску со всеми слоями кроме текущего объекта, 1 << 2 - 2 слой IgnoreRaycast
		{
			distToNextObstacle = (allHit.point - currentPosition).magnitude; // Считаем дистанцию до препятствия
		}
			
		rayTrace = new Ray(currentPosition + upOffset, Vector3.down); // Начинаем проверять, есть ли блок земли, начиная с клетки кубика

		// Если под ногами
		if(Physics.Raycast(rayTrace, out allHit, 1, ~(1 << gameObject.layer) & ~(1 << 2)) == false) return origin;


		while(distToNextObstacle - (rayTrace.origin - currentPosition).magnitude > -0.1f 
			&& Physics.Raycast(rayTrace, out allHit, 1, ~(1 << gameObject.layer) & ~(1 << 2)) == true ) // Идем до препятствия (-EPSILON нужен чтобы исключать погрешность) и трейсим вниз, проверяя, есть ли там блок земли
		{
			rayTrace.origin += direction;
		}

		// Написанная Егором, возможно нужно именно так
		//	Vector3 targetPoint = rayTrace.origin - direction - upOffset; // Смещаемся дополнительно на один блок против направления движения, т.к. блок, на который указывает rayTrace.origin, не прошел проверку/является препятствием на пути

		Vector3 targetPoint = rayTrace.origin - direction - upOffset;

		rayTrace = new Ray(origin + upOffset, direction); // Здесь важно использовать неокругленную позицию куба, т. к. иначе мы можем пропустить кубы, которые в данный момент частично находятся в одной клетке с нами
		cubesHitAmount = Physics.RaycastAll(rayTrace, (targetPoint - origin + upOffset).magnitude, 1 << gameObject.layer).Length;

		// Находим все кубики вблизи, чтобы проверить что ничего не упустили в RaycastAll
		cubesHitInBox = Physics.OverlapSphere(origin, 1f, 1 << gameObject.layer); 
		for(int i = 0; i < cubesHitInBox.Length; i++)
		{
			if((origin - cubesHitInBox[i].transform.position).magnitude >= 0.5f) // Если кубик дальше чем на расстоянии 0.5f, его уже заметит RaycastAll и значит ничего делать не надо
				continue;

			// Убираем лишнии координаты в векторах (оставим только по направлению движения) и потом сравниваем, чтобы кубик был впереди (если он сзади нас, то для подсчета позиции его учитывать не нужно)
			Vector3 currentPositionAlongMovement = Vector3.Scale(origin, direction); 
			Vector3 testedCubePositionAlongMovement = Vector3.Scale(cubesHitInBox[i].transform.position, direction);
			if(testedCubePositionAlongMovement.x > currentPositionAlongMovement.x || testedCubePositionAlongMovement.z > currentPositionAlongMovement.z)
				cubesHitAmount += 1; // Если все сошлось, добавляем указанный кубик к кол-ву кубиков перед нами
		}

		targetPosition = targetPoint;

		if(targetPosition != CubeTransform.position)
		{
			targetPosition.y = Mathf.Round(allHit.point.y);
		}

		Debug.DrawLine(CubeTransform.position + new Vector3(0, 1, 0) , targetPoint + new Vector3(0, 1, 0));
		Debug.DrawLine(targetPoint, targetPoint + new Vector3(0, 1, 0));

		int cubeOnCubeAmount = 0;

		// Если целевая точка на кубике
		if(IsCubeBottom(targetPosition) == true)
		{	
			// Куб должен куда-то пододвинуться 
			if(targetPosition != CubeTransform.position)
			{
				rayTrace = new Ray(targetPosition + upOffset + direction, Vector3.down);
				// Если впереди ниижнего куба есть плитки 
				if(Physics.Raycast(rayTrace, out allHit, 2) == true) 
				{
					// Получаем кол-во кубов внизу перед целевой точкой
					// и кол-во кубов стоящих на кубах впереди куба
					int cubeBottomAnount = 0;
					Vector3 startCheckPos = targetPosition;

					while(IsCubeBottom(startCheckPos) == true)
					{
						if(IsCubeBottom(startCheckPos + new Vector3(0, 1, 0)) == true)
						{
							cubeOnCubeAmount++;
						}

						startCheckPos -= direction;
						cubeBottomAnount++;
					}
			
					// Куб стоит на кубе
					if(IsCubeBottom(CubeTransform.position) == true)
					{
						return CubeTransform.position; // никуда не нужно двигать 
					}

					targetPosition = targetPosition - direction * cubeBottomAnount;
				}
				else // Если впереди куба нет плиток
				{
					rayTrace = new Ray(targetPosition + new Vector3(0, 0.5f, 0), Vector3.down);
					if(Physics.Raycast(rayTrace, out allHit, 1, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer))) == true)
					{

						if(allHit.collider.GetComponent<CubeModel>() != null)
						{
							if(allHit.collider.GetComponent<CubeModel>().RampBottomCube != null) // но куб стоит на рампе 
							{
								if(allHit.collider.GetComponent<CubeModel>().RampBottomCube.IsCanToMoveOnDirection(direction) == true) 
								{
									return CubeTransform.position; // никуда не нужно двигать 
								}
							}

						}
					}

				}
			}
		}

		// Сдвигаем относительно кол-во кубов, но не учитываю кубы, которые уедут на других кубах
		targetPosition = targetPosition - direction * (cubesHitAmount - cubeOnCubeAmount);
		return targetPosition;
	}

	// Trace methods

	public bool IsCubeBottom(Vector3 point)
	{
		RaycastHit   	allHit;
		Vector3         upOffset = new Vector3(0, 0.5f, 0);
		Ray 			rayTrace = new Ray(point + upOffset, Vector3.down); 

		if(Physics.Raycast(rayTrace, out allHit, 1) == true)
		{
			if(allHit.collider.CompareTag("Cube") == true)
			{
				return true;
			}
			return false;
		}

		return false;
	}
		
	private bool IsGoalBottom(Vector3 point)
	{
		if(IsEmpty == true) return true;

		RaycastHit  hit;
		Ray 		rayTrace = new Ray(point + new Vector3(0, 1f, 0), Vector3.down);

		if(Physics.Raycast(rayTrace, out hit, 1, Physics.IgnoreRaycastLayer) )
		{
			if(hit.collider.CompareTag("Goal"))
			{
				if(hit.collider.gameObject.GetComponent<Goal>().Color == Materials.Color)
					return true;
				else
					return false;
			}
		}
		return false;
	}


	private float GetFloorTop(Vector3 point)
	{
		RaycastHit  hit;
		Ray 		rayTrace = new Ray(point + new Vector3(0, 0.5f, 0), Vector3.down);

		if(Physics.Raycast(rayTrace, out hit, 1,  ~(1 << gameObject.layer) & ~(1 << 2)) )
		{
			return hit.point.y;
		}

		return point.y;
	}

	private Vector3 GetFloorNormal(Vector3 point)
	{
		RaycastHit  hit;
		Ray 		rayTrace = new Ray(point + new Vector3(0, 0.5f, 0), Vector3.down);

		if(Physics.Raycast(rayTrace, out hit, 1,  ~(1 << gameObject.layer) & ~(1 << 2)) )
		{
			return hit.normal;
		}

		return Vector3.zero;
	}

	private bool IsNotBrakeObjectBottom(Vector3 point)
	{
		RaycastHit  hit;
		Ray 		rayTrace = new Ray(point + new Vector3(0, 1f, 0), Vector3.down);

		if(Physics.Raycast(rayTrace, out hit, 1,  Physics.IgnoreRaycastLayer))
		{
			for(int i = 0; i < ObjectsTagsBeforeNotBreak.Strings.Length; i++)
			{
				if( hit.collider.CompareTag(ObjectsTagsBeforeNotBreak.Strings[i]) == true)
					return true;
			}
			return false;
		}
		return false;

	}


	// Getters
	public void SetFreeze(bool freeze)
	{
		IsFreeze = freeze;
	}

	public Vector3 GetTargetPosition()
	{
		return TargetPosition;
	}

	public Vector3 GetMovementDirection()
	{
		return MovementDirection;
	}


}

