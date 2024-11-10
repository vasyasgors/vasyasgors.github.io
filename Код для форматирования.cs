using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QUBE
{

public class CubeModel : Actor 
{
public Vector3 FindTargetPosition(Vector3 origin, Vector3 direction)
{
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
}
}

// Сдвигаем относительно кол-во кубов, но не учитываю кубы, которые уедут на других кубах
targetPosition = targetPosition - direction * (cubesHitAmount - cubeOnCubeAmount);
return targetPosition;
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
{
return true;
}
}

return false;

}
else
{
return false;
}
}
}
}

