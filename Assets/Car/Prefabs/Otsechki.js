var a=0; // доли секунды
var b=0; // секунды
var c=0; // минуты
var a2=0; // доли секунды
var b2=0; // секунды
var c2=0; // минуты
var a3=0; // доли секунды
var b3=0; // секунды
var c3=0; // минуты

var e=100; // предел долей
var f=60; // предел секунд

var VremKruga = 0; // время круга
var LucheeVK = 0; // лучшее время круга
var Taimer=0; //  таймер не включен
var TekKrug=1; // текущий круг 
var VKnet=0; // время круга не установлено
var ObheeVrem=0; // общее время сесии
var ObnulenieKruga=0; 

public var styleText : GUIStyle; // добавляем изменяемый стиль текста в Гуи 
function OnGUI () {
GUI.Box(Rect(10,10,430,130)," "); // создаём рамку под текст 
GUI.Box(Rect(10,10,430,130)," "); // создаём рамку под текст (делаем её темнее)
GUI.Box(Rect(440,10,400,40)," "); // создаём рамку под текст 
GUI.Label(new Rect (20,10,40,30),"Общее время "+c3+" : "+b3+" : "+a3,styleText);
GUI.Label(new Rect (40,90,40,30),"Текущий Круг "+c+" : "+b+" : "+a,styleText);
GUI.Label(new Rect (40,50,40,30),"Лучший Круг "+c2+" : "+b2+" : "+a2,styleText);
GUI.Label(new Rect (440,10,40,30),"Круг "+TekKrug,styleText);}
function OnTriggerEnter (other : Collider){ }
function OnTriggerExit (other : Collider){Taimer=1;ObheeVrem=1;TekKrug++;} 

function Update () {
if (ObheeVrem==1){
if (a3<e){a3=a3+5;}
if (a3>=e){a3=0;b3=b3+1;}
if (b3==f){b3=0;c3=c3+1;}
if (Taimer==1){VremKruga=VremKruga+1;
if (a<e){a=a+5;}} 
if (a>=e){a=0;b=b+1;} 
if (b==f){c=c+1;b=0;} 
                 }
if (TekKrug<2){} 
if (TekKrug>2){a2=a;b2=b;c2=c;LucheeVK=VremKruga;ObnulenieKruga=1;} 
if (ObnulenieKruga==1){a=0;b=0;c=0;ObnulenieKruga=2;} 
if (ObnulenieKruga==2){}
}
