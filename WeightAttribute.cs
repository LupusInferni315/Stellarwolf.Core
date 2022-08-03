#region Using Directives

using System;
using System.Reflection;
using System.Linq;

#endregion

namespace StellarWolf.Core
{
    /// <summary>
    /// Defines the weight of an enum value.
    /// </summary>
    [AttributeUsage ( AttributeTargets.Field, AllowMultiple = false, Inherited = false )]
    public class WeightAttribute : Attribute
    {

        #region Fields

        private readonly int m_Weight;

        #endregion

        #region Properties

        /// <summary>
        /// The weight of the value.
        /// </summary>
        public int Weight => m_Weight;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new WeightAttribute.
        /// </summary>
        /// <param name="weight">The weight of the value.</param>
        public WeightAttribute ( int weight ) => m_Weight = weight;

        #endregion

    }

    /// <summary>
    /// Extensions relating to <seealso cref="WeightAttribute"/>
    /// </summary>
    public static class WeightAttributeExtensions
    {

        /// <summary>
        /// Gets the weight of an enum value, if <seealso cref="WeightAttribute"/> is not present it defaults to a weight of 1.
        /// </summary>
        /// <typeparam name="T">The type of the enum value.</typeparam>
        /// <param name="enumValue">The value to get the weight of.</param>
        /// <returns>The weight of the enum value.</returns>
        public static int GetWeight<T> ( this T enumValue ) where T : Enum
        {
        
            Type type = typeof ( T );
            MemberInfo info = type.GetMember ( enumValue.ToString () ).Where ( m => m.DeclaringType == type ).FirstOrDefault ();
            WeightAttribute wa = info.GetCustomAttribute<WeightAttribute> ( false );
            return wa != null ? wa.Weight : 1;
            
        }
        
    }
    
}
