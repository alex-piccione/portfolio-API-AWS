module error_messages

let mustBeDefined field = $"{field} must be defined"
let mustBeGreaterThanZero field = $"{field} must be greater than zero"
let mustBeInTheFuture field = $"{field} must be in the future"
let mustBeInThePast field = $"{field} must be in the past"

