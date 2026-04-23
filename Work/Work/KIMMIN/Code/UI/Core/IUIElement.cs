namespace Code.UI.Core
{
    public interface IUIElement<T>
    {
        public void EnableFor(T element);
        void Clear();
    }
    
    public interface IUIElement<T1, T2>
    {
        public void EnableFor(T1 element, T2 element2);
        void Clear();
    }
    
    public interface IUIElement<T1, T2, T3>
    {
        public void EnableFor(T1 element, T2 element2, T3 element3);
        void Clear();
    }
}