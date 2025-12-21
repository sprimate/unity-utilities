// DO NOT CHANGE THE NAMESPACE OF THIS CLASS OR YOUR MODULE WILL BREAK 
using HitTrax.CoreUtilities;

namespace HitTrax.Rules
{
    // DO NOT CHANGE THE NAME OF THIS CLASS OR YOUR MODULE WILL BREAK
    public static class RulesLoader
    {
        // DO NOT CHANGE THE NAME OF THIS FUNCTION OR YOUR MODULE WILL BREAK
        public static void Load()
        {
            // This is where you can initialize your module            
            Services.RegisterSingleton(new BuildRuleService());
        }
    }
}