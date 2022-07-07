// скрипт вешаем на одну из зон триггеров глаз машины
var Car:GameObject; // машина
var GLAZ:GameObject; // Глаз машины другой 
var a=0.1;  // скорость машины
var c=0.01; // скорость поворота машины
var b=0; // в зоне нет преграды 
function OnTriggerEnter (other : Collider){b=1;}
function OnTriggerExit (other : Collider){b=0;}
function Update () {
if (b==1){Car.transform.Rotate(0,c,0);GLAZ.SetActive(false);Car.transform.Translate(0,0,a/2);}
if (b==0){Car.transform.Translate(0,0,a);GLAZ.SetActive(true);}	
}

