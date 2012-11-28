#include <Servo.h>

Servo servo0;
Servo servo1;
Servo servo2;
Servo servo3;
Servo servo4;
Servo servo5;
Servo servo6;

int servo0Left = 50;
int servo1Left = 60;
int servo2Left = 50;
int servo3Left = 50;
int servo4Left = 30;
int servo5Left = 80;
int servo6Left = 50;

int servo0Right = 130;
int servo1Right = 120;
int servo2Right = 115;
int servo3Right = 120;
int servo4Right = 90;
int servo5Right = 130;
int servo6Right = 110;

int input = 0;
  
// the setup routine runs once when you press reset:
void setup() {                
  // initialize the digital pin as an output.
  servo0.attach(2);
  servo1.attach(3);
  servo2.attach(4);
  servo3.attach(5);
  servo4.attach(6);
  servo5.attach(7);
  servo6.attach(8);
  
  Serial.begin(9600);
  Serial.println("Beginning Connect 4 Robot Arduino");
}

// the loop routine runs over and over again forever:
void loop() {
  if(Serial.available() > 0) {
    //main loop
    input = Serial.read();
    Serial.println("got an input");
    Serial.println(input);
    int col = input - 48;
    if(col >= 0 && col <= 7){
      switch(col){
        case 0:
          servo0.write(servo0Right);
          servo1.write(servo1Right);
          servo3.write(servo3Right);
          break;
         case 1:
           servo0.write(servo0Right);
           servo1.write(servo1Right);
           servo3.write(servo3Left);
           break;
         case 2:
           servo0.write(servo0Right);
           servo1.write(servo1Left);
           servo4.write(servo4Right);
           break;
          case 3:
           servo0.write(servo0Right);
           servo1.write(servo1Left);
           servo4.write(servo4Left);
           break;
          case 4:
           servo0.write(servo0Left);
           servo2.write(servo2Right);
           servo5.write(servo5Right);
           break;
          case 5:
           servo0.write(servo0Left);
           servo2.write(servo2Right);
           servo5.write(servo5Left);
           break;
          case 6:
           servo0.write(servo0Left);
           servo2.write(servo2Left);
           servo6.write(servo6Right);
           break;
          case 7:
           servo0.write(servo0Left);
           servo2.write(servo2Left);
           servo6.write(servo6Left);
           break;
        default:
          break;
      }
    }
  }
  /*
  servo0.write(servo0Left);
  servo1.write(servo1Left);
  servo2.write(servo2Left);
  servo3.write(servo3Left);
  servo4.write(servo4Left);
  servo5.write(servo5Left);
  servo6.write(servo6Left);
  delay(1000);
  servo0.write(servo0Right);
  servo1.write(servo1Right);
  servo2.write(servo2Right);
  servo3.write(servo3Right);
  servo4.write(servo4Right);
  servo5.write(servo5Right);
  servo6.write(servo6Right);
  delay(1000); */
  
}
