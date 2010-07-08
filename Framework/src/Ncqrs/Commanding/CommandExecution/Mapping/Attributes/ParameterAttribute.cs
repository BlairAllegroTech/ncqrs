﻿using System;

namespace Ncqrs.Commanding.CommandExecution.Mapping.Attributes
{
    /// <summary>
    /// Specifies the parameter information of where the command property
    /// maps to. This can be either by ordinal or by name. Where if ordinal 
    /// is used, name will be ignored.
    /// </summary>
    public class ParameterAttribute : Attribute
    {
        private int? _ordinal;
        private string _name;

        /// <summary>
        /// Gets or sets the ordinal of the parameter that is used. The ordinal numbers start by one (<c>1</c>).
        /// </summary>
        public int? Ordinal
        {
            get { return _ordinal; }
            set { _ordinal = value; }
        }

        /// <summary>
        /// Gets or sets the name of the parameter that the property maps to.
        /// </summary>
        /// <remarks>If <see cref="Ordinal"/> is set, this will be ignored.</remarks>
        /// <value>The name.</value>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public ParameterAttribute()
        {
        }

        public ParameterAttribute(int ordinal)
        {
            _ordinal = ordinal;
        }

        public ParameterAttribute(string name)
        {
            _name = name;
        }
    }
}
