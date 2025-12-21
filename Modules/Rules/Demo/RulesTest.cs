using HitTrax.CoreUtilities;
using HitTrax.GlobalMessagingService;
using HitTrax.ErrorHandling;
using UnityEngine;

namespace HitTrax.Rules.Demo
{
    public static class ErrorCatalogueExample
    {
        public static LogInfo ErrorFailedColTest => Services.Get<IErrorLogService_v1>().Error(10, "FailedColTest", "You are failure.");
    }

    public class MsgCountIncreased : IMessageKey<int> { } 

    public class RulesTest : MonoBehaviour
    {
        private IMessageService_v1 MessageService => Services.Get<IMessageService_v1>();

        public float reqVel;

        public Material pass;
        public Material fail;

        bool DebugErrors() => true;
        bool DebugSuccesses() => true;

        IRule_v1 _validateBall;
        //float _time = 0;

        int _successCount = 0;

        //GameObject _lastObj;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {           
            _validateBall = Services.Get<IBuildRuleService_v1>()
                .Create<MsgBallCollidedTest, GameObject>(ErrorCatalogueExample.ErrorFailedColTest)
                .SetPredicate(HasReqVelocity)
                .SetOnSuccess(DoSuccess)
                .SetOnFail(DoFail)
                //.LogIf(obj => ((int)Velocity(obj)) % 2 == 0)                
                .LogFailAsError(ErrorLog)
                .LogFailIf(DebugErrors)
                .LogSuccessIf(DebugSuccesses)
                .SetSuccessLog(SuccessLog)
                .ReleaseWhen<MsgCountIncreased, int>(count => count >= 3)
                .SetOnReleaseAction(() => "Release MEEEEEEE!".Log())                
                .Rule                
                ;
        }

        private void DoSuccess(GameObject obj)
        {
            SetMat(obj, pass);
            _successCount++;
            MessageService.Raise<MsgCountIncreased, int>(_successCount);
        }

        private void DoFail(GameObject obj, LogInfo info)
        {
            SetMat(obj, fail);
        }

        private string ErrorLog(GameObject obj) => $"FAIL | Velocity = {(int)Velocity(obj)}";
        private string SuccessLog(GameObject obj) => $"SUCCESS | Velocity = {(int)Velocity(obj)}";      

        bool CheckRandom()
            => UnityEngine.Random.Range(0, 100) > 50;

        void SetMat(GameObject obj, Material mat)
        {
            obj.GetComponent<Renderer>().material = mat;    
        }

        float Velocity(GameObject obj) {
            return obj.GetComponent<Rigidbody>().linearVelocity.z;
        }

        bool HasReqVelocity(GameObject obj)
        {
            return Velocity(obj) > reqVel;
        }

    }
}
