/** Matches the ProblemDetails shape returned by AuditIQ.Api's GlobalExceptionHandler. */
export interface ProblemDetails {
  status?: number
  title?: string
  detail?: string
  type?: string
  instance?: string
}
