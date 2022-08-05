#region Using Directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#endregion

namespace StellarWolf.Core
{

    /// <summary>
    /// A pseudorandom number generator.
    /// </summary>
    public sealed class ChaosEngine
    {

        #region Fields

        private int m_Seed;
        private int m_INext;
        private int m_INextP;
        private readonly int [] m_SeedArray = new int [ 56 ];
        private object m_StateLock = new object ();

        [ThreadStatic] private static ChaosEngine m_Shared;

        #endregion

        #region Properties

        /// <summary>
        /// A thread safe instance of the <seealso cref="ChaosEngine"/>
        /// </summary>
        public static ChaosEngine Shared
        {
            get
            {
                if ( m_Shared is null )
                    m_Shared = new ChaosEngine ();
                return m_Shared;
            }

            set
            {
                if ( value is null )
                    value = new ChaosEngine ();
                m_Shared = value;
            }
        }

        /// <summary>
        /// The seed used for the current state of the <seealso cref="ChaosEngine"/>.
        /// </summary>
        public int? Seed { get => m_Seed; set => Reseed ( value ); }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <seealso cref="ChaosEngine"/> with an undetermined seed.
        /// </summary>
        public ChaosEngine () : this ( Guid.NewGuid ().GetHashCode () ) { }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="ChaosEngine"/> with the specified seed.
        /// </summary>
        /// <param name="seed">The initial state of the <seealso cref="ChaosEngine"/>.</param>
        public ChaosEngine ( string seed ) : this ( ParseSeed ( seed ) ) { }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="ChaosEngine"/> with the specified state.
        /// </summary>
        /// <param name="saveState">The state of the <seealso cref="ChaosEngine"/>.</param>
        public ChaosEngine ( int [] saveState ) => LoadState ( saveState );

        /// <summary>
        /// Initializes a new instance of the <seealso cref="ChaosEngine"/> with the specified seed.
        /// </summary>
        /// <param name="seed">The initial state of the <seealso cref="ChaosEngine"/>.</param>
        public ChaosEngine ( int seed ) => Reseed ( seed );

        #endregion

        #region Seed Methods

        private static int ParseSeed ( string seed ) => string.IsNullOrEmpty ( seed ) ? Guid.NewGuid ().GetHashCode () : int.TryParse ( seed, out int i ) ? i : seed.GetHashCode ();

        /// <summary>
        /// Reset the state of the <seealso cref="ChaosEngine"/> with the existing seed.
        /// </summary>
        public void Reset () => Reseed ( m_Seed );

        /// <summary>
        /// Reseeds the <seealso cref="ChaosEngine"/> with a new undetermined seed.
        /// </summary>
        public void Reseed () => Reseed ( Guid.NewGuid ().GetHashCode () );

        /// <summary>
        /// Reseeds the <seealso cref="ChaosEngine"/> with a specified seed.
        /// </summary>
        /// <param name="seed">The initial state of the <seealso cref="ChaosEngine"/>.</param>
        public void Reseed ( string seed ) => Reseed ( ParseSeed ( seed ) );

        /// <summary>
        /// Reseeds the <seealso cref="ChaosEngine"/> with a specified seed.
        /// </summary>
        /// <param name="seed">The initial state of the <seealso cref="ChaosEngine"/>.</param>
        public void Reseed ( int? seed )
        {

            lock ( m_StateLock )
            {
                m_Seed = seed ?? Guid.NewGuid ().GetHashCode ();

                int subtraction = ( m_Seed == int.MinValue ) ? int.MaxValue : Math.Abs ( m_Seed );
                int mj = 161803398 - subtraction;
                m_SeedArray [ 55 ] = mj;
                int mk = 1;

                for ( int i = 1; i < 55; i++ )
                {
                    int ii = 21 * i % 55;
                    m_SeedArray [ ii ] = mk;
                    mk = mj - mk;
                    if ( mk < 0 )
                        mk += int.MaxValue;
                    mj = m_SeedArray [ ii ];
                }

                for ( int k = 1; k < 5; k++ )
                {
                    for ( int i = 1; i < 56; i++ )
                    {
                        m_SeedArray [ i ] -= m_SeedArray [ 1 + ( ( i + 30 ) % 55 ) ];
                        if ( m_SeedArray [ i ] < 0 )
                            m_SeedArray [ i ] += int.MaxValue;
                    }
                }
                m_INext = 0;
                m_INextP = 21;
            }

        }

        #endregion

        #region Integer

        /// <summary>
        /// Returns a random 32-bit integer in the range 0..<seealso cref="Int32.MaxValue"/>.
        /// </summary>
        /// <returns>A random 32-bit integer such that 0 ≤ value &lt; <seealso cref="Int32.MaxValue"/>.</returns>
        public int NextInteger () => NextSample ();

        /// <summary>
        /// Returns a random 32-bit integer in the range <paramref name="minValue"/>..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The inclusive minimum value.</param>
        /// <param name="maxValue">The exclusive maximum value.</param>
        /// <returns>A random 32-bit integer such that <paramref name="minValue"/> ≤ value &lt; <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int NextInteger ( int minValue, int maxValue )
        {
            if ( minValue > int.MaxValue )
                throw new ArgumentOutOfRangeException ( "minValue", "The minimum value cannot be greater than the maximum value." );
            return (int) ( NextSample () * ( 1.0 / int.MaxValue ) * ( maxValue - minValue ) ) + minValue;
        }

        /// <summary>
        /// Returns a random 32-bit integer in the range 0..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The exclusive maximum value.</param>
        /// <returns>A random 32-bit integer such that 0 ≤ value &lt; <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int NextInteger ( int maxValue )
        {
            if ( maxValue < 1 )
                throw new ArgumentOutOfRangeException ( "maxValue", "The maximum value cannot be less than 1." );
            return NextInteger ( 0, maxValue );
        }

        #endregion

        #region Integers

        /// <summary>
        /// Fills an array of 32-bit integers with random values in the range 0..<seealso cref="Int32.MaxValue"/>.
        /// </summary>
        /// <param name="buffer">The array of 32-bit integers to be filled.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextIntegers ( int [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextInteger ();
        }

        /// <summary>
        /// Fills an array of 32-bit integers with random values in the range <paramref name="minValue"/>..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The inclusive minimum value.</param>
        /// <param name="maxValue">The exclusive maximum value.</param>
        /// <param name="buffer">The array of 32-bit integers to be filled.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextIntegers ( int minValue, int maxValue, int [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextInteger ( minValue, maxValue );
        }

        /// <summary>
        /// Fills an array of 32-bit integers with random values in the range 0..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The exclusive maximum value.</param>
        /// <param name="buffer">The array of 32-bit integers to be filled.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextIntegers ( int maxValue, int [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextInteger ( maxValue );
        }

        /// <summary>
        /// Returns an array of 32-bit integers with random values in the range 0..<seealso cref="Int32.MaxValue"/>.
        /// </summary>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of 32-bit integers filled with random values such that 0 ≤ value &lt; <seealso cref="Int32.MaxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int [] NextIntegers ( int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            int [] buffer = new int [ amount ];
            NextIntegers ( buffer );
            return buffer;
        }

        /// <summary>
        /// Returns an array of 32-bit integers with random values in the range <paramref name="minValue"/>..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The inclusive minimum value.</param>
        /// <param name="maxValue">The exclusive maximum value.</param>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of 32-bit integers filled with random values such that <paramref name="minValue"/> ≤ value &gt; <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int [] NextIntegers ( int minValue, int maxValue, int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            int [] buffer = new int [ amount ];
            NextIntegers ( minValue, maxValue, buffer );
            return buffer;
        }

        /// <summary>
        /// Returns an array of 32-bit integers with random values in the range 0..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The exclusive maximum value.</param>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of 32-bit integers filled with random values such that 0 ≤ value &lt; <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int [] NextIntegers ( int maxValue, int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            int [] buffer = new int [ amount ];
            NextIntegers ( maxValue, buffer );
            return buffer;
        }

        #endregion

        #region Double

        /// <summary>
        /// Returns a random double-precision floating-point number in the range 0..1.
        /// </summary>
        /// <returns>A random double-precision floating-point number such that 0 ≤ value ≤ 1.</returns>
        public double NextDouble () => NextSample () * ( 1.0 / ( int.MaxValue - 1 ) );

        /// <summary>
        /// Returns a random double-precision floating-point number in the range <paramref name="minValue"/>..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The inclusive minimum value.</param>
        /// <param name="maxValue">The inclusive maximum value.</param>
        /// <returns>A random double-precision floating-point number such that <paramref name="minValue"/> ≤ value ≤ <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public double NextDouble ( double minValue, double maxValue )
        {
            if ( minValue > maxValue )
                throw new ArgumentOutOfRangeException ( "minValue", "The minimum value cannot be greater than the maximum value." );
            return ( NextDouble () * ( maxValue - minValue ) ) + minValue;
        }

        /// <summary>
        /// Returns a random double-precision floating-point number in the range 0..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The inclusive maximum value.</param>
        /// <returns>A random double-precision floating-point number such that 0 ≤ value ≤ <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public double NextDouble ( double maxValue )
        {
            if ( maxValue < 0 )
                throw new ArgumentOutOfRangeException ( "maxValue", "The maximum value cannot be less than 0." );
            return NextDouble ( 0, maxValue );
        }

        #endregion

        #region Doubles

        /// <summary>
        /// Fills an array of double-precision floating-point numbers with random values in the range 0..1.
        /// </summary>
        /// <param name="buffer">The array of double-precision floating-point numbers to be filled.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextDoubles ( double [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextDouble ();
        }

        /// <summary>
        /// Fills an array of double-precision floating-point numbers with random values in the range <paramref name="minValue"/>..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The inclusive minimum value.</param>
        /// <param name="maxValue">The inclusive maximum value.</param>
        /// <param name="buffer">The array of double-precision floating-point numbers to be filled.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextDoubles ( double minValue, double maxValue, double [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextDouble ( minValue, maxValue );
        }

        /// <summary>
        /// Fills an array of double-precision floating-point numbers with random values in the range 0..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The inclusive maximum value.</param>
        /// <param name="buffer">The array of double-precision floating-point numbers to be filled.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextDoubles ( double maxValue, double [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextDouble ( maxValue );
        }

        /// <summary>
        /// Returns an array of double-precision floating-point numbers with random values in the range 0..1.
        /// </summary>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of double-precision floating-point numbers filled with random values such that 0 ≤ value ≤ 1.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public double [] NextDoubles ( int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            double [] buffer = new double [ amount ];
            NextDoubles ( buffer );
            return buffer;
        }

        /// <summary>
        /// Returns an array of double-precision floating-point numbers with random values in the range <paramref name="minValue"/>..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The inclusive minimum value.</param>
        /// <param name="maxValue">The inclusive maximum value.</param>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of double-precision floating-point numbers filled with random values such that <paramref name="minValue"/> ≤ value ≤ <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public double [] NextDoubles ( double minValue, double maxValue, int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            double [] buffer = new double [ amount ];
            NextDoubles ( minValue, maxValue, buffer );
            return buffer;
        }

        /// <summary>
        /// Returns an array of double-precision floating-point numbers with random values in the range 0..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The inclusive maximum value.</param>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of double-precision floating-point numbers filled with random values such that 0 ≤ value ≤ <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public double [] NextDoubles ( double maxValue, int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            double [] buffer = new double [ amount ];
            NextDoubles ( maxValue, buffer );
            return buffer;
        }

        #endregion

        #region Float

        /// <summary>
        /// Returns a random single-precision floating-point number in the range 0..1.
        /// </summary>
        /// <returns>A random single-precision floating-point number such that 0 ≤ value ≤ 1.</returns>
        public float NextFloat () => NextSample () * ( 1.0f / ( int.MaxValue - 1 ) );

        /// <summary>
        /// Returns a random single-precision floating-point number in the range <paramref name="minValue"/>..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The inclusive minimum value.</param>
        /// <param name="maxValue">The inclusive maximum value.</param>
        /// <returns>A random single-precision floating-point number such that <paramref name="minValue"/> ≤ value ≤ <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public float NextFloat ( float minValue, float maxValue )
        {
            if ( minValue > maxValue )
                throw new ArgumentOutOfRangeException ( "minValue", "The minimum value cannot be greater than the maximum value." );
            return ( NextFloat () * ( maxValue - minValue ) ) + minValue;
        }

        /// <summary>
        /// Returns a random single-precision floating-point number in the range 0..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The inclusive maximum value.</param>
        /// <returns>A random single-precision floating-point number such that 0 ≤ value ≤ <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public float NextFloat ( float maxValue )
        {
            if ( maxValue < 0 )
                throw new ArgumentOutOfRangeException ( "maxValue", "The maximum value cannot be less than 0." );
            return NextFloat ( 0, maxValue );
        }

        #endregion

        #region Floats

        /// <summary>
        /// Fills an array of single-precision floating-point numbers with random values in the range 0..1.
        /// </summary>
        /// <param name="buffer">The array of single-precision floating-point numbers to be filled.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextFloats ( float [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextFloat ();
        }

        /// <summary>
        /// Fills an array of single-precision floating-point numbers with random values in the range <paramref name="minValue"/>..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The inclusive minimum value.</param>
        /// <param name="maxValue">The inclusive maximum value.</param>
        /// <param name="buffer">The array of single-precision floating-point numbers to be filled.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextFloats ( float minValue, float maxValue, float [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextFloat ( minValue, maxValue );
        }

        /// <summary>
        /// Fills an array of single-precision floating-point numbers with random values in the range 0..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The inclusive maximum value.</param>
        /// <param name="buffer">The array of single-precision floating-point numbers to be filled.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void NextFloats ( float maxValue, float [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextFloat ( maxValue );
        }

        /// <summary>
        /// Returns an array of single-precision floating-point numbers with random values in the range 0..1.
        /// </summary>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of single-precision floating-point numbers filled with random values such that 0 ≤ value ≤ 1.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public float [] NextFloats ( int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            float [] buffer = new float [ amount ];
            NextFloats ( buffer );
            return buffer;
        }

        /// <summary>
        /// Returns an array of single-precision floating-point numbers with random values in the range <paramref name="minValue"/>..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The inclusive minimum value.</param>
        /// <param name="maxValue">The inclusive maximum value.</param>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of single-precision floating-point numbers filled with random values such that <paramref name="minValue"/> ≤ value ≤ <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public float [] NextFloats ( float minValue, float maxValue, int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            float [] buffer = new float [ amount ];
            NextFloats ( minValue, maxValue, buffer );
            return buffer;
        }

        /// <summary>
        /// Returns an array of single-precision floating-point numbers with random values in the range 0..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The inclusive maximum value.</param>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of single-precision floating-point numbers filled with random values such that 0 ≤ value ≤ <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public float [] NextFloats ( float maxValue, int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            float [] buffer = new float [ amount ];
            NextFloats ( maxValue, buffer );
            return buffer;
        }

        #endregion

        #region Byte

        /// <summary>
        /// Returns a random 8-bit integer in the range 0..<seealso cref="Byte.MaxValue"/>.
        /// </summary>
        /// <returns>A random 8-bit integer such that 0 ≤ value &lt; <seealso cref="Byte.MaxValue"/>.</returns>
        public byte NextByte () => (byte) NextInteger ( 0, 256 );

        /// <summary>
        /// Returns a random 8-bit integer in the range <paramref name="minValue"/>..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The inclusive minimum value.</param>
        /// <param name="maxValue">The exclusive maximum value.</param>
        /// <returns>A random 8-bit integer such that <paramref name="minValue"/> ≤ value &lt; <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte NextByte ( byte minValue, byte maxValue )
        {
            if ( minValue > maxValue )
                throw new ArgumentOutOfRangeException ( "minValue", "the minumum value cannot be greater than the maximum value." );
            return (byte) NextInteger ( minValue, maxValue );
        }

        /// <summary>
        /// Returns a random 8-bit integer in the range 0..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The exclusive maximum value.</param>
        /// <returns>A random 8-bit integer such that 0 ≤ value &lt; <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte NextByte ( byte maxValue )
        {
            if ( maxValue < 1 )
                throw new ArgumentOutOfRangeException ( "maxValue", "The maximum value cannot be less than 1." );
            return (byte) NextInteger ( maxValue );
        }

        #endregion

        #region Bytes

        /// <summary>
        /// Fills an array of 8-bit integers with random values in the range 0..<seealso cref="Byte.MaxValue"/>.
        /// </summary>
        /// <param name="buffer">The array of 8-bit integers to be filled.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextBytes ( byte [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextByte ();
        }

        /// <summary>
        /// Fills an array of 8-bit integers with random values in the range <paramref name="minValue"/>..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The inclusive minimum value.</param>
        /// <param name="maxValue">The exclusive maximum value.</param>
        /// <param name="buffer">The array of 8-bit integers to be filled.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextBytes ( byte minValue, byte maxValue, byte [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextByte ( minValue, maxValue );
        }

        /// <summary>
        /// Fills an array of 8-bit integers with random values in the range 0..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The exclusive maximum value.</param>
        /// <param name="buffer">The array of 8-bit integers to be filled.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextBytes ( byte maxValue, byte [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextByte ( maxValue );
        }

        /// <summary>
        /// Returns an array of 8-bit integers with random values in the range 0..<seealso cref="Byte.MaxValue"/>.
        /// </summary>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of 8-bit integers filled with random values such that 0 ≤ value &lt; <seealso cref="Byte.MaxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte [] NextBytes ( int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            byte [] buffer = new byte [ amount ];
            NextBytes ( buffer );
            return buffer;
        }

        /// <summary>
        /// Returns an array of 8-bit integers with random values in the range <paramref name="minValue"/>..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The inclusive minimum value.</param>
        /// <param name="maxValue">The exclusive maximum value.</param>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of 8-bit integers filled with random values such that <paramref name="minValue"/> ≤ value &lt; <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte [] NextBytes ( byte minValue, byte maxValue, int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            byte [] buffer = new byte [ amount ];
            NextBytes ( minValue, maxValue, buffer );
            return buffer;
        }

        /// <summary>
        /// Returns an array of 8-bit integers with random values in the range 0..<paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The exclusive maximum value.</param>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of 8-bit integers filled with random values such that 0 ≤ value &lt; <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte [] NextBytes ( byte maxValue, int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            byte [] buffer = new byte [ amount ];
            NextBytes ( maxValue, buffer );
            return buffer;
        }

        #endregion

        #region Bool

        /// <summary>
        /// Returns a random boolean value at equal odds.
        /// </summary>
        public bool NextBool () => NextInteger ( 0, 2 ) == 1;

        #endregion

        #region Bools

        /// <summary>
        /// Fills an array of boolean values at equal odds.
        /// </summary>
        /// <param name="buffer">The array of booleans to be filled.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextBools ( bool [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextBool ();
        }

        /// <summary>
        /// Returns an array of boolean values at equal odds.
        /// </summary>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of booleans filled with random values at equal odds.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool [] NextBools ( int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            bool [] buffer = new bool [ amount ];
            NextBools ( buffer );
            return buffer;
        }

        #endregion

        #region Arrays & Lists

        /// <summary>
        /// Shuffles the values in a list.
        /// </summary>
        /// <typeparam name="T">The type of data contained within the list.</typeparam>
        /// <param name="list">The list being shuffled.</param>
        public void Shuffle<T> ( List<T> list )
        {
            int n = list.Count;

            while ( n > 1 )
            {
                n--;
                int k = NextInteger ( 0, n + 1 );
                (list [ n ], list [ k ]) = (list [ k ], list [ n ]);
            }
        }

        /// <summary>
        /// Shuffles the values in a array.
        /// </summary>
        /// <typeparam name="T">The type of data contained within the array.</typeparam>
        /// <param name="array">The array being shuffled.</param>
        public void Shuffle<T> ( T [] array )
        {
            int n = array.Length;

            while ( n > 1 )
            {
                n--;
                int k = NextInteger ( 0, n + 1 );
                (array [ n ], array [ k ]) = (array [ k ], array [ n ]);
            }
        }

        /// <summary>
        /// Selects a random element out of a list.
        /// </summary>
        /// <typeparam name="T">The type of data contained within the list.</typeparam>
        /// <param name="list">The list to select a random element from.</param>
        /// <returns>A random element selected from <paramref name="list"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public T Choose<T> ( List<T> list )
        {
            if ( list is null || list.Count < 1 )
                throw new ArgumentNullException ( "list", "The list cannot be null or contain less than 1 element." );

            if ( typeof ( IWeighted ).IsAssignableFrom ( typeof ( T ) ) )
            {
                List<T> newList = new List<T> ();

                foreach ( T t in list )
                    for ( int i = 0; i < ( t as IWeighted ).Weight; i++ )
                        newList.Add ( t );
                return newList [ NextInteger ( newList.Count ) ];
            }
            else
            {
                return list [ NextInteger ( list.Count ) ];
            }
        }

        /// <summary>
        /// Fills an array with random elements selected from a list.
        /// </summary>
        /// <typeparam name="T">The type of data contained within the list.</typeparam>
        /// <param name="list">The list to select a random element from.</param>
        /// <param name="buffer">The array to fill with random elements.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Choose<T> ( List<T> list, T [] buffer )
        {
            if ( list is null || list.Count < 1 )
                throw new ArgumentNullException ( "list", "The list cannot be null or contain less than 1 element." );
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = Choose ( list );
        }

        /// <summary>
        /// Returns an array filled with random elements selected from a list.
        /// </summary>
        /// <typeparam name="T">The type of data contained within the list.</typeparam>
        /// <param name="list">The list to select a random element from.</param>
        /// <param name="amount">The size of the array to create.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public T [] Choose<T> ( List<T> list, int amount )
        {
            if ( list is null || list.Count < 1 )
                throw new ArgumentNullException ( "list", "The list cannot be null or contain less than 1 element." );
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            T [] buffer = new T [ amount ];
            Choose ( list, buffer );
            return buffer;
        }

        /// <summary>
        /// Selects a random element out of an array.
        /// </summary>
        /// <typeparam name="T">The type of data contained within the array.</typeparam>
        /// <param name="array">The array to select a random element from.</param>
        /// <returns>A random element selected from <paramref name="array"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public T Choose<T> ( T [] array )
        {
            if ( array is null || array.Length < 1 )
                throw new ArgumentNullException ( "array", "The array cannot be null or contain less than 1 element." );
            return array [ NextInteger ( array.Length ) ];
        }

        /// <summary>
        /// Fills an array with random elements selected from an array.
        /// </summary>
        /// <typeparam name="T">The type of data contained within the array.</typeparam>
        /// <param name="array">The array to select a random element from.</param>
        /// <param name="buffer">The array to fill with random elements.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Choose<T> ( T [] array, T [] buffer )
        {
            if ( array is null || array.Length < 1 )
                throw new ArgumentNullException ( "array", "The array cannot be null or contain less than 1 element." );
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = Choose ( array );
        }

        /// <summary>
        /// Returns an array filled with random elements selected from an array.
        /// </summary>
        /// <typeparam name="T">The type of data contained within the array.</typeparam>
        /// <param name="array">The array to select a random element from.</param>
        /// <param name="amount">The size of the array to create.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public T [] Choose<T> ( T [] array, int amount )
        {
            if ( array is null || array.Length < 1 )
                throw new ArgumentNullException ( "list", "The list cannot be null or contain less than 1 element." );
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            T [] buffer = new T [ amount ];
            Choose ( array, buffer );
            return buffer;
        }

        #endregion

        #region Probability

        /// <summary>
        /// Returns a boolean value with a <paramref name="percent"/>% chance of being <see langword="true"/>.
        /// </summary>
        /// <param name="percent">The probability that the value will be <see langword="true"/>.</param>
        public bool NextProbability ( float percent )
        {
            if ( percent >= 1 )
                return true;
            else if ( percent <= 0 )
                return false;
            return NextFloat () < percent;
        }

        /// <summary>
        /// Returns a boolean value with a <paramref name="percent"/>% chance of being <see langword="true"/>.
        /// </summary>
        /// <param name="percent">The probability that the value will be <see langword="true"/>.</param>
        public bool NextProbability ( int percent )
        {
            if ( percent >= 100 )
                return true;
            else if ( percent <= 0 )
                return false;
            return NextInteger ( 100 ) < percent;
        }

        #endregion

        #region Probabilities

        /// <summary>
        /// Fills an array of boolean values with a <paramref name="percent"/>% chance of being <see langword="true"/>.
        /// </summary>
        /// <param name="percent">The probability that the value will be <see langword="true"/>.</param>
        /// <param name="buffer">The array of booleans to be filled.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextProbabilities ( float percent, bool [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextProbability ( percent );
        }

        /// <summary>
        /// Returns an array of boolean values with a <paramref name="percent"/>% chance of being <see langword="true"/>.
        /// </summary>
        /// <param name="percent">The probability that the value will be <see langword="true"/>.</param>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of booleans filled with random values at equal odds.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool [] NextProbabilities ( float percent, int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            bool [] buffer = new bool [ amount ];
            NextProbabilities ( percent, buffer );
            return buffer;
        }

        /// <summary>
        /// Fills an array of boolean values with a <paramref name="percent"/>% chance of being <see langword="true"/>.
        /// </summary>
        /// <param name="percent">The probability that the value will be <see langword="true"/>.</param>
        /// <param name="buffer">The array of booleans to be filled.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void NextProbabilities ( int percent, bool [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            for ( int i = 0; i < buffer.Length; i++ )
                buffer [ i ] = NextProbability ( percent );
        }

        /// <summary>
        /// Returns an array of boolean values with a <paramref name="percent"/>% chance of being <see langword="true"/>.
        /// </summary>
        /// <param name="percent">The probability that the value will be <see langword="true"/>.</param>
        /// <param name="amount">The size of the array to create.</param>
        /// <returns>An array of booleans filled with random values at equal odds.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool [] NextProbabilities ( int percent, int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            bool [] buffer = new bool [ amount ];
            NextProbabilities ( percent, buffer );
            return buffer;
        }

        #endregion

        #region Odds

        /// <summary>
        /// Returns a boolean value with a <paramref name="a"/> in <paramref name="b"/> chance of being <see langword="true"/>.
        /// </summary>
        /// <param name="a">The number of time <see langword="true"/> can be expected in <paramref name="b"/> attempts.</param>
        /// <param name="b">The number of attempts in which you can expect <paramref name="a"/> results of <see langword="true"/></param>
        /// <exception cref="DivideByZeroException"></exception>
        public bool NextOdds ( int a, int b )
        {
            if ( b == 0 )
                throw new DivideByZeroException ();
            return NextProbability ( (float) a / b );
        }

        /// <summary>
        /// Fills an array of boolean values with a <paramref name="a"/> in <paramref name="b"/> chance of being <see langword="true"/>.
        /// </summary>
        /// <param name="a">The number of times <see langword="true"/> can be expected in <paramref name="b"/> attempts.</param>
        /// <param name="b">The number of attempts in which you can expect <paramref name="a"/> results of <see langword="true"/></param>
        /// <param name="buffer">The array of booleans to be filled.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DivideByZeroException"></exception>
        public void NextOdds ( int a, int b, bool [] buffer )
        {
            if ( buffer is null || buffer.Length < 1 )
                throw new ArgumentNullException ( "buffer", "The buffer cannot be null or contain less than 1 element." );
            if ( b == 0 )
                throw new DivideByZeroException ();
            NextProbabilities ( (float) a / b, buffer );
        }

        /// <summary>
        /// Returns an array of boolean values filled with a <paramref name="a"/> in <paramref name="b"/> chance of being <see langword="true"/>.
        /// </summary>
        /// <param name="a">The number of times <see langword="true"/> can be expected in <paramref name="b"/> attempts.</param>
        /// <param name="b">The number of attempts in which you can expect <paramref name="a"/> results of <see langword="true"/></param>
        /// <param name="amount">The size of the array to create.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="DivideByZeroException"></exception>
        public bool [] NextOdds ( int a, int b, int amount )
        {
            if ( amount < 1 )
                throw new ArgumentOutOfRangeException ( "amount", "The array size cannot be less than 1." );
            if ( b == 0 )
                throw new DivideByZeroException ();
            return NextProbabilities ( (float) a / b, amount );
        }

        #endregion

        #region Enumerations

        /// <summary>
        /// Selects a random value from an <seealso cref="Enum"/>
        /// </summary>
        /// <typeparam name="T">The type of <seealso cref="Enum"/> being selected from.</typeparam>
        /// <returns>A random value of type <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public T ChooseEnum<T> () where T : Enum
        {

            T [] values = (T []) Enum.GetValues ( typeof ( T ) );

            List<T> newValues = new List<T> ();

            foreach ( T t in values )
            {
                for ( int i = 0; i < t.GetWeight (); i++ )
                    newValues.Add ( t );
            }

            return Choose ( newValues );
        }

        /// <summary>
        /// Fills an array of <typeparamref name="T"/> Enum values.
        /// </summary>
        /// <typeparam name="T">The type of <seealso cref="Enum"/> being selected from.</typeparam>
        /// <param name = "buffer" > The array to fill with random elements.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ChooseEnum<T> ( T [] buffer ) where T : Enum
        {
            T [] values = (T []) Enum.GetValues ( typeof ( T ) );

            List<T> newValues = new List<T> ();

            foreach ( T t in values )
            {
                for ( int i = 0; i < t.GetWeight (); i++ )
                    newValues.Add ( t );
            }

            Choose ( newValues, buffer );
        }

        /// <summary>
        /// Returns an array of random <typeparamref name="T"/> Enum values.
        /// </summary>
        /// <typeparam name="T">The type of <seealso cref="Enum"/> being selected from.</typeparam>
        /// <param name = "amount" >The size of array to create.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public T [] ChooseEnum<T> ( int amount ) where T : Enum
        {
            T [] values = (T []) Enum.GetValues ( typeof ( T ) );

            List<T> newValues = new List<T> ();

            foreach ( T t in values )
            {
                for ( int i = 0; i < t.GetWeight (); i++ )
                    newValues.Add ( t );
            }

            return Choose ( newValues, amount );
        }

        #endregion

        #region Internal

        private int NextSample ()
        {
            int locINext = m_INext;
            int locINextP = m_INextP;

            if ( ++locINext >= 56 )
                locINext = 1;

            if ( ++locINextP >= 56 )
                locINextP = 1;

            int retVal = m_SeedArray [ locINext ] - m_SeedArray [ locINextP ];

            if ( retVal == int.MaxValue )
                retVal--;

            if ( retVal < 0 )
                retVal += int.MaxValue;

            lock ( m_StateLock )
            {
                m_SeedArray [ locINext ] = retVal;
                m_INext = locINext;
                m_INextP = locINextP;
            }
            return retVal;
        }

        #endregion

        #region State

        /// <summary>
        /// Get the current state of the <seealso cref="ChaosEngine"/>.
        /// </summary>
        /// <returns></returns>
        public int [] SaveState ()
        {
            int [] state = new int [ 59 ];
            state [ 0 ] = m_Seed;
            state [ 1 ] = m_INext;
            state [ 2 ] = m_INextP;

            for ( int i = 3; i < state.Length; i++ )
                state [ i ] = m_SeedArray [ i - 3 ];
            return state;
        }

        /// <summary>
        /// Write the current state of the <seealso cref="ChaosEngine"/> to a <seealso cref="Stream"/>
        /// </summary>
        /// <param name="stream">The stream to write the state to.</param>
        /// <exception cref="NotSupportedException">The stream must be writable.</exception>
        public void SaveState ( Stream stream )
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter ();
                formatter.Serialize ( stream, SaveState () );
            }
            catch ( NotSupportedException e )
            {
                throw e;
            }
        }

        /// <summary>
        /// Load an existing <seealso cref="ChaosEngine"/> state to the instance.
        /// </summary>
        /// <param name="state">The state of a <seealso cref="ChaosEngine"/> to load.</param>
        /// <exception cref="Exception">state must be 59 elements in size.</exception>
        public void LoadState ( int [] state )
        {

            lock ( m_StateLock )
            {

                if ( state.Length != 59 || state [ 3 ] != 0 )
                    throw new Exception ( "Chaos state is invalid or corrupted." );

                m_Seed = state [ 0 ];
                m_INext = state [ 1 ];
                m_INextP = state [ 2 ];

                for ( int i = 3; i < state.Length; i++ )
                    m_SeedArray [ i - 3 ] = state [ i ];

            }

        }

        /// <summary>
        /// Load an existing <seealso cref="ChaosEngine"/> state from a <seealso cref="Stream"/>
        /// </summary>
        /// <param name="stream">The stream to read the state from.</param>
        /// <exception cref="NotSupportedException">The stream must be readable.</exception>
        /// <exception cref="Exception">state must be 59 elements in size.</exception>
        public void LoadState ( Stream stream )
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter ();
                int [] state = (int []) formatter.Deserialize ( stream );
                LoadState ( state );
            }
            catch ( NotSupportedException e )
            {
                throw e;
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string representation of the <seealso cref="ChaosEngine"/>
        /// </summary>
        /// <returns>A string representation of the <seealso cref="ChaosEngine"/></returns>
        public override string ToString () => $"[ChaosEngine:{m_Seed}]";

        #endregion

    }

}
