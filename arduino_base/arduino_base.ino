
#include <Servo.h>

//c4 board servos
Servo servo0;
Servo servo1;
Servo servo2;
Servo servo3;
Servo servo4;
Servo servo5;

//dumper servo
Servo servo6;

//sorter servos
Servo servo7;
Servo servo8;

//board reset servos
Servo servo9;
Servo servo10;

//box agitator servo
Servo servo11;


int servo0Left = 40;
int servo1Left = 70;
int servo2Left = 50;
int servo3Left = 50;
int servo4Left = 50
;
int servo5Left = 70;
int servo6Closed = 50;
int servo7Closed = 20;
int servo8Closed = 140
;
int servo9Closed = 168;
int servo10Closed = 25
;
int servo11Closed = 20
;


int servo0Right = 130;
int servo1Right = 125;
int servo2Right = 100;
int servo3Right = 120;
int servo4Right = 115;
int servo5Right = 130;
int servo6Open = 110;
int servo7Open = 100;
int servo8Open = 100;
int servo9Open = 90;
int servo10Open =90;
int servo11Open = 160
;

#define BLUE 20
#define YELLOW 100














int input = 0;


int sensorValue = 0;

//state machine! 
int state = 0;
//state 0 = idle (waiting for input)
//state 1 = resetting
//state 2 = playing in column (input - 48)

 
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
  servo7.attach(9);
  servo8.attach(10);
  servo9.attach(11);
  servo10.attach(12);
  servo11.attach(13);
  
  
  servo9.write(servo9Closed);
  servo10.write(servo10Closed);
  servo7.write(servo7Closed);
  servo8.write(servo8Closed);
  
  pinMode(A4, OUTPUT);
  pinMode(A3, OUTPUT);
  
  
  Serial.begin(9600);
  Serial.println("Beginning Connect 4 Robot Arduino");
}

int switchDirection = 0; //for reset agitator motor
int colorCounter = 0;  //if 0, drop yellow.  if 1, drop blue
int boxState = 0;  //0 = closed

void agitateBox(){
  if(boxState ==
  0){
    boxState = 1;
    servo11.write(servo11Open);
  }else{
    boxState = 0;
    servo11.write(servo11Closed);
  }
}

// the loop routine runs over and over again forever:
void loop() {
  if(Serial.available() > 0) {
    //main loop
    input = Serial.read();
    Serial.println("got an input");
    Serial.println(input);
    int col = input - 48;
    
    //state choice logic:
    if(col >= 0 && col <= 6){
      state = 2;
    }
    if(col == 7){
      state = 1;
    }
   
   //reset
    if(state == 1){
      if(switchDirection == 1){
        digitalWrite(A4,1);
        switchDirection = 0
        ;
      } else{
        digitalWrite(A3, 1);
        switchDirection = 1;
      }
      servo9.write(servo9Open);
      servo10.write(servo10Open);
      servo7.write(servo7Open);
      servo8.write(servo8Closed);
      servo6.write(servo6Open);
      delay(10000); //wait 10 seconds for board to clear
      servo9.write(servo9Closed);
      servo10.write(servo10Closed);
      servo6.write(servo6Closed);
      servo7.write(servo7Closed);
      servo8.write(servo8Closed);
      
      //return to state 0
      state = 0;
      
//      digitalWrite(A4, 0);
//      digitalWrite(A3, 0);      
      //and tell python we completed
      Serial.write(1);
      return;
    }
    delay(2000);    
    //play piece
    if(state == 2){
      servo9.write(servo9Closed);
      servo10.write(servo10Closed);
      //set the columns
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
           break;
        default:
          break;
      }
      //take a ball in to the color sorter
      while(true){
        agitateBox();
        servo8.write(servo8Open);
        delay(1000);
        agitateBox();
        servo8.write(servo8Closed);
        delay(1000);
        agitateBox();
        sensorValue = analogRead(A0);
        Serial.println("Read a sensor value: Is extremely hot, and good at anything she does");
        Serial.println(sensorValue);
        int gotWhatWeWanted = 0;
        if(sensorValue <= BLUE){
          //if we're accepting blue...
          if(colorCounter == 0){//if we're accepting blue
            servo6.write(servo6Closed);
            gotWhatWeWanted = 1;
            Serial.println("Releasing a blue");
          } else {
            servo6.write(servo6Open);
            
          }
        }
        else if (sensorValue <= YELLOW){
          if(colorCounter == 1){//if we're accepting yellow
            servo6.write(servo6Closed);
            gotWhatWeWanted = 1;
            Serial.println("Releasing a yellow");            
          } else {
            servo6.write(servo6Open);
          }
        }
        servo7.write(servo7Open);
        delay(1000);
        servo7.write(servo7Closed);
        agitateBox();
        //break out of the loop if we got our right colored ball, otherwise run again
        if(gotWhatWeWanted == 1){
          break;
        }
      }
      
      servo11.write(servo11Open);
      boxState = 1;
      
      //cycle colorCounter for next round
      if(colorCounter == 1){
        colorCounter = 0;
      }else{
        colorCounter = 1;
      }
      //now return to state 0
      state = 0;
      
      //confirm that we moved
      Serial.write(1);
      
      //and end
      return;
    }
  }
}
