using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XGame.State;

namespace XClient.Login.State
{
    internal class StateSwitchValidator : IStateSwitchValidator
    {
        public bool IsTargetValid(int newStateID, object stateContext = null)
        {
            return true;
        }

        public void OnCreate(IStateMachine machine)
        {
        }

        public void OnRelease()
        {
        }
    }
}
