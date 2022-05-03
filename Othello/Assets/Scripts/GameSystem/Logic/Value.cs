namespace GameSystem.Logic
{
    // セルの古い状態と新しい状態を同時に管理するためのクラス
    public class Value<T>
    {
        public T newValue;
        public T oldValue;

        public Value(T oldValue, T newValue)
        {
            this.oldValue = oldValue;
            this.newValue = newValue;
        }
    }
}