public enum Direction {Up, Down, Stop}
enum Sensor {Above, Below}
Direction currentDir;
int currentFloor;
int targetFloor;
List<IMyTerminalBlock> clockwise = new List<IMyTerminalBlock>(); 
List<IMyTerminalBlock> anticlockwise = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> lcds = new List<IMyTerminalBlock>();
IMyTimerBlock timer;
public Program() {
    timer = (IMyTimerBlock)GridTerminalSystem.GetBlockWithName("timer1");
    currentFloor = 0;
    currentDir = Direction.Stop;
    List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>(); 
    GridTerminalSystem.GetBlocks(allBlocks); 
    for(int i=0; i<allBlocks.Count; i++) { 
        if(allBlocks[i].CustomName == "AntiClockwise") { 
            anticlockwise.Add(allBlocks[i]); 
        } else if(allBlocks[i].CustomName == "Clockwise") { 
            clockwise.Add(allBlocks[i]); 
        } else if(allBlocks[i].CustomName == "LCD Panel") {
            lcds.Add(allBlocks[i]);
        }
    }
}

public void Save() {
    // Called when the program needs to save its state. Use
    // this method to save your state to the Storage field
    // or some other means. 
    // 
    // This method is optional and can be removed if not
    // needed.
}

public void Main(string argument) {
    if(argument.ToLower() == "up"){
        move(Direction.Up);
    } else if(argument.ToLower() == "down") {
        move(Direction.Down);
    } else if(argument.ToLower() == "stop" ) {
        move(Direction.Stop);
    } else if(argument.Split('.')[0].ToLower() == "detection") {
        string sensor = argument.Split('.')[1].ToLower();
        if(sensor == "below") {
            detection(Sensor.Below);
        } else {
            detection(Sensor.Above);
        }
    } else if(argument.Split('.')[0].ToLower() == "goto"){
        int target = Int32.Parse(argument.Split('.')[1]);
        goTo(target);
    } 
}

void goTo(int target) {
    targetFloor = target;
    display("Going to floor: " + targetFloor.ToString());
    if(target > currentFloor) {
        move(Direction.Up);
    } else if(target < currentFloor) {
        move(Direction.Down);
    }
}

void detection(Sensor sensor) {
    if(currentDir == Direction.Down && sensor == Sensor.Below) {
        --currentFloor;
    } else if(currentDir == Direction.Up && sensor == Sensor.Above) {
        ++currentFloor;
    }
    if(targetFloor == currentFloor) {
        timer.SetValue("TriggerDelay", 1f);
        timer.ApplyAction("Start");
        //move(Direction.Stop);
    }
    display(currentFloor.ToString());
}

void display(String str) {
    for(int i=0; i<lcds.Count; i++) {
        IMyTextPanel display = (IMyTextPanel)lcds[i];
        display.WritePublicText(str);
        display.ShowPublicTextOnScreen();
    }
}

public void move(Direction direction) {
    currentDir = direction;
    if(direction == Direction.Up) {
        for(int i=0; i<anticlockwise.Count; i++) {   
            anticlockwise[i].SetValue("Velocity", 30f);   
        }
        for(int i=0; i<clockwise.Count; i++) {    
            clockwise[i].SetValue("Velocity", -30f);    
        }
    } else if(direction == Direction.Down) {
        for(int i=0; i<anticlockwise.Count; i++) { 
            anticlockwise[i].SetValue("Velocity", -30f); 
        } 
        for(int i=0; i<clockwise.Count; i++) {  
            clockwise[i].SetValue("Velocity", 30f);  
        }
    } else {
        for(int i=0; i<anticlockwise.Count; i++) {  
            anticlockwise[i].SetValue("Velocity", 0f);  
        }  
        for(int i=0; i<clockwise.Count; i++) {   
            clockwise[i].SetValue("Velocity", 0f);   
        }
    }
}
