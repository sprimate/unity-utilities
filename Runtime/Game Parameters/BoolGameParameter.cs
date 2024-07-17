public class BoolGameParameter : GameParameter<bool> 
{
    public BoolGameParameter(bool val) : base(val) { }
    public BoolGameParameter() : this(default) { }//required for inspector
}
