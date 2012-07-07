using System;

namespace System.Management.Automation
{
    public struct SwitchParameter
    {
        public SwitchParameter(bool isPresent)
        {
            _isPresent = isPresent;
        }

        public static bool operator !=(bool first, SwitchParameter second)
        {
            return ! (first == second);
        }

        public static bool operator !=(SwitchParameter first, bool second)
        {
            return  ! first.Equals(second);
        }

        public static bool operator !=(SwitchParameter first, SwitchParameter second)
        {
            return ! first.Equals(second);
        }

        public static bool operator ==(bool first, SwitchParameter second)
        {
            return first == second.ToBool();
        }

        public static bool operator ==(SwitchParameter first, bool second)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(SwitchParameter first, SwitchParameter second)
        {
            throw new NotImplementedException();
        }

        public static implicit operator SwitchParameter(bool value)
        {
            return new SwitchParameter(value);
        }

        public static implicit operator bool(SwitchParameter switchParameter)
        {
            return switchParameter.IsPresent;
        }

        private bool _isPresent;
        public bool IsPresent
        {
            get { return _isPresent; }
        }

        public static SwitchParameter Present
        {
            get
            {
                return new SwitchParameter(true);
            }
        }

        public override bool Equals(object obj)
        {
            return IsPresent == (bool) obj;
        }

        public override int GetHashCode()
        {
            return IsPresent.GetHashCode();
        }

        public bool ToBool()
        {
            return IsPresent;
        }

        public override string ToString()
        {
            return IsPresent.ToString();
        }
    }
}
