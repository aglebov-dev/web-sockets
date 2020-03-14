using System;

namespace WSClient.UI
{
    public class Binding
    {
        public static Binding Create<T>(Func<T, string> accessor)
            => new Binding(x => accessor((T)x));

      
        private readonly Func<object, string> _accessor;
        public Binding(Func<object, string> accessor)
        {
            _accessor = accessor;
        }

        public string GetValue(object data)
        {
            return _accessor(data);
        }
    }

    //internal class TypeBinding<T> : Binding
    //{
    //    private readonly Func<T, string> _accessor;

    //    public TypeBinding(Func<T, string> accessor)
    //    {
    //        _accessor = accessor;
    //    }

    //    public string GetValue(T data)
    //    {
    //        return _accessor(data);
    //    }
    //}
}
