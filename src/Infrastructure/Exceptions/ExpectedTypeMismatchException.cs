namespace ZxenLib.Infrastructure.Exceptions
{
    using System;

    [Serializable]
    public class ExpectedTypeMismatchException : Exception
    {
        private const string EXPECTED_TYPE_MISMATCH_DEFAULT_MESSAGE = "The expected type does not match the actual type.";
        private const string FORMATTED_EXPECTED_TYPE_MISMATCH_DEFAULT_MESSAGE = "The expected type does not match the actual type. Expected: {0}, Actual: {1}";

        public string ActualTypeName { get; private set; }
        public string ExpectedTypeName { get; private set; }

        public ExpectedTypeMismatchException() : base(EXPECTED_TYPE_MISMATCH_DEFAULT_MESSAGE)
        {
        }

        public ExpectedTypeMismatchException(Type expectedType, string actualTypeName) : base(string.Format(FORMATTED_EXPECTED_TYPE_MISMATCH_DEFAULT_MESSAGE, expectedType.Name, actualTypeName))
        {
            this.ExpectedTypeName = expectedType.Name;
            this.ActualTypeName = actualTypeName;
        }
    }
}
