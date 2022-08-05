#region Using Directives

using System;

#endregion

namespace StellarWolf.Core
{

    [Serializable]
    public struct Dice : IComparable<Dice>, IEquatable<Dice>
    {

        #region Fields

        private int m_Count;
        private int m_Sides;

        #endregion

        #region Properties

        /// <summary>
        /// The number of dice in the set.
        /// </summary>
        public int Count => m_Count;

        /// <summary>
        /// The number of sides a die in the set has.
        /// </summary>
        public int Sides => m_Sides;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of Dice.
        /// </summary>
        /// <param name="count">The number of dice in the set. </param>
        /// <param name="sides">The number of sides a die in the set has.</param>
        public Dice ( int count, int sides )
        {
            m_Count = Mathf.Max ( 1, count );
            m_Sides = Mathf.Max ( 1, sides );
        }

        /// <summary>
        /// Initializes a new instance of Dice.
        /// </summary>
        /// <param name="count">The number of dice in the set. </param>
        /// <param name="type">The number of sides a die in the set has.</param>
        public Dice ( int count, DieType type )
        {
            m_Count = Mathf.Max ( 1, count );
            m_Sides = (int) type;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public int CompareTo ( Dice other )
        {
            int c0 = ( m_Count * m_Sides ).CompareTo ( other.m_Count * other.m_Sides );
            return c0 != 0 ? c0 : m_Count.CompareTo ( other.m_Count );
        }

        /// <inheritdoc/>
        public override bool Equals ( object obj ) => !( obj is null ) && obj is Dice dice && Equals ( dice );

        /// <inheritdoc/>
        public bool Equals ( Dice other ) => m_Count == other.m_Count && m_Sides == other.m_Sides;

        /// <inheritdoc/>
        public override int GetHashCode ()
        {
            unchecked
            {
                int hash = 59; // might change
                hash = ( hash * 37 ) + m_Count.GetHashCode ();
                hash = ( hash * 83 ) + m_Sides.GetHashCode ();
                return hash;
            }
        }

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

            if ( count < 1 )
                throw new ArgumentOutOfRangeException ( "count", "The number of dice being rolled cannot be less than 1." );
            if ( sides < 1 )
                throw new ArgumentOutOfRangeException ( "sides", "The number of sides on a die being rolled cannot be less than 1." );
            return ChaosEngine.Shared.NextInteger ( count, ( count * sides ) + 1 ) + modifier;
        }

        /// <summary>
        /// Rolls the set and returns the individual rolls.
        /// </summary>
        /// <param name="count">The number of dice in the set.</param>
        /// <param name="sides">The number of sides a die in the set has.</param>
        /// <returns>An array containing each roll in the set.</returns>
        public static int [] Rolls ( int count, int sides )
        {
            if ( count < 1 )
                throw new ArgumentOutOfRangeException ( "count", "The number of dice being rolled cannot be less than 1." );
            if ( sides < 1 )
                throw new ArgumentOutOfRangeException ( "sides", "The number of sides on a die being rolled cannot be less than 1." );
            return ChaosEngine.Shared.NextIntegers ( 1, sides + 1, count );
        }

        /// <summary>
        /// Rolls the set and returns the total ± the modifier.
        /// </summary>
        /// <param name="count">The number of dice in the set.</param>
        /// <param name="type">The number of sides a die in the set has.</param>
        /// <param name="modifier">The amount to add or subract from the total.</param>
        /// <returns>The total ± the modifier.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static int Roll ( int count, DieType type, int modifier = 0 ) => Roll ( count, (int) type, modifier );

        /// <summary>
        /// Rolls the set and returns the individual rolls.
        /// </summary>
        /// <param name="count">The number of dice in the set.</param>
        /// <param name="sides">The number of sides a die in the set has.</param>
        /// <returns>An array containing each roll in the set.</returns>
        public static int [] Rolls ( int count, DieType type ) => Rolls ( count, (int) type );

        /// <summary>
        /// Rolls the set and returns the total ± the modifier.
        /// </summary>
        /// <param name="modifier">The amount to add or subract from the total.</param>
        /// <returns>The total ± the modifier.</returns>
        public int Roll ( int modifier = 0 ) => Roll ( Count, Sides, modifier );

        /// <summary>
        /// Rolls the set and returns the individual rolls.
        /// </summary>
        /// <returns>An array containing each roll in the set.</returns>
        public int [] Rolls () => Rolls ( Count, Sides );

        /// <inheritdoc/>
        public override string ToString () => $"{m_Count}d{m_Sides}";

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
