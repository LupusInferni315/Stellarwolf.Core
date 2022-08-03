#region Using Directives

using System;

#endregion

namespace StellarWolf.Core
{

    /// <summary>
    /// A structure to store and roll dice.
    /// </summary>
    [Serializable]
    public struct Dice : IComparable<Dice>, IEquatable<Dice>
    {

        #region Fields
        
        private readonly int m_Count;
        private readonly DieType m_Type;

        #endregion

        #region Properties

        /// <summary>
        /// The number of dice in the set.
        /// </summary>
        public int Count => m_Count;

        /// <summary>
        /// The number of sides a die in the set has.
        /// </summary>
        public DieType Type => m_Type;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of Dice.
        /// </summary>
        /// <param name="count">The number of dice in the set. </param>
        /// <param name="type">The number of sides a die in the set has.</param>
        public Dice ( int count, DieType type )
        {
            m_Count = Math.Max ( 1, count );
            m_Type = type;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Compares this instance to a specified dice and returns an indication of their relative values.
        /// </summary>
        /// <param name="dice">Dice to compare.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="dice"/></returns>
        public int CompareTo ( Dice dice )
        {
            int c0 = ( (int) m_Type ).CompareTo ( (int) dice.m_Type );
            return c0 != 0 ? c0 : m_Count.CompareTo ( dice.m_Count );
        }

        /// <summary>
        /// Rolls the set and returns the total ± the modifier.
        /// </summary>
        /// <param name="count">The number of dice in the set.</param>
        /// <param name="type">The number of sides a die in the set has.</param>
        /// <param name="modifier">The amount to add or subract from the total.</param>
        /// <returns>The total ± the modifier.</returns>
        public static int Roll ( int count, DieType type, int modifier = 0 ) => Roll ( count, (int) type, modifier );

        /// <summary>
        /// Rolls the set and returns the total ± the modifier.
        /// </summary>
        /// <param name="count">The number of dice in the set.</param>
        /// <param name="sides">The number of sides a die in the set has.</param>
        /// <param name="modifier">The amount to add or subract from the total.</param>
        /// <returns>The total ± the modifier.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static int Roll ( int count, int sides, int modifier = 0 )
        {
            if( count < 1 )
                throw new ArgumentOutOfRangeException ( "count", "There cannot be less than 1 die to roll." );
            if ( sides < 1 )
                throw new ArgumentOutOfRangeException ( "sides", "There cannot be less than 1 side on the dice." );
            return ChaosEngine.Shared.NextInteger ( count, ( count * sides ) + 1 ) + modifier;
        }

        /// <summary>
        /// Rolls the set and returns the total ± the modifier.
        /// </summary>
        /// <param name="modifier">The amount to add or subract from the total.</param>
        /// <returns>The total ± the modifier.</returns>
        public int Roll ( int modifier = 0 ) => Roll ( m_Count, m_Type, modifier );

        /// <summary>
        /// Rolls the set and returns the individual rolls.
        /// </summary>
        /// <param name="count">The number of dice in the set.</param>
        /// <param name="type">The number of sides a die in the set has.</param>
        /// <returns>An array containing each roll in the set.</returns>
        public static int [] Rolls ( int count, DieType type ) => Rolls ( count, (int) type );

        /// <summary>
        /// Rolls the set and returns the individual rolls.
        /// </summary>
        /// <param name="count">The number of dice in the set.</param>
        /// <param name="sides">The number of sides a die in the set has.</param>
        /// <returns>An array containing each roll in the set.</returns>
        public static int [] Rolls(int count, int sides)
        {
            if( count < 1 )
                throw new ArgumentOutOfRangeException ( "count", "There cannot be less than 1 die to roll." );
            if ( sides < 1 )
                throw new ArgumentOutOfRangeException ( "sides", "There cannot be less than 1 side on the dice." );
            return ChaosEngine.Shared.NextIntegers ( 1, sides + 1, count );
        }

        /// <summary>
        /// Rolls the set and returns the individual rolls.
        /// </summary>
        /// <returns>An array containing each roll in the set.</returns>
        public int [] Rolls () => Rolls ( m_Count, m_Type );

        /// <inheritdoc/>
        public override bool Equals ( object obj ) => !( obj is null ) && obj is Dice dice && Equals ( dice );

        /// <inheritdoc/>
        public bool Equals ( Dice dice ) => m_Count == dice.m_Count && m_Type == dice.m_Type;

        /// <inheritdoc/>
        public override int GetHashCode ()
        {

            unchecked
            {
                int hash = 17;
                hash = ( hash * 31 ) + m_Count.GetHashCode ();
                hash = ( hash * 31 ) + m_Type.GetHashCode ();
                return hash;
            }

        }

        /// <inheritdoc/>
        public override string ToString () => $"{m_Count}{m_Type}";

        #endregion

        #region Operators

        /// <summary>
        /// Checks for equality between 2 dice objects.
        /// </summary>
        /// <param name="lhs">A dice to compare.</param>
        /// <param name="rhs">A dice to compare.</param>
        /// <returns>A <seealso cref="Boolean"/> value determining equality.</returns>
        public static bool operator == ( Dice lhs, Dice rhs ) => lhs.Equals ( rhs );

        /// <summary>
        /// Checks for inequality between 2 dice objects.
        /// </summary>
        /// <param name="lhs">A dice to compare.</param>
        /// <param name="rhs">A dice to compare.</param>
        /// <returns>A <seealso cref="Boolean"/> value determining inequality.</returns>
        public static bool operator != ( Dice lhs, Dice rhs ) => !lhs.Equals ( rhs );

        /// <summary>
        /// Compares <paramref name="lhs"/> against <paramref name="rhs"/> to see if it is smaller than it.
        /// </summary>
        /// <param name="lhs">A dice to compare.</param>
        /// <param name="rhs">A dice to compare.</param>
        /// <returns>A <seealso cref="Boolean"/> value determining whether <paramref name="lhs"/> is less than <paramref name="rhs"/>.</returns>
        public static bool operator < ( Dice lhs, Dice rhs ) => lhs.CompareTo ( rhs ) < 0;

        /// <summary>
        /// Compares <paramref name="lhs"/> against <paramref name="rhs"/> to see if it is larger than it.
        /// </summary>
        /// <param name="lhs">A dice to compare.</param>
        /// <param name="rhs">A dice to compare.</param>
        /// <returns>A <seealso cref="Boolean"/> value determining whether <paramref name="lhs"/> is greater than <paramref name="rhs"/>.</returns>
        public static bool operator > ( Dice lhs, Dice rhs ) => lhs.CompareTo ( rhs ) > 0;

        /// <summary>
        /// Compares <paramref name="lhs"/> against <paramref name="rhs"/> to see if it is smaller than or equal to it.
        /// </summary>
        /// <param name="lhs">A dice to compare.</param>
        /// <param name="rhs">A dice to compare.</param>
        /// <returns>A <seealso cref="Boolean"/> value determining whether <paramref name="lhs"/> is less than or equal to <paramref name="rhs"/>.</returns>
        public static bool operator <= ( Dice lhs, Dice rhs ) => lhs.CompareTo ( rhs ) <= 0;

        /// <summary>
        /// Compares <paramref name="lhs"/> against <paramref name="rhs"/> to see if it is larger than or equal to it.
        /// </summary>
        /// <param name="lhs">A dice to compare.</param>
        /// <param name="rhs">A dice to compare.</param>
        /// <returns>A <seealso cref="Boolean"/> value determining whether <paramref name="lhs"/> is greater than or equal to <paramref name="rhs"/>.</returns>
        public static bool operator >= ( Dice lhs, Dice rhs ) => lhs.CompareTo ( rhs ) >= 0;

        #endregion

    }

}
