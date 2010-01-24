namespace SumControls
{
    using System;

    /// <summary>
    /// A collection of random usefuul functions. An instance of this class cannot be created.
    /// </summary>
    public static class Functions
    {
        /// <summary>
        /// Retrieves a Win32 LOWORD from the given pointer.
        /// </summary>
        /// <param name="value">The pointer to extract from</param>
        /// <returns>A LOWORD message</returns>
        public static int LoWord(IntPtr value)
        {
            try
            {
                var val32 = value.ToInt32();
                return val32 & 0xFFFF;
            }
            catch (OverflowException)
            {
                return 0;
            }
        }

        /// <summary>
        /// Retrieves a Win32 HIWORD from the given pointer
        /// </summary>
        /// <param name="value">The pointer to extract from</param>
        /// <returns>A HIWORD message</returns>
        public static int HiWord(IntPtr value)
        {
            try
            {
                var val32 = value.ToInt32();
                return (val32 >> 16) & 0xFFFF;
            }
            catch (OverflowException)
            {
                return 0;
            }
        }
    }
}