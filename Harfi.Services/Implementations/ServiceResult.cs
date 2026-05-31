using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.Services.Implementations
{
    /// <summary>
    /// Generic wrapper for service method results.
    /// 
    /// WHY THIS PATTERN?
    /// Instead of throwing exceptions for business rule failures
    /// (which is slow and semantically wrong — a duplicate review
    /// is not an "exception", it's an expected scenario),
    /// we return a result object that the Controller can inspect.
    /// 
    /// Usage in Service:
    ///   return ServiceResult<ReviewResponseDto>.Fail("الطلب غير موجود");
    ///   return ServiceResult<ReviewResponseDto>.Ok(responseDto);
    /// 
    /// Usage in Controller:
    ///   if (!result.Success) return BadRequest(result.Error);
    ///   return Ok(result.Data);
    /// </summary>
    public class ServiceResult<T>
    {
        /// <summary>
        /// True if operation succeeded, false if a business rule blocked it.
        /// </summary>
        public bool Success { get; private set; }
        /// <summary>
        /// The data to return to client on success. Null on failure.
        /// </summary>
        public T? Data { get; private set; }

        /// <summary>
        /// Arabic error message to return to client on failure. 
        /// Empty string on success.
        /// </summary>
        public string Error { get; private set; } = string.Empty;

        /// <summary>
        /// Creates a successful result containing data.
        /// Called when all business rules pass and data is saved.
        /// </summary>
        public static ServiceResult<T> Ok(T data) =>
            new() { Success = true, Data = data };

        /// <summary>
        /// Creates a failed result containing an error message.
        /// Called when any business rule fails.
        /// </summary>
        public static ServiceResult<T> Fail(string error) =>
            new() { Success = false, Error = error };
    }
}
