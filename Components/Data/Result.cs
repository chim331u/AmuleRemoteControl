namespace AmuleRemoteControl.Components.Data
{
    /// <summary>
    /// Represents the result of an operation that can either succeed with a value or fail with an error.
    /// This pattern provides explicit error handling without exceptions and makes success/failure states clear.
    /// </summary>
    /// <typeparam name="T">The type of the value returned on success</typeparam>
    /// <remarks>
    /// Usage examples:
    /// <code>
    /// // Success case
    /// return Result&lt;User&gt;.Success(user);
    ///
    /// // Failure case
    /// return Result&lt;User&gt;.Failure("User not found");
    ///
    /// // Pattern matching
    /// var result = GetUser(id);
    /// if (result.IsSuccess)
    /// {
    ///     Console.WriteLine($"Found user: {result.Value.Name}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Error: {result.Error}");
    /// }
    /// </code>
    /// </remarks>
    public class Result<T>
    {
        /// <summary>
        /// Gets a value indicating whether the operation succeeded.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the value if the operation succeeded, or default(T) if it failed.
        /// Always check IsSuccess before accessing this property.
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// Gets the error message if the operation failed, or null if it succeeded.
        /// </summary>
        public string? Error { get; }

        /// <summary>
        /// Gets the exception that caused the failure, if any.
        /// </summary>
        public Exception? Exception { get; }

        // Private constructor to enforce use of factory methods
        private Result(bool isSuccess, T? value, string? error, Exception? exception)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            Exception = exception;
        }

        /// <summary>
        /// Creates a successful result with the specified value.
        /// </summary>
        /// <param name="value">The value to return</param>
        /// <returns>A successful Result containing the value</returns>
        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, null, null);
        }

        /// <summary>
        /// Creates a failed result with the specified error message.
        /// </summary>
        /// <param name="error">The error message describing why the operation failed</param>
        /// <returns>A failed Result with the error message</returns>
        public static Result<T> Failure(string error)
        {
            return new Result<T>(false, default, error, null);
        }

        /// <summary>
        /// Creates a failed result with the specified error message and exception.
        /// </summary>
        /// <param name="error">The error message describing why the operation failed</param>
        /// <param name="exception">The exception that caused the failure</param>
        /// <returns>A failed Result with the error message and exception</returns>
        public static Result<T> Failure(string error, Exception exception)
        {
            return new Result<T>(false, default, error, exception);
        }

        /// <summary>
        /// Creates a failed result from an exception.
        /// </summary>
        /// <param name="exception">The exception that caused the failure</param>
        /// <returns>A failed Result with the exception's message</returns>
        public static Result<T> Failure(Exception exception)
        {
            return new Result<T>(false, default, exception.Message, exception);
        }

        /// <summary>
        /// Executes one of two functions based on the result state.
        /// </summary>
        /// <typeparam name="TResult">The return type of the functions</typeparam>
        /// <param name="onSuccess">Function to execute if the result is successful</param>
        /// <param name="onFailure">Function to execute if the result is a failure</param>
        /// <returns>The result of the executed function</returns>
        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value!) : onFailure(Error!);
        }

        /// <summary>
        /// Implicitly converts a value to a successful Result.
        /// </summary>
        public static implicit operator Result<T>(T value)
        {
            return Success(value);
        }

        /// <summary>
        /// Returns a string representation of the result.
        /// </summary>
        public override string ToString()
        {
            return IsSuccess
                ? $"Success: {Value}"
                : $"Failure: {Error}";
        }
    }
}
