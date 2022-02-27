namespace Portfolio.Core.Logic

open System

//type BalanceUpdateResult = 
//| Created
//| Updated
//| InvalidRequest of string

//type DeleteResult =
//| Deleted
//| DeleteInvalidRequest of string

//type CreateResult<'T> =
//| Created of 'T
//| CreateInvalidRequest of string

//type UpdateResult<'T> =
//| Updated of 'T
//| UpdateInvalidRequest of string

//type LogicResult<'a> =
//| Success

//type LogicExecutionResult<'T> =
//| Success of 'T
//| InvalidRequest of string

type ValidateResult =
| Valid 
| NotValid of string

type IIdGenerator =
    abstract member New: unit -> string

type IdGenerator () = 
    interface IIdGenerator with
        member this.New () = Guid.NewGuid().ToString()

type IChronos =
    abstract member Now: DateTime

type Chronos () =
    interface IChronos with 
        member this.Now = DateTime.UtcNow