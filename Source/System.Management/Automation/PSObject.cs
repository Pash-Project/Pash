// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    /// <summary>
    /// PSObject is a massive object that is very important to the functioning of Pash/PowerShell.
    /// </summary>
    [TypeDescriptionProvider(typeof(PSObjectTypeDescriptionProvider))]
    public class PSObject : IFormattable, IComparable
    {
        #region Constructors

        /****************************************************
        * CONTRUCTORS                                      *              
        ****************************************************/

        /// <summary>
        /// Builds a new PSObject.
        /// </summary>
        /// <param name="obj">The object to encapsulate in the PSObject.</param>
        public PSObject(object obj)
        {
            Initialize(obj);
        }

        /// <summary>
        /// Builds a PSCustomObject instance.
        /// </summary>
        public PSObject()
        {
            Initialize(PSCustomObject.Instance);
        }

        #endregion

        #region Public Properties
        /****************************************************
         * PUBLIC PROPERTIES                                *
         * These get called a lot.                          *               
         ****************************************************/
        /// <summary>
        ///  The actual object this PSObject is encapsulating
        /// </summary>
        public object ImmediateBaseObject { get; private set; }

        /// <summary>
        ///  A collection of all the members of the encapsulated object
        /// </summary>
        public PSMemberInfoCollection<PSMemberInfo> Members { get; private set; }

        /// <summary>
        ///  A collection of all the methods of the encapsulated object
        /// </summary>
        public PSMemberInfoCollection<PSMethodInfo> Methods { get; private set; }

        /// <summary>
        /// A collection of all the properties of the encapsulated object
        /// </summary>
        public PSMemberInfoCollection<PSPropertyInfo> Properties { get; private set; }

        /// <summary>
        /// A collection of the type names of the encapsulated object.
        /// </summary>
        public Collection<string> TypeNames { get; private set; }

        /// <summary>
        /// This property returns the non-PSObject object that is encapsulated
        /// </summary>
        public object BaseObject
        {
            get
            {
                object objParent = null;
                PSObject obj = this;
                do
                {
                    objParent = obj.ImmediateBaseObject;
                    obj = objParent as PSObject;
                }
                while (obj != null);
                return objParent;
            }
        }

        #endregion

        #region Protected Methods
        /****************************************************
         * PROTECTED METHODS                                *               
         ****************************************************/
        /// <summary>
        /// Reflect on the object being passed.
        /// </summary>
        /// <param name="obj">The object being passed.</param>
        protected void Initialize(object obj)
        {
            Members = new PSMemberInfoCollectionImplementation<PSMemberInfo>(this);
            Properties = new PSMemberInfoCollectionImplementation<PSPropertyInfo>(this);
            Methods = new PSMemberInfoCollectionImplementation<PSMethodInfo>(this);
            ImmediateBaseObject = obj;
        }

        #endregion

        #region Object Overloads
        /****************************************************
         * OPERATOR/OBJECT OVERLOADS                        *               
         ****************************************************/

        /// <summary>
        /// Checks if the encapsulated object (in PSObject) equals the comparison object. PSObjects are thus not compared to one another, but what they contain are.
        /// </summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>True is equal, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return ImmediateBaseObject.Equals(obj);
        }

        /// <summary>
        /// Gets the hash code of the encapsulated object (in PSObject).
        /// </summary>
        /// <returns>Returns the hash code.</returns>
        public override int GetHashCode()
        {
            return ImmediateBaseObject.GetHashCode();
        }

        /// <summary>
        /// Gets the ToString() value of the encapsulated object (in PSObject).
        /// </summary>
        /// <returns>mmediateBaseObject.ToString()</returns>
        public override string ToString()
        {
            return ImmediateBaseObject.ToString();
        }

        #endregion

        #region Static Logic
        /****************************************************
         * STATIC LOGIC                                     *               
         ****************************************************/

        public static PSObject AsPSObject(object obj)
        {
            PSObject _psobj = obj as PSObject;

            if (_psobj != null)
                return _psobj;

            return new PSObject(obj);
        }

        #endregion

        #region IFormattable Members
        /****************************************************
        * IFormattable Members                             *               
        ****************************************************/

        public string ToString(string format, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable Members
        /****************************************************
         * IComparable Members                              *               
         ****************************************************/

        public int CompareTo(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return 0;

            else
                return LanguagePrimitives.Compare(this.BaseObject, obj);
        }

        #endregion

    }
}
