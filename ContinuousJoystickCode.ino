#include<Keyboard.h>

const int upPin = 6;
const int downPin = 5;
const int leftPin = 4;
const int rightPin = 3;
int del=15;
void setup() {
  pinMode(upPin, INPUT_PULLUP);
  pinMode(downPin, INPUT_PULLUP);
  pinMode(leftPin, INPUT_PULLUP);
  pinMode(rightPin, INPUT_PULLUP);
}

void loop() {
  String s="";
  if (digitalRead(upPin) == LOW){ Keyboard.press(KEY_UP_ARROW); delay(del); Keyboard.release(KEY_UP_ARROW);}
  if (digitalRead(downPin) == LOW) {Keyboard.press(KEY_DOWN_ARROW); delay(del); Keyboard.release(KEY_DOWN_ARROW);}
  if (digitalRead(leftPin) == LOW) {Keyboard.press(KEY_LEFT_ARROW); delay(del); Keyboard.release(KEY_LEFT_ARROW);}
  if (digitalRead(rightPin) == LOW) {Keyboard.press(KEY_RIGHT_ARROW); delay(del); Keyboard.release(KEY_RIGHT_ARROW);}
  delay(del);
}