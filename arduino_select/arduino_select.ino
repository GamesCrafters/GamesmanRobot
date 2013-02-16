#include <Servo.h> 
 
Servo servo0;
Servo servo1;
 
int pos0 = 30;
int pos1 = 90;
 
void setup() { 
  servo0.attach(8);
  servo0.write(pos0);

  servo1.attach(10);
  servo1.write(pos1);

  Serial.begin(9600);
}


void loop() {
  if(Serial.available() > 0) {
    char in = Serial.read();
    switch(in) {
      case 0: {
        pos0 = 30;
        break;
      }
      case 1: {
        pos0 = 90;
        break;
      }
      case 2: {
        pos1 = 90;
        break;
      }
      case 3: {
        pos1 = 30;
        break;
      }
      default: {
        pos0 = 30;
        pos1 = 90;
        break;
      }
    }
    servo0.write(pos0);
    delay(15);
    servo1.write(pos1);
    delay(15);
  }
}

