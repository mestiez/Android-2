using System;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Applying this attribute to a property will make it show up in the client
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UiVariableTypeAttribute : Attribute
    {
        /// <summary>
        /// Type of the value
        /// </summary>
        public VariableType VariableType { get; }

        /// <summary>
        /// Tells the client that this is en editable value with a type
        /// </summary>
        /// <param name="variableType"></param>
        public UiVariableTypeAttribute(VariableType variableType)
        {
            VariableType = variableType;
        }
    }
}
