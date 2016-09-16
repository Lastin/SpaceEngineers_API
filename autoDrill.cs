enum State {Start, Expanding, Switching, Contracting};        
enum Side {Left, Right};        
State state;   
Side side;   
public Program() {   
    IMyPistonBase pistonR = (IMyPistonBase)GridTerminalSystem.GetBlockWithName("PistonR");   
    IMyPistonBase pistonL = (IMyPistonBase)GridTerminalSystem.GetBlockWithName("PistonL");        
    IMyShipMergeBlock mergeRight = (IMyShipMergeBlock)GridTerminalSystem.GetBlockWithName("MergeRight");          
    IMyShipMergeBlock mergeLeft = (IMyShipMergeBlock)GridTerminalSystem.GetBlockWithName("MergeLeft");      
    mergeRight.ApplyAction("OnOff_On");   
    mergeLeft.ApplyAction("OnOff_On");   
    pistonR.SetValue("Velocity", -1.0f);   
    pistonL.SetValue("Velocity", -1.0f);   
    //know your limits   
    pistonR.SetValue("LowerLimit", 0f);   
    pistonL.SetValue("LowerLimit", 0f);   
    pistonR.SetValue("UpperLimit", 10f);   
    pistonL.SetValue("UpperLimit", 10f);   
    //   
    mergeRight.ApplyAction("OnOff_Off");   
    state = State.Expanding;   
    side = Side.Left;   
    //set timer   
    IMyTimerBlock timer = (IMyTimerBlock)GridTerminalSystem.GetBlockWithName("Timer Block");   
    timer.SetValue("TriggerDelay", 10f);   
    timer.ApplyAction("Start");   
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
        
    IMyTextPanel lcd = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("LCD");        
    try{        
    //testing lcd            
    IMyPistonBase pistonL = (IMyPistonBase)GridTerminalSystem.GetBlockWithName("PistonL");        
    IMyPistonBase pistonR = (IMyPistonBase)GridTerminalSystem.GetBlockWithName("PistonR");          
    float positionL = pistonL.CurrentPosition;          
    float positionR = pistonR.CurrentPosition;          
    IMyShipMergeBlock mergeRight = (IMyShipMergeBlock)GridTerminalSystem.GetBlockWithName("MergeRight");          
    IMyShipMergeBlock mergeLeft = (IMyShipMergeBlock)GridTerminalSystem.GetBlockWithName("MergeLeft");      
    IMyTimerBlock timer = (IMyTimerBlock)GridTerminalSystem.GetBlockWithName("Timer Block");      
    //print info on LCD        
    lcd.WritePublicText("");        
    lcd.WritePublicText("Position Left: " + positionL.ToString() + "\n", true);        
    lcd.WritePublicText("Position Right: " + positionR.ToString() + "\n", true);        
    lcd.WritePublicText("State: " + state .ToString() + "\n", true);        
    lcd.WritePublicText("Left/Right: " + side.ToString() + "\n", true);       
    lcd.ShowPublicTextOnScreen();        
    if(state == State.Expanding) {        
        if(side == Side.Left) {        
            pistonL.SetValue("UpperLimit", 10.0f);        
            pistonL.SetValue("Velocity", 0.1f);        
            if(nearlyEqual(positionL, 10.0f)) {     
                state = State.Switching;     
                side = Side.Right;     
            }        
        }        
        else if(side == Side.Right) {        
            pistonR.SetValue("UpperLimit", 10.0f);        
            pistonR.SetValue("Velocity", 0.1f);        
            if(nearlyEqual(positionR, 10.0f)) {             
                state = State.Switching;     
                side = Side.Left;     
            }        
        }        
    }        
    if(state == State.Switching) {        
        if(side == Side.Right) {     
            mergeRight.ApplyAction("OnOff_On");     
            pistonR.SetValue("UpperLimit", 2.5f);        
            pistonR.SetValue("Velocity", 1.0f);     
            if(nearlyEqual(positionR, 2.5f)) {   
                timer.SetValue("TriggerDelay", 1f);   
                timer.ApplyAction("Start");   
                state = State.Contracting;     
                side = Side.Left;     
            }     
        }     
        else if(side == Side.Left) {     
            mergeLeft.ApplyAction("OnOff_On");     
            pistonL.SetValue("UpperLimit", 2.5f);        
            pistonL.SetValue("Velocity", 1.0f);     
            if(nearlyEqual(positionL, 2.5f)) {   
                timer.SetValue("TriggerDelay", 1f);   
                timer.ApplyAction("Start");   
                state = State.Contracting;     
                side = Side.Right;     
            }     
        }     
    }        
    if(state == State.Contracting) {        
        if(side == Side.Left) {     
            mergeLeft.ApplyAction("OnOff_Off");     
            pistonL.SetValue("Velocity", -1.0f);        
            if(nearlyEqual(positionL, 0.0f)) {        
                state = State.Expanding;        
                side = Side.Right;        
            }        
        }        
        else if(side == Side.Right) {     
            mergeRight.ApplyAction("OnOff_Off");     
            pistonR.SetValue("Velocity", -1.0f);        
            if(nearlyEqual(positionL, 0.0f)) {        
                state = State.Expanding;        
                side = Side.Left;        
            }        
        }      
           
    }   
        timer.SetValue("TriggerDelay", 1f);   
        timer.ApplyAction("Start");   
    } catch (Exception e) {        
        lcd.WritePublicText(e.ToString());        
        lcd.ShowPublicTextOnScreen();        
    }      
          
}  
  
bool nearlyEqual(float a, float b) {  
    float tolerance = 0.01f;  
    float diff = a-b >= 0 ? a-b : a-b*-1.0f;  
    return diff <= tolerance;  
}