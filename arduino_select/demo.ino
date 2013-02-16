#include <Servo.h> 
 
Servo myservo;
Servo myservo1;

int pos = 30;
int pos1 = 90;
 
void setup() { 
  myservo.attach(8);
  myservo1.attach(10);  // attaches the servo on pin 9 to the servo object 
}  
 
 
void loop() { 
  myservo.write(30);
  delay(2000);
  myservo.write(90);
  delay(2000);
  myservo.write(30);
  delay(2000);
  
  myservo1.write(90);
  delay(2000);
  myservo1.write(30);
  delay(2000);
  myservo1.write(90);
  delay(2000);
  /*
  myservo1.write(30);
  delay(2000);
  myservo1.write(90);
  delay(2000);
  myservo1.write(30);
  delay(2000);
  /*for(pos = 30; pos < 90; pos += 1) {
    myservo.write(pos);
    delay(50); 
  } 
  for(pos = 90; pos>=30; pos-=1) {
    myservo.write(pos);
    delay(50);
  }
  delay(2000); 
  for(pos1 = 30; pos1 < 90; pos1 += 1) {
    myservo1.write(pos1);
    delay(50); 
  } 
  
  for(pos1 = 90; pos1>=30; pos1-=1) {
    myservo1.write(pos1);
    delay(50);
  } */
} 
