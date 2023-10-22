﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace OALProgramControl
{
    public abstract class EXECommand
    {
        public bool IsActive { get; set; } = false;
        protected EXEScope SuperScope { get; set; }
        public EXEExecutionStack CommandStack { get; set; } = null;
        public virtual IEnumerable<EXEScope> ScopesToTop()
        {
            EXEScope currentScope = this.SuperScope;

            while (currentScope != null)
            {
                yield return currentScope;
                currentScope = currentScope.SuperScope;
            }
        }
        public EXEScopeMethod GetCurrentMethodScope()
        {
            return ScopesToTop()
                .FirstOrDefault(scope => scope is EXEScopeMethod)
                as EXEScopeMethod;
        }
        public EXEScopeLoop GetCurrentLoopScope()
        {
            return ScopesToTop()
                .FirstOrDefault(scope => scope is EXEScopeLoop)
                as EXEScopeLoop;
        }
        public EXEExecutionResult PerformExecution(OALProgram OALProgram)
        {
            EXEExecutionResult Result = Execute(OALProgram);

            return Result;
        }
        protected abstract EXEExecutionResult Execute(OALProgram OALProgram);
        public virtual bool CollectReturn(EXEValueBase returnedValue, OALProgram programInstance)
        {
            return false;
        }
        protected EXEExecutionResult Success()
        {
            return EXEExecutionResult.Success(this);
        }
        protected EXEExecutionResult Error(string errorCode, string errorMessage)
        {
            return EXEExecutionResult.Error(errorMessage, errorCode, this);
        }
        public EXEScope GetSuperScope()
        {
            return this.SuperScope;
        }
        public virtual void SetSuperScope(EXEScope SuperScope)
        {
            this.SuperScope = SuperScope;
        }
        public EXEScope GetTopLevelScope()
        {
            EXEScope CurrentScope = this.SuperScope;

            if (CurrentScope == null && this is EXEScope)
            {
                return this as EXEScope;
            }

            while (CurrentScope.SuperScope != null)
            {
                CurrentScope = CurrentScope.SuperScope;
            }

            return CurrentScope;
        }
        public virtual Boolean IsComposite()
        {
            return false;
        }
        public abstract EXECommand CreateClone();
        public virtual String ToCode(String Indent = "")
        {
            return Indent + ToCodeSimple() + ";\n";
        }
        public virtual String ToCodeSimple()
        {
            return "Command";
        }
        public virtual string ToFormattedCode(String Indent = "")
        {
            return HighlightCodeIf(IsActive, ToCode(Indent));
        }
        protected string HighlightCodeIf(bool condition, string code)
        {
            return condition ? HighlightCode(code) : code;
        }
        private string HighlightCode(string code)
        {
            return "<b><color=green>" + code + "</color></b>";
        }
        public void ToggleActiveRecursiveBottomUp(bool active)
        {
            this.IsActive = active;

            if (this.SuperScope != null)
            {
                this.SuperScope.ToggleActiveRecursiveBottomUp(active);
            }
        }
        /**Use this when evaluating AST nodes and Execute might need to be called again.**/
        protected bool HandleRepeatableASTEvaluation(EXEExecutionResult executionResult)
        {
            if (!executionResult.IsDone)
            {
                // It's a stack-like structure, so we enqueue the current command first, then the pending command.
                this.CommandStack.Enqueue(this);
                this.CommandStack.Enqueue(executionResult.PendingCommand);
                return false;
            }

            if (!executionResult.IsSuccess)
            {
                executionResult.OwningCommand = this;
                return false;
            }

            return true;
        }
        /**Use this when performing an action and Execute cannot be called again.**/
        protected bool HandleSingleShotASTEvaluation(EXEExecutionResult executionResult)
        {
            executionResult.OwningCommand = this;

            if (!executionResult.IsDone)
            {
                executionResult.IsSuccess = false;
                executionResult.ErrorMessage = "Unexpected request to re-evaluate during command execution";
                executionResult.ErrorCode = "XEC2024";
                return false;
            }

            return executionResult.IsSuccess;
        }
    }
}
