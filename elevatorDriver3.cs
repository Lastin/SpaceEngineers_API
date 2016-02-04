//////////////////////////////
//VARIABLES
/////////////////////////////
bool initialised = false;
IMyTimerBlock accTimer;
IMyTimerBlock distTimer;
IMyTimerBlock speedTimer;
IMyRemoteControl rc;
List <int> floorsQueue = new List<int>();
string direction = "";
int targetFloor;
bool running = false;
double lastY;
double speed;
//aka int array-method, because arrays don't work
double floors(int i){
    switch(i){
        case 0: return 437;
        case 1: return 424.9;
        case 2: return 404.8;
        case 3: return 392.4;
        case 4: return 379.8;
        case 5: return 367.45;
        case 6: return 354.2;
        case 7: return 342.38;
        case 8: return 327.37;
    }
    return (int)getCurrentY();
}
////////////////////////////

void Main(string argument){
    if(!initialised){
      initialiseVariables();
    }
    if(argument == "test"){
        test();
    }
    else if(argument == "stopAcc"){
        stopAcc();
    }
    else if(argument == "checkDistance"){
        checkDistance();
    }
    else if(argument == "updateSpeed"){
        updateSpeed();
    }
    else {
        int floor = Int16.Parse(argument);
        if(floorsQueue.Count == 0){
            flushVariables();
        }
        addToQueue(floor);
        if(!running){
            running = true;
            triggerSpeedTimer();
            start();
        }
    }
}

void initialiseVariables(){
  accTimer = GridTerminalSystem.GetBlockWithName("accTimer") as IMyTimerBlock;
  distTimer = GridTerminalSystem.GetBlockWithName("distanceTimer") as IMyTimerBlock;
  speedTimer = GridTerminalSystem.GetBlockWithName("speedTimer") as IMyTimerBlock;
  rc = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;
  initialised = true;
  lastY = getCurrentY();
}

void flushVariables(){
    direction = "";
    running = false;
    targetFloor = -1;
    floorsQueue.Clear();
}

/////////////////////////////////
//operation sequence section
/////////////////////////////////

void start(){
    lastY = getCurrentY();
    targetFloor = getNearestTarget();
    direction = getMoveDirection(targetFloor);
    print("Current Floor: " + getNearestFloor() + "\r\nNext: " + targetFloor + "\r\nLift going " + direction);
    move();
}

void addToQueue(int floor){
    for(int i=0; i<floorsQueue.Count; i++){
        if(floor == floorsQueue[i]){
            return;
        }
    }
    floorsQueue.Add(floor);
    floorsQueue.Sort();
}

void goNext(){
    if(floorsQueue.Count == 0){
      flushVariables();
      stopTimers();
      return;
    }
    int next = getNextFromQueue();
    if(next < 0){
      if(direction == "up"){
        direction = "down";
      }
      else {
        direction = "up";
      }
      goNext();
    }
    targetFloor = next;
    move();
}

/////////////////////
//motion section
/////////////////////

void move(){
    gainSpeed(direction);
    triggerDistanceChecker();
}

void gainSpeed(string direction){
    switchDampeners(false);
    overrideThrusters(direction, 12000);
    accTimer.SetValueFloat("TriggerDelay", 3);
    accTimer.ApplyAction("Start");
}

void stop(){
    stopAcc();
    switchDampeners(true);
}

void switchDampeners(bool mode){
    rc.SetValueBool("DampenersOverride", mode);
}

void overrideThrusters(string direction, Single force){
    List<IMyTerminalBlock> thrusters = new List<IMyTerminalBlock>();
    GridTerminalSystem.SearchBlocksOfName(direction, thrusters);
    for(int i=0; i<thrusters.Count; i++){
        thrusters[i].SetValue<Single>("Override", force);
    }
}

void stopAcc(){
    overrideThrusters("up", 0);
    overrideThrusters("down", 0);
}



//////////////////////////
//timed events section
/////////////////////////
void triggerDistanceChecker(){
    distTimer.SetValueFloat("TriggerDelay", 0.1f);
    distTimer.ApplyAction("Start");
}

void checkDistance(){
    double correction = -0.9 * speed;
    if(direction == "down"){
      correction = 0.5 * speed;
    }
    double currY = getCurrentY();
    if(isCloseEnough(currY, floors(targetFloor), correction)){
        stop();
        floorsQueue.Remove(targetFloor);
        goNext();
    }
    else {
        triggerDistanceChecker();
    }
}

void updateSpeed(){
  double currY = getCurrentY();
  double diff = lastY - currY;
  lastY = currY;
  speed =  diff;
  speedTimer.ApplyAction("Start");
  //print(speed.ToString());
  print(floorsQueue.ToString());
}

void triggerSpeedTimer(){
  speedTimer.SetValueFloat("TriggerDelay", 0.1f);
  speedTimer.ApplyAction("Start");
}

void stopTimers(){
  accTimer.ApplyAction("Stop");
  distTimer.ApplyAction("Stop");
  speedTimer.ApplyAction("Stop");
}

/////////////////////////////
//useful functions section
/////////////////////////////
double getCurrentY(){
    IMyRemoteControl rc = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;
    return rc.GetPosition().GetDim(1);
}

bool isCloseEnough(double num1, double num2, double correction){
    double tolerance = 1.0;
    double diff = Math.Abs(num1 - num2) + correction;
    return (diff < tolerance);
}

int getNearestFloor(){
    double currY = getCurrentY();
    double distance = Math.Abs(currY - floors(0));
    int nearestFloor = 0;
    for(int i=1; i<9; i++){
        double tempD = Math.Abs(currY - floors(i));
        if(tempD < distance){
            distance = tempD;
            nearestFloor = i;
        }
    }
    return nearestFloor;
}

int getNearestTarget(){
  double currY = getCurrentY();
  int nearestTarget = 0;
  double smallest = Math.Abs(floors(floorsQueue[0]) - currY);
  for(int i=1; i<floorsQueue.Count; i++){
    double temp = Math.Abs(floors(i) - currY);
    if(temp < smallest){
      smallest = temp;
      nearestTarget = i;
    }
  }
  return floorsQueue[nearestTarget];
}

string getMoveDirection(int targetFloor){
  double targetY = floors(targetFloor);
  double currY = getCurrentY();
  if(targetY > currY){
    return "down";
  }
  return "up";
}

int getNextFromQueue(){
  bool up = direction == "up";
  double currY = getCurrentY();
  for(int i=0; i<floorsQueue.Count; i++){
    double tempY = floors(floorsQueue[i]);
    if(up && currY > tempY){
      return floorsQueue[i];
    }
    else if(!up && currY < tempY){
      return floorsQueue[i];
    }
  }
  return -1;
}


////////////////////////////////////////////////
//Display Section
////////////////////////////////////////////////
void print(string text){
    List<IMyTerminalBlock> panels = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(panels);
    for(int i=0; i<panels.Count; i++){
        ((IMyTextPanel)panels[i]).WritePublicText(text);
        ((IMyTextPanel)panels[i]).ShowPublicTextOnScreen();
    }
}


////////////////////////////////////////////////
//TESTING SECTION
////////////////////////////////////////////////
void test(){
    initialiseVariables();
    triggerSpeedTimer();
    stop();
}
