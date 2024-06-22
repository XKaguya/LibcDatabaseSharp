namespace LibcDatabaseSharp.Generic
{
    public class Utils
    {
        public static bool TryParseLogLevel(string input, out Enum.LogLevel logLevel)
        {
            return System.Enum.TryParse(input, true, out logLevel) && System.Enum.IsDefined(typeof(Enum.LogLevel), logLevel);
        }
    }
}