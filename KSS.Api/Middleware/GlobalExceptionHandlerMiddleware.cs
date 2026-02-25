using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KSS.Api.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        [DebuggerStepThrough]
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Check if this is a known SQL exception that we handle gracefully
                var sqlEx = FindSqlException(ex);
                var isHandledException = sqlEx != null || ex is DbUpdateException || ex is ArgumentException || ex is UnauthorizedAccessException || ex is KeyNotFoundException;
                
                // Only log unexpected exceptions
                if (!isHandledException)
                {
                    _logger.LogError(ex, "An unhandled exception occurred");
                }
                else
                {
                    // Log at debug level for handled exceptions (for troubleshooting)
                    _logger.LogDebug(ex, "Handled exception: {ExceptionType}", ex.GetType().Name);
                }
                
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            ErrorResponse errorResponse;

            // Handle SqlException whether it's the exception itself or wrapped (e.g. in DbUpdateException)
            var sqlEx = FindSqlException(exception);
            if (sqlEx != null)
            {
                errorResponse = HandleSqlException(sqlEx);
            }
            else switch (exception)
            {
                case DbUpdateException dbEx:
                    // Check for OUTPUT clause error (tables with triggers)
                    if (dbEx.Message.Contains("OUTPUT clause") || dbEx.Message.Contains("triggers") ||
                        (dbEx.InnerException != null && (dbEx.InnerException.Message.Contains("OUTPUT clause") || dbEx.InnerException.Message.Contains("triggers"))))
                    {
                        errorResponse = new ErrorResponse
                        {
                            StatusCode = (int)HttpStatusCode.BadRequest,
                            Message = "Cannot perform this operation because the table has database triggers. Entity Framework OUTPUT clause is disabled for this table.",
                            Details = dbEx.InnerException?.Message ?? dbEx.Message
                        };
                    }
                    else
                    {
                        errorResponse = new ErrorResponse
                        {
                            StatusCode = (int)HttpStatusCode.BadRequest,
                            Message = "A database error occurred while processing your request.",
                            Details = dbEx.Message
                        };
                    }
                    break;

                case ArgumentException argEx:
                    errorResponse = new ErrorResponse
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = argEx.Message,
                        Details = argEx.ParamName != null ? $"Parameter: {argEx.ParamName}" : null
                    };
                    break;

                case KeyNotFoundException knfEx:
                    errorResponse = new ErrorResponse
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = knfEx.Message,
                        Details = null
                    };
                    break;

                case UnauthorizedAccessException:
                    errorResponse = new ErrorResponse
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized,
                        Message = "You are not authorized to perform this action."
                    };
                    break;

                default:
                    errorResponse = new ErrorResponse
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        Message = "An unexpected error occurred while processing your request.",
                        Details = exception.Message
                    };
                    break;
            }

            if (!response.HasStarted)
            {
                response.StatusCode = errorResponse.StatusCode;
                var jsonResponse = JsonSerializer.Serialize(errorResponse);
                return response.WriteAsync(jsonResponse);
            }
            // If response has already started, we can't modify it
            // This shouldn't happen for our use case, but handle gracefully
            return Task.CompletedTask;
        }

        /// <summary>
        /// Finds SqlException in the exception chain (exception itself or any InnerException).
        /// Works for both Microsoft.Data.SqlClient and System.Data.SqlClient.
        /// </summary>
        private static Exception? FindSqlException(Exception ex)
        {
            for (var e = ex; e != null; e = e.InnerException)
            {
                if (e.GetType().Name == "SqlException")
                    return e;
            }
            return null;
        }

        private static ErrorResponse HandleSqlException(Exception sqlEx)
        {
            // SQL Server error codes:
            // 2601 = Cannot insert duplicate key row (unique index violation)
            // 2627 = Violation of UNIQUE KEY constraint
            // 547 = Foreign key constraint violation
            // 515 = Cannot insert NULL value
            // 8152 = String or binary data would be truncated

            // Get error number using reflection (works for both SqlException types)
            var numberProperty = sqlEx.GetType().GetProperty("Number");
            var errorNumber = numberProperty?.GetValue(sqlEx) as int? ?? 0;

            return errorNumber switch
            {
                2601 or 2627 => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Message = ExtractDuplicateKeyMessage(sqlEx),
                    Details = sqlEx.Message
                },
                547 => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "The operation violates a foreign key constraint. Please check that all referenced records exist.",
                    Details = sqlEx.Message
                },
                515 => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "A required field cannot be null.",
                    Details = sqlEx.Message
                },
                8152 => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ExtractTruncationMessage(sqlEx),
                    Details = sqlEx.Message
                },
                _ => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "A database error occurred while processing your request.",
                    Details = sqlEx.Message
                }
            };
        }

        private static string ExtractDuplicateKeyMessage(Exception sqlEx)
        {
            var message = sqlEx.Message;

            // Extract the constraint/index name and column names from the error message
            // Example: "Cannot insert duplicate key row in object 'dbo.Company' with unique index 'UX_Company_NationalId_Country'. The duplicate key value is (1, 1234567890)."

            if (message.Contains("UX_Company_NationalId_Country"))
            {
                return "A company with this National ID already exists in this country.";
            }
            if (message.Contains("UX_Company_RegistrationNo_Country"))
            {
                return "A company with this Registration Number already exists in this country.";
            }
            if (message.Contains("UX_Company_EconomicCode_Country"))
            {
                return "A company with this Economic Code already exists in this country.";
            }
            if (message.Contains("duplicate key"))
            {
                // Generic duplicate key message
                var match = Regex.Match(message, @"duplicate key value is \((.+?)\)");
                if (match.Success)
                {
                    return $"A record with these values already exists: {match.Groups[1].Value}";
                }
                return "A record with these values already exists. Please use different values.";
            }

            return "A duplicate record already exists. Please check your input and try again.";
        }

        private static string ExtractTruncationMessage(Exception sqlEx)
        {
            var message = sqlEx.Message;

            // Extract field name and truncated value from error message
            // Example: "String or binary data would be truncated in table 'KSS_Company_Prod.dbo.Company', column 'EconomicCode'. Truncated value: 'DUPLICATE17715858164'."

            var columnMatch = Regex.Match(message, @"column '(\w+)'");
            var valueMatch = Regex.Match(message, @"Truncated value: '([^']+)'");

            if (columnMatch.Success)
            {
                var columnName = columnMatch.Groups[1].Value;
                var truncatedValue = valueMatch.Success ? valueMatch.Groups[1].Value : "";
                
                // Map column names to user-friendly field names
                var fieldName = columnName switch
                {
                    "EconomicCode" => "Economic Code",
                    "RegistrationNo" => "Registration Number",
                    "NationalId" => "National ID",
                    "TaxId" => "Tax ID",
                    "Website" => "Website",
                    "LogoUrl" => "Logo URL",
                    _ => columnName
                };

                if (!string.IsNullOrEmpty(truncatedValue))
                {
                    return $"The value for '{fieldName}' is too long. Maximum length exceeded. Provided value length: {truncatedValue.Length} characters.";
                }
                else
                {
                    return $"The value for '{fieldName}' exceeds the maximum allowed length.";
                }
            }

            return "The data provided exceeds the maximum allowed length for one or more fields.";
        }
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}
