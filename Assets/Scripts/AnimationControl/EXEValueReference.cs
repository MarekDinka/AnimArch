﻿using System;
using UnityEditor;
using UnityEngine;

namespace OALProgramControl
{
    public class EXEValueReference : EXEValueBase
    {
        public override string TypeName => ClassInstance.OwningClass.Name;
        public override bool CanHaveAttributes => true;
        public override bool CanHaveMethods => true;
        public CDClass TypeClass { get; protected set; }
        public CDClassInstance ClassInstance { get; protected set; }
        public EXEValueReference()
        {
            this.ClassInstance = null;
            this.TypeClass = null;
        }
        public EXEValueReference(CDClass typeClass)
        {
            this.ClassInstance = null;
            this.TypeClass = typeClass;
        }
        public EXEValueReference(CDClassInstance classInstance)
        {
            this.ClassInstance = null;
            this.TypeClass = classInstance.OwningClass;
            this.ClassInstance = classInstance;
        }
        public EXEValueReference(EXEValueReference original)
        {
            CopyValues(original, this);
        }
        public override EXEValueBase DeepClone()
        {
            return new EXEValueReference(this);
        }
        public override string ToText()
        {
            throw new NotImplementedException();
        }
        public override bool AttributeExists(string attributeName)
        {
            if (!this.WasInitialized)
            {
                return false;
            }

            return this.ClassInstance.OwningClass.GetAttributeByName(attributeName) != null;
        }
        public override bool MethodExists(string methodName)
        {
            if (!this.WasInitialized)
            {
                return false;
            }

            return FindMethod(methodName) != null;
        }
        public override EXEExecutionResult RetrieveAttributeValue(string attributeName)
        {
            if (!this.WasInitialized)
            {
                return UninitializedError();
            }

            EXEValueBase attributeValue = this.ClassInstance.GetAttributeValue(attributeName);

            if (attributeValue == null)
            {
                return EXEExecutionResult.Error(ErrorMessage.AttributeNotFoundOnClass(attributeName, ClassInstance.OwningClass), "XEC2005");
            }

            EXEExecutionResult executionResult = EXEExecutionResult.Success();
            executionResult.ReturnedOutput = attributeValue;
            return executionResult;
        }
        public override CDMethod FindMethod(string methodName)
        {
            if (!this.WasInitialized)
            {
                return null;
            }

            return this.ClassInstance.OwningClass.GetMethodByName(methodName);
        }
        public override EXEExecutionResult AssignValueFrom(EXEValueBase assignmentSource)
        {
            return assignmentSource.AssignValueTo(this);
        }
        public override EXEExecutionResult AssignValueTo(EXEValueReference assignmentTarget)
        {
            if (!this.WasInitialized)
            {
                return UninitializedError();
            }

            if (this.TypeClass != null && this.ClassInstance == null)
            {
                if (this.TypeClass.CanBeAssignedTo(assignmentTarget.TypeClass))
                {
                    CopyValues(this, assignmentTarget);
                    this.WasInitialized = true;

                    return EXEExecutionResult.Success();
                }
                else
                {
                    return base.AssignValueTo(assignmentTarget);
                }
            }
            else if (!this.ClassInstance.OwningClass.CanBeAssignedTo(assignmentTarget.ClassInstance.OwningClass))
            {
                return base.AssignValueTo(assignmentTarget);
            }

            CopyValues(this, assignmentTarget);

            return EXEExecutionResult.Success();
        }
        private void CopyValues(EXEValueReference source, EXEValueReference target)
        {
            target.ClassInstance = source.ClassInstance;
            target.TypeClass = source.TypeClass;
        }
        public override EXEExecutionResult ApplyOperator(string operation)
        {
            if (!this.WasInitialized)
            {
                return UninitializedError();
            }

            EXEExecutionResult result = null;

            if ("cardinality".Equals(operation))
            {
                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueInt(this.ClassInstance == null ? 0 : 1);
                return result;
            }
            else if ("empty".Equals(operation))
            {
                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.ClassInstance == null);
                return result;
            }
            else if ("not_empty".Equals(operation))
            {
                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.ClassInstance != null);
                return result;
            }

            return base.ApplyOperator(operation);
        }
        public override EXEExecutionResult ApplyOperator(string operation, EXEValueBase operand)
        {
            if (!this.WasInitialized || !operand.WasInitialized)
            {
                return UninitializedError();
            }

            EXEExecutionResult result = null;

            if ("==".Equals(operation))
            {
                if (operand is not EXEValueReference)
                {
                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.ClassInstance?.UniqueID == (operand as EXEValueReference).ClassInstance?.UniqueID);
                return result;
            }
            else if ("!=".Equals(operation))
            {
                if (operand is not EXEValueString)
                {
                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.ClassInstance?.UniqueID != (operand as EXEValueReference).ClassInstance?.UniqueID);
                return result;
            }

            return base.ApplyOperator(operation, operand);
        }
        public void Dereference()
        {
            this.ClassInstance = null;
        }

        public override string ToObjectDiagramText()
        {
            return this.ClassInstance.UniqueID.ToString();
        }
    }
}