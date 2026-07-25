EXTERNAL Event(eventName)

=== function Event(eventName)
// Fallback in case actual function is not available.
EVENT: {eventName}

=== function Get_State(id)
GET_STATE: {id}
~ return 0

=== function Add_State(id, amount)
SET_STATE: {id} - VALUE: {amount}

=== function Timer(time)
TIMER: {time}
