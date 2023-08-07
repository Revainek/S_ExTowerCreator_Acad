using System;

namespace S_ExTowerCreator_Acad.Models
{
    [Serializable]
    public class ObjectTriple<T, U, V>
    {
        public T First { get; set; }

        public U Second { get; set; }

        public V Third { get; set; }

        public ObjectTriple()
        {
        }

        public ObjectTriple(T first, U second, V third)
        {
            First = first;
            Second = second;
            Third = third;
        }
    }
}
