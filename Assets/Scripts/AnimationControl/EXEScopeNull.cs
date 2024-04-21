﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OALProgramControl
{
    public class EXEScopeNull : EXEScopeBase
    {
        public EXEScopeNull() : base()
        {
        }
        
        public override IEnumerable<EXEScopeBase> ScopesToTop()
        {
            yield break;
        }
        
        protected override EXEExecutionResult Execute(OALProgram OALProgram) 
        {
            return EXEExecutionResult.Success();
        }
        
        public override Dictionary<string, string> AllDeclaredVariables()
        {
            return new Dictionary<string, string>();
        }
        
        protected override void AddCommandsToStack(List<EXECommand> Commands)
        {
        }
        
        public override EXEExecutionResult AddVariable(EXEVariable variable)
        {
            return EXEExecutionResult.Success();
        }
        
        public override bool VariableExists(String seekedVariableName)
        {
            return FindVariable(seekedVariableName) != null;
        }
        
        public override EXEVariable FindVariable(String seekedVariableName)
        {
            return null;
        }       
        
        public override Boolean IsComposite()
        {
            return true;
        }
        
        public override EXECommand CreateClone()
        {
            return new EXEScopeNull();
        }

        public override void ToggleActiveRecursiveBottomUp(bool active)
        {
        }
    }
}