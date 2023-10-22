using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OALProgramControl
{
    public class EXEASTNodeMethodCall : EXEASTNodeBase
    {
        public readonly string MethodName;
        public readonly List<EXEASTNodeBase> Arguments;
        public CDMethod Method { get; private set; }
        public EXEValueBase OwningObject { get; private set; }
        private EXEExecutionResult evaluationResult { get; set; }
        public override EXEExecutionResult EvaluationResult
        {
            get => this.EvaluationResult;
            set
            {
                this.evaluationResult = value;

                if (this.evaluationResult != null && this.evaluationResult.IsDone)
                {
                    this.EvaluationState = EEvaluationState.HasBeenEvaluated;
                }
            }
        }

        public EXEASTNodeMethodCall(string methodName) : base()
        {
            this.MethodName = methodName;
            this.Arguments = new List<EXEASTNodeBase>();
            this.Method = null;
        }

        public override EXEExecutionResult Evaluate(EXEScope currentScope, OALProgram currentProgramInstance, EXEASTNodeAccessChainContext valueContext = null)
        {
            if (this.EvaluationState == EEvaluationState.HasBeenEvaluated)
            {
                return this.EvaluationResult;
            }

            this.EvaluationState = EEvaluationState.IsBeingEvaluated;

            if (valueContext == null || valueContext.CurrentValue == null)
            {
                // We want to access a method of the current method owning object
                EXEVariable selfVariable = currentScope.FindVariable(EXETypes.SelfReferenceName);
                OwningObject = selfVariable.Value;
            }
            else
            {
                // We want to access a method of an object given by the context
                OwningObject = valueContext.CurrentValue;
            }

            if (OwningObject.MethodExists(this.MethodName))
            {
                // Method exists
                Method = OwningObject.FindMethod(this.MethodName);
            }
            else
            {
                // Method does not exist, time to raise an error
                return EXEExecutionResult.Error(ErrorMessage.MethodNotFoundOnClass(this.MethodName, this.OwningObject.TypeName), "XEC2015");
            }

            if (this.Arguments.Count != this.Method.Parameters.Count)
            {
                return EXEExecutionResult.Error
                    (
                        ErrorMessage.InvalidParameterCount
                        (
                            this.Method.OwningClass.Name,
                            this.MethodName,
                            this.Method.Parameters.Count,
                            this.Arguments.Count
                        ),
                        "XEC2016"
                    );
            }

            // Now, let us evaluate the args and invoke the method
            EXEExecutionResult argumentExecutionResult;
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                argumentExecutionResult = this.Arguments[i].Evaluate(currentScope, currentProgramInstance);

                if (!argumentExecutionResult.IsSuccess)
                {
                    this.EvaluationState = EEvaluationState.HasBeenEvaluated;
                    this.EvaluationResult = argumentExecutionResult;
                }

                if (!argumentExecutionResult.IsDone || !argumentExecutionResult.IsSuccess)
                {
                    // Either current argument evaluation did not finish or produced an error
                    return argumentExecutionResult;
                }

                // Current argument evaluation finished and did not produce an error

                // Check if the returned value matches the expected parameter type
                if (!EXETypes.CanBeAssignedTo(argumentExecutionResult.ReturnedOutput, this.Method.Parameters[i].Type, currentProgramInstance.ExecutionSpace))
                {
                    return EXEExecutionResult.Error
                    (
                        ErrorMessage.InvalidParameterValue
                        (
                            this.Method.OwningClass.Name,
                            this.MethodName,
                            this.Method.Parameters[i].Name,
                            this.Method.Parameters[i].Type, argumentExecutionResult.ReturnedOutput.ToText()
                        ),
                        "XEC2017"
                    );
                }
            }

            // All arguments have been evaluated and without an error
            this.EvaluationResult = EXEExecutionResult.Success();
            this.EvaluationResult.PendingCommand = new EXECommandCallPreEvaluated(OwningObject, this);
            return this.EvaluationResult;
        }

        public override void PrintPretty(string indent, bool last)
        {
            throw new System.NotImplementedException();
        }

        public override string ToCode()
        {
            throw new System.NotImplementedException();
        }

        public override EXEASTNodeBase Clone()
        {
            return new EXEASTNodeMethodCall(this.MethodName);
        }
    }
}
