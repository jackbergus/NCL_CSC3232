using System;
namespace MinMaxLibrary.utils
{
    public class Optional<T>
    {
        public bool HasValue { get; private set; }
        private T value;
        public T Value
        {
            get
            {
                if (HasValue)
                    return value;
                else
                    throw new InvalidOperationException();
            }
        }

        public void set(T x)
        {
            if (x != null)
            {
                value = x;
                HasValue = true;
            }  else
            {
                HasValue = false;
            }
        }

        public Optional()
        {
            this.value = default(T);
            HasValue = false;
        }

        public Optional(T value)
        {
            this.value = value;
            HasValue = true;
        }


        public T optionalGet(T def)
        {
            return HasValue ? value : def;
        }

        public static explicit operator T(Optional<T> optional)
        {
            return optional.Value;
        }
        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Optional<T>)
                return this.Equals((Optional<T>)obj);
            else
                return false;
        }
        public bool Equals(Optional<T> other)
        {
            if (HasValue && other.HasValue)
                return object.Equals(value, other.value);
            else
                return HasValue == other.HasValue;
        }

        public override int GetHashCode()
        {
            if (HasValue)
                return value.GetHashCode() * 2;
            else
                return 0;
        }
    }
}
