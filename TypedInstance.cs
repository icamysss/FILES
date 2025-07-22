using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Common.Utils.Wrappers
{
    /// <summary>
    /// Универсальная типобезопасная обертка для объектов любого типа.
    /// Обеспечивает неявное преобразование к оборачиваемому типу, корректное сравнение
    /// и удобную работу в DI-контейнерах.
    /// </summary>
    /// <typeparam name="T">Тип оборачиваемого объекта</typeparam>
     [Preserve]
    public class TypedInstance<T>
    {
        /// <summary>
        /// Оборачиваемое значение
        /// </summary>
        public T Value { get; }
    
        /// <summary>
        /// Создает новую обертку для указанного значения
        /// </summary>
        /// <param name="value">Значение для обертывания</param>
        public TypedInstance(T value) => Value = value;
    
        /// <summary>
        /// Неявное преобразование обертки в обернутое значение
        /// </summary>
        public static implicit operator T(TypedInstance<T> wrapper) => wrapper != null ? wrapper.Value : default;
    
        /// <summary>
        /// Строковое представление для отладки
        /// </summary>
        public override string ToString() => Value != null ? Value.ToString() : "Null Wrapper";
    
        /// <summary>
        /// Сравнение по значению обернутых объектов
        /// </summary>
        protected bool Equals(TypedInstance<T> other) => EqualityComparer<T>.Default.Equals(Value, other.Value);
    
        /// <summary>
        /// Сравнение с другим объектом
        /// </summary>
        public override bool Equals(object obj) => obj is TypedInstance<T> other && Equals(other);
    
        /// <summary>
        /// Хэш-код на основе обернутого значения
        /// </summary>
        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;
    }
}